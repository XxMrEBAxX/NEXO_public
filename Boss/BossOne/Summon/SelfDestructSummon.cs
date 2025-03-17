using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using DG.Tweening;
using UnityEngine.Splines;
using FMOD.Studio;
using FMODUnity;

namespace BirdCase
{
    enum ETargetPlayer
    {
        PLAYER_ONE,
        PLAYER_TWO
    }

    public class SelfDestructSummon : NetworkBehaviour, IAffectByExplosion, IBossGetDamage, IGetOffLauncher
    {
        public event Action DiedAction;
        public event Action<IGetOffLauncher> GetOffLauncher;

        public int CurrentGetDamage { get; set; }
        public ObjectSize GetObjectSize() => ObjectSize.SMALL;
        private SelfDestructManager summonManager;

        [SerializeField] private ETargetPlayer targetPlayer;
        [SerializeField] private ParticleSystem selfDestructSignalEffect;
        [SerializeField] private ParticleSystem selfDestructEffect;
        [SerializeField] private GameObject mesh;

        private SphereCollider sphereCollider;
        private SphereCollider sphereExplosionCollider;
        private Rigidbody rb;
        private GameObject target;
        private CancellationTokenSource cancel;
        private Vector3 originPosition;
        private int curSummonHealth;
        private bool canTracking = false;

        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] Ease moveType = Ease.Linear;
        private Tween tween;
        private HitFXHandler hitFXHandler;
        private EventInstance moveSoundInstance;

        Vector3[] interestDirections = new Vector3[8];

        private void Awake()
        {
            cancel = new CancellationTokenSource();
            transform.position = splineContainer.EvaluatePosition(0);
            originPosition = transform.position;
            sphereCollider = GetComponent<SphereCollider>();
            sphereExplosionCollider = selfDestructEffect.GetComponent<SphereCollider>();
            summonManager = transform.parent.GetComponent<SelfDestructManager>();
            hitFXHandler = GetComponentInChildren<HitFXHandler>();
            rb = GetComponent<Rigidbody>();

            for (int i = 0; i < interestDirections.Length; i++)
            {
                float angle = i * Mathf.PI / 4;
                interestDirections[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            }
        }

        public void Enable()
        {
            Init();
            SoundManager.Instance.Stop(moveSoundInstance);
            moveSoundInstance = SoundManager.Instance.Play(summonManager.bossOne.sound.summonMove, SoundManager.Banks.SFX, 1, transform.position);

            if (InGamePlayManager.Instance.IsDebugMode)
            {
                var player = FindFirstObjectByType<PlayerBase>();
                try
                {
                    target = player.gameObject;
                }
                catch
                {
                    Destruct().Forget();
                    return;
                }
            }
            else
            {
                target = InGamePlayManager.Instance.GetAllPlayer()[(int)targetPlayer].gameObject;
            }

            SetSelfDestructSummon();
        }

        private void OnDisable()
        {
            GetOffLauncher?.Invoke(this);
            cancel.Cancel();
            SetActiveSelfClientRpc(false);
            DiedAction?.Invoke();
            canTracking = false;
            sphereCollider.enabled = false;
            selfDestructSignalEffect.Stop();
            transform.position = originPosition;
            if (tween != null)
                tween.Kill();
        }

        private void Init()
        {
            cancel = new CancellationTokenSource();
            curSummonHealth = summonManager.bossOne.SummonData.SelfDestructHealth;
            selfDestructEffect.Stop();
            SetActiveSelfClientRpc(true);
            sphereCollider.enabled = true;
            rb.isKinematic = false;
        }

        private void Update()
        {
            if (gameObject.activeInHierarchy)
            {
                PLAYBACK_STATE state;
                if (moveSoundInstance.getPlaybackState(out state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
                {
                    Vector3 pos = transform.position;
                    pos.z = Camera.main.transform.position.z;
                    moveSoundInstance.set3DAttributes(pos.To3DAttributes());
                }
            }
            
            if (!IsServer)
                return;

            if (canTracking)
            {
                TrackingPlayer();
            }
        }

        private void OnCollisionStay(Collision other)
        {
            if (!IsServer)
                return;

            if (other.gameObject.CompareTag("Player"))
            {
                if (other.gameObject.TryGetComponent(out PlayerBase playerBase))
                {
                    playerBase.TakeDamage(summonManager.bossOne.SummonData.SelfDestructDamage);
                }
            }
        }

        private async void SetSelfDestructSummon()
        {
            try
            {
                MoveSpline(3);
                await DelaySystem(3);
                cancel.Token.ThrowIfCancellationRequested();

                canTracking = true;
                await DelaySystem(summonManager.bossOne.SummonData.SelfDestructTrackingTime);
                cancel.Token.ThrowIfCancellationRequested();

                await DestructSignal();
                cancel.Token.ThrowIfCancellationRequested();
            }
            catch { }
        }

        private void MoveSpline(float time)
        {
            tween = DOTween.To(() => 0f, x => transform.position = splineContainer.EvaluatePosition(x), 1f, time).SetEase(moveType);
        }

        private void TrackingPlayer()
        {
            if (!InGamePlayManager.Instance.IsDebugMode)
            {
                if (target.GetComponent<PlayerBase>().IsDead())
                {
                    target = InGamePlayManager.Instance.GetAllPlayer()[(int)targetPlayer ^ 1].gameObject;
                    if (target.GetComponent<PlayerBase>().IsDead())
                    {
                        DestructSignal().Forget();
                    }
                }
            }

            ContextBasedSteering();
        }

        float lastWeight = 0;
        Vector3 lastDirection = Vector3.zero;
        private void ContextBasedSteering()
        {
            if (target == null)
                return;

            Vector3 targetPosition = target.transform.position;
            targetPosition = new Vector3(targetPosition.x, targetPosition.y + 1.5f, targetPosition.z);
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.z = 0;

            float[] weightDirections = new float[interestDirections.Length];

            for (int i = 0; i < interestDirections.Length; i++)
            {
                weightDirections[i] = Vector3.Dot(interestDirections[i], direction);
            }

            if (Vector3.Distance(transform.position, target.transform.position) <= summonManager.bossOne.SummonData.SelfDestructPlayerTrackingMinDistance - 1)
            {
                for (int i = 0; i < interestDirections.Length; i++)
                {
                    weightDirections[i] = 1 - weightDirections[i];
                }
            }
            else if (Vector3.Distance(transform.position, target.transform.position) < summonManager.bossOne.SummonData.SelfDestructPlayerTrackingMinDistance)
            {
                rb.linearVelocity = Vector3.zero;
                return;
                // for (int i = 0; i < interestDirections.Length; i++)
                // {
                //     weightDirections[i] = 1 - Mathf.Abs(weightDirections[i]);
                // }
            }
            else
            {
                for (int i = 0; i < interestDirections.Length; i++)
                {
                    weightDirections[i] = 1 + weightDirections[i];
                }
            }

            Vector3 greatestDirection = Vector3.zero;
            float greatestWeight = 0;
            RaycastHit hit;
            float speed = summonManager.bossOne.SummonData.SelfDestructSpeed * Time.deltaTime + sphereCollider.bounds.size.x;
            int layerMask = LayerMask.GetMask("Ground") + LayerMask.GetMask("Summon");
            for (int i = 0; i < interestDirections.Length; i++)
            {
                bool forward = Physics.Raycast(transform.position, interestDirections[i], out hit, speed, layerMask);
                Vector3 sizeOffsetRay = Vector3.Cross(interestDirections[i], Vector3.forward).normalized * sphereCollider.bounds.size.x * 0.5f;
                Vector3 leftRayOrigin = transform.position - sizeOffsetRay;
                Vector3 rightRayOrigin = transform.position + sizeOffsetRay;
                bool left = Physics.Raycast(leftRayOrigin, interestDirections[i], out hit, speed, layerMask);
                bool right = Physics.Raycast(rightRayOrigin, interestDirections[i], out hit, speed, layerMask);

                if (forward || left || right)
                {
                    weightDirections[i] = 0;
                }

                if (weightDirections[i] > greatestWeight)
                {
                    greatestDirection = interestDirections[i];
                    greatestWeight = weightDirections[i];
                }
            }
            bool isBlocked = false;
            Vector3 sizeOffsetRay2 = Vector3.Cross(direction, Vector3.forward).normalized * sphereCollider.bounds.size.x * 0.5f;
            Vector3 leftRayOrigin2 = transform.position - sizeOffsetRay2;
            Vector3 rightRayOrigin2 = transform.position + sizeOffsetRay2;
            isBlocked = Physics.Raycast(transform.position, direction, out hit, speed, layerMask)
            || Physics.Raycast(leftRayOrigin2, direction, out hit, speed, layerMask)
            || Physics.Raycast(rightRayOrigin2, direction, out hit, speed, layerMask);

            if (greatestDirection == -lastDirection && isBlocked)
            {
                if (MathF.Abs(MathF.Abs(lastWeight) - MathF.Abs(greatestWeight)) < 0.2f)
                {
                    greatestDirection = lastDirection;
                }
            }
            if (greatestDirection != Vector3.zero)
            {
                lastDirection = greatestDirection;
                lastWeight = greatestWeight;
            }

            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, greatestDirection * summonManager.bossOne.SummonData.SelfDestructSpeed, Time.deltaTime * summonManager.bossOne.SummonData.SelfDestructSpeed);
        }

        private async UniTaskVoid Destruct()
        {
            PlaySelfDestructEffectClientRPC(transform.position);
            StopMoveSoundClientRPC();

            await UniTask.WaitForFixedUpdate();

            Collider[] hitColliders = Physics.OverlapSphere(selfDestructEffect.gameObject.transform.position,
                sphereExplosionCollider.radius * selfDestructEffect.transform.localScale.x);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].gameObject.CompareTag("Player"))
                {
                    if (hitColliders[i].TryGetComponent(out PlayerBase playerBase))
                    {
                        playerBase.TakeDamage(summonManager.bossOne.SummonData.SelfDestructDamage);
                    }
                }
            }

            ExplosionYShakeClientRPC();
            summonManager.bossOne.sound.PlaySummonExplosionClientRPC(transform.position);
            OnDisable();
        }

        private async UniTask DelaySystem(float delay)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancel.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void GetDamage(int damage, PlayerName playerName)
        {
            if (!canTracking)
                return;

            if (curSummonHealth >= 0)
            {
                int damageValue = curSummonHealth - damage >= 0 ? damage : curSummonHealth;
                if (playerName == PlayerName.Ria)
                    DataSaveManager.Instance.CurPlayData.BossDamagedByRia += damageValue;
                else
                    DataSaveManager.Instance.CurPlayData.BossDamagedByNia += damageValue;
            }
            
            curSummonHealth = Mathf.Clamp(curSummonHealth - damage, 0, summonManager.bossOne.SummonData.SelfDestructHealth);
            HitFXClientRPC();

            if (curSummonHealth == 0)
            {
                Destruct().Forget();
            }
        }

        private async UniTask DestructSignal()
        {
            if (!canTracking)
                return;

            StopMoveSoundClientRPC();
            canTracking = false;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
            PlaySelfDestructSignalEffectClientRPC();
            summonManager.bossOne.sound.PlaySummonExplosionSignalClientRPC(transform.position);
            SignalHitFX(summonManager.bossOne.SummonData.SelfDestructSignalTime).Forget();
            await DelaySystem(summonManager.bossOne.SummonData.SelfDestructSignalTime);
            Destruct().Forget();
        }

        private async UniTaskVoid SignalHitFX(float time)
        {
            float interval = 0.5f;
            float minInterval = 0.13f;
            float decrement = 0.07f;

            while (time > 0)
            {
                HitFXClientRPC();
                await UniTask.Delay((int)(interval * 1000), cancellationToken: cancel.Token);
                time -= interval;
                interval = Mathf.Max(minInterval, interval - decrement);
            }
        }

        public void GetNeutralize(int neutralize, PlayerName playerName)
        {
            return;
        }

        public void AffectByExplosion(Vector3 explosionCenterPosition,
            LauncherBaseData.ExplosionData explosionData, int damage, int neutralizeValue, PlayerName playerName)
        {
            GetDamage(damage, playerName);
            GetNeutralize(neutralizeValue, playerName);
        }

        #region ClientRpc

        [ClientRpc]
        private void PlaySelfDestructEffectClientRPC(Vector3 pos)
        {
            selfDestructEffect.gameObject.transform.position = pos;
            selfDestructEffect.Play();
            selfDestructSignalEffect.Stop();
        }

        [ClientRpc]
        private void PlaySelfDestructSignalEffectClientRPC()
        {
            selfDestructSignalEffect.Play();
        }

        [ClientRpc]
        public void SetActiveSelfClientRpc(bool active)
        {
            mesh.SetActive(active);
            sphereCollider.enabled = active;
            sphereExplosionCollider.enabled = active;
        }

        [ClientRpc]
        private void HitFXClientRPC()
        {
            hitFXHandler.Play();
        }

        [ClientRpc]
        private void ExplosionYShakeClientRPC()
        {
            try
            {
                Vector3 dir = PlayManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId).transform.position - transform.position;
                dir.z = 0;
                CameraManager.Instance.ExplosionYCameraShake(dir, 0.05f);
            }
            catch { }
        }

        [ClientRpc]
        private void StopMoveSoundClientRPC()
        {
            SoundManager.Instance.Stop(moveSoundInstance);
        }

        #endregion
    }
}