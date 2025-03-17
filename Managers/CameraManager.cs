using System.Collections;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using USingleton.Utility;

namespace BirdCase
{
    public class CameraManager : NetworkSingleton<CameraManager>
    {
        public enum CameraType
        {
            Player,
            LeftClash,
            RightClash
        }

        [SerializeField] private Volume hitVolume;
        [SerializeField] private Volume counterVolume;
        [SerializeField] private Volume grayscaleVolume;
        private Coroutine hitVignetteCoroutine;
        private Coroutine counterVignetteCoroutine;
        [SerializeField] private CinemachineImpulseSource explosionXImpulseSource;
        [SerializeField] private CinemachineImpulseSource explosionYImpulseSource;
        [SerializeField] private CinemachineImpulseSource clashImpulseSource;
        [SerializeField] private CinemachineImpulseSource recoilYImpulseSource;
        private CinemachineFollow cinemachineFollow;
        private CinemachineBrain cinemachineBrain;
        private Vector3 defaultPositionDamping;
        [SerializeField] private GameObject hud;

        private CinemachineCamera playerCamera;
        private CinemachineCamera leftClashCamera;
        public CinemachineCamera LeftClashCamera { get => leftClashCamera; private set => leftClashCamera = value; }
        private CinemachineCamera rightClashCamera;

        private bool isChangeGrayScaleVolume = false;

        public static bool IsActiveShake = true;

        public bool IgnoreTimeScale
        {
            get
            {
                return cinemachineBrain.IgnoreTimeScale;
            }
            set
            {
                cinemachineBrain.IgnoreTimeScale = value;
            }
        }

        public bool IsUIActive
        {
            get
            {
                return hud.activeSelf;
            }
            set
            {
                hud.SetActive(value);
            }
        }

        protected override void OnAwake()
        {
            if (hitVolume == null)
            {
                Debug.LogError("No global volume found in scene");
            }

            if (counterVolume == null)
            {
                Debug.LogError("No counter volume found in scene");
            }

            cinemachineFollow = GetComponent<CinemachineFollow>();
            cinemachineBrain = GetComponentInChildren<CinemachineBrain>();
            defaultPositionDamping = cinemachineFollow.TrackerSettings.PositionDamping;
            playerCamera = GetComponent<CinemachineCamera>();
            leftClashCamera = GameObject.Find("leftClashCamera")?.GetComponent<CinemachineCamera>();
            rightClashCamera = GameObject.Find("rightClashCamera")?.GetComponent<CinemachineCamera>();
        }

        private void Start()
        {
            // Register Messages
            Messenger.RemoveMessage("HitVignette");
            Messenger.RegisterMessage("HitVignette", HitVignette);
        }

        public void ChangeTimeDamping()
        {
            if (TimeManager.Instance.TimeScale != 0)
            {
                float timeValue = TimeManager.Instance.TimeScale / TimeManager.Instance.PlayerTimeScale;
                float x = Mathf.Clamp(defaultPositionDamping.x * timeValue, 0.15f, 15f);
                float y = Mathf.Clamp(defaultPositionDamping.y * timeValue, 0.2f, 20f);
                float z = Mathf.Clamp(defaultPositionDamping.z * timeValue, 0.2f, 20f);
                cinemachineFollow.TrackerSettings.PositionDamping = new Vector3(x, y, z);
            }
        }

        [ContextMenu("Hit Vignette")]
        public void HitVignette()
        {
            if (hitVignetteCoroutine != null)
            {
                StopCoroutine(hitVignetteCoroutine);
            }

            hitVignetteCoroutine = StartCoroutine(HitVignetteCoroutine(0.3f));
            ExplosionXCameraShake(UnityEngine.Random.Range(0, 2) == 0 ? Vector3.left : Vector3.right, 2);
        }

        private IEnumerator HitVignetteCoroutine(float time)
        {
            float t = 0;
            float lerpTime = time * 0.5f - time * 0.1f;
            while (t < lerpTime)
            {
                t += Time.deltaTime;
                hitVolume.weight = Mathf.Lerp(0, 1, t / lerpTime);
                yield return null;
            }
            yield return new WaitForSeconds(time * 0.2f);
            t = 0;
            while (t < lerpTime)
            {
                t += Time.deltaTime;
                hitVolume.weight = Mathf.Lerp(1, 0, t / lerpTime);
                yield return null;
            }
        }

        public void StartCounterVignette(float time)
        {
            if (counterVignetteCoroutine != null)
            {
                StopCoroutine(counterVignetteCoroutine);
            }

            counterVignetteCoroutine = StartCoroutine(StartCounterVignetteCoroutine(time));
        }

        public void StopCounterVignette(float time)
        {
            if (counterVignetteCoroutine != null)
            {
                StopCoroutine(counterVignetteCoroutine);
            }

            counterVignetteCoroutine = StartCoroutine(StopCounterVignetteCoroutine(time));
        }

        private IEnumerator StartCounterVignetteCoroutine(float time)
        {
            float t = 0;
            while (t < time)
            {
                t += TimeManager.Instance.GetUnscaledDeltaTime();
                counterVolume.weight = Mathf.Lerp(0, 1, t / time);
                yield return null;
            }
        }

        private IEnumerator StopCounterVignetteCoroutine(float time)
        {
            float t = 0;
            float weight = counterVolume.weight;
            while (t < time)
            {
                t += TimeManager.Instance.GetUnscaledDeltaTime();
                counterVolume.weight = Mathf.Lerp(weight, 0, t / time);
                yield return null;
            }
        }

        public void SetGrayScale(float time, bool grayScaleOn)
        {
            if (ReferenceEquals(grayscaleVolume, null))
                return;

            isChangeGrayScaleVolume = grayScaleOn;
            SetGrayScaleAsync(time, grayScaleOn).Forget();
        }

        private async UniTaskVoid SetGrayScaleAsync(float time, bool grayScaleOn)
        {
            float t = 0;

            float start = grayscaleVolume.weight;
            float end = grayScaleOn ? 1 : 0;

            while (t < time)
            {
                if (isChangeGrayScaleVolume != grayScaleOn)
                {
                    return;
                }

                t += TimeManager.Instance.GetUnscaledDeltaTime();
                grayscaleVolume.weight = Mathf.Lerp(start, end, t / time);

                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
            grayscaleVolume.weight = end;
        }

        public void ExplosionXCameraShake(Vector3 direction, float force)
        {
            if (ReferenceEquals(explosionXImpulseSource, null) || !IsActiveShake)
                return;

            if (TimeManager.Instance.TimeScale < TimeManager.Instance.OriginTimeScale)
            {
                force *= Mathf.Lerp(0.7f, 1, TimeManager.Instance.TimeScale);
            }

            explosionXImpulseSource.DefaultVelocity = direction;
            explosionXImpulseSource.GenerateImpulseWithForce(force);
        }

        public void ExplosionYCameraShake(Vector3 direction, float force)
        {
            if (ReferenceEquals(explosionYImpulseSource, null) || !IsActiveShake)
                return;

            if (TimeManager.Instance.TimeScale < TimeManager.Instance.OriginTimeScale)
            {
                force *= Mathf.Lerp(0.7f, 1, TimeManager.Instance.TimeScale);
            }

            explosionYImpulseSource.DefaultVelocity = direction;
            explosionYImpulseSource.GenerateImpulseWithForce(force);
        }

        public void RecoilYCameraShake(float force)
        {
            if (ReferenceEquals(recoilYImpulseSource, null) || !IsActiveShake)
                return;

            if (TimeManager.Instance.TimeScale < TimeManager.Instance.OriginTimeScale)
            {
                force *= Mathf.Lerp(0.7f, 1, TimeManager.Instance.TimeScale);
            }

            recoilYImpulseSource.GenerateImpulseWithForce(force);
        }

        public void ClashCameraShake(float force)
        {
            if (ReferenceEquals(clashImpulseSource, null) || !IsActiveShake)
                return;

            clashImpulseSource.GenerateImpulseWithForce(force);
        }

        public void ChangeCamera(CameraType cameraType)
        {
            switch (cameraType)
            {
                case CameraType.Player:
                    playerCamera.Priority = 1;
                    leftClashCamera.Priority = 0;
                    rightClashCamera.Priority = 0;
                    break;
                case CameraType.LeftClash:
                    leftClashCamera.Priority = 1;
                    rightClashCamera.Priority = 0;
                    playerCamera.Priority = 0;
                    break;
                case CameraType.RightClash:
                    rightClashCamera.Priority = 1;
                    playerCamera.Priority = 0;
                    leftClashCamera.Priority = 0;
                    break;
                default:
                    break;
            }
        }
    }
}
