using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Splines;
using DG.Tweening;
using Unity.Netcode.Components;
using FMOD.Studio;
using FMODUnity;

namespace BirdCase
{
    public class StaticElectricitySummon : NetworkBehaviour, IBossGetDamage, IAffectByExplosion, IGetOffLauncher
    {
        public event Action DiedAction;
        public event Action<IGetOffLauncher> GetOffLauncher;

        public int CurrentGetDamage { get; set; }
        public ObjectSize GetObjectSize() => ObjectSize.SMALL;
        public Collider areaCollider;
        public StaticElectricitySummonManager summonManager;
        public Vector3 targetPosition;

        [SerializeField] private ParticleSystem[] dangerEffect;
        [SerializeField] private StaticElectricityLaser[] laserEffect;

        private Vector3 originPosition;
        private CancellationTokenSource cancel;

        private int curSummonHealth;

        private bool canMoveSummon = true;
        private bool canRollingThunder = false;

        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] Ease moveType = Ease.Linear;
        private Tween tween;
        private DamageArea damageArea;
        private NetworkAnimator networkAnimator;
        [SerializeField] private EffectMaterialSwitch[] effectMaterialSwitch;
        private EffectTimingToolManager foreshadowEffectTimingToolManager;

        [SerializeField] private ParticleSystem selfDestructSignalEffect;
        [SerializeField] private ParticleSystem selfDestructEffect;
        private SphereCollider sphereExplosionCollider;
        [SerializeField] private ParticleSystem foreshadowEffect;

        private HitFXHandler hitFXHandler;
        private HitFXHandler counterFXHandler;
        private EventInstance moveSoundInstance;
        private EventInstance[] laserFireInstance;

        private void Awake()
        {
            cancel = new CancellationTokenSource();
            transform.position = splineContainer.EvaluatePosition(0);
            originPosition = transform.position;
            damageArea = GetComponentInChildren<DamageArea>();
            networkAnimator = GetComponent<NetworkAnimator>();
            foreshadowEffectTimingToolManager = GetComponent<EffectTimingToolManager>();
            sphereExplosionCollider = selfDestructEffect.GetComponent<SphereCollider>();
            hitFXHandler = GetComponentInChildren<HitFXHandler>();
            counterFXHandler = GetComponent<HitFXHandler>();
            networkAnimator.Animator.keepAnimatorStateOnDisable = true;

            laserFireInstance = new EventInstance[laserEffect.Length];
        }

        private void Start()
        {
            foreshadowEffectTimingToolManager.SetFixedTime(summonManager.bossOne.SummonData.StaticElectricityAttackDelay);
        }

        public void SetForeshadowMaterial(Material material)
        {
            for (int i = 0; i < effectMaterialSwitch.Length; i++)
            {
                effectMaterialSwitch[i].material = material;
            }
        }

        public void Enable()
        {
            Init();
            SetSummon();
            SoundManager.Instance.Stop(moveSoundInstance);
            moveSoundInstance = SoundManager.Instance.Play(summonManager.bossOne.sound.summonMove, SoundManager.Banks.SFX, 1, transform.position);
            if (IsServer)
                curSummonHealth = summonManager.bossOne.SummonData.StaticElectricityHealth;
        }

        private void OnDisable()
        {
            Init();
            cancel.Cancel();
            StopLaserEffectClientRpc();
            GetOffLauncher?.Invoke(this);
            if (tween != null)
                tween.Kill();
            if (DiedAction != null)
                DiedAction.Invoke();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            cancel.Cancel();
            cancel.Dispose();
        }

        private void Init()
        {
            cancel.Dispose();
            cancel = new CancellationTokenSource();
            canMoveSummon = false;
            canRollingThunder = false;
            transform.position = originPosition;
            targetPosition = originPosition;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            networkAnimator.SetTrigger("Idle");
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

            if (laserEffect[0].IsActive)
            {
                for (int i = 0; i < laserEffect.Length; i++)
                {
                    PLAYBACK_STATE state;
                    if (laserFireInstance[i].getPlaybackState(out state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
                    {
                        Vector3 pos = laserEffect[i].BeamHitEffect.transform.position;
                        pos.z = Camera.main.transform.position.z;
                        laserFireInstance[i].set3DAttributes(pos.To3DAttributes());
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer)
                return;

            if (canMoveSummon)
            {
                MoveSummon();
            }

            if (canRollingThunder)
            {
                transform.Rotate(summonManager.Direction * Vector3.forward *
                                 (summonManager.bossOne.SummonData.StaticElectricityRotateSpeed * -10 * Time.deltaTime));
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (other.gameObject.TryGetComponent(out PlayerBase playerBase))
                {
                    playerBase.TakeDamage(summonManager.bossOne.SummonData.StaticElectricityDamage);
                }
            }
        }

        private async UniTask DelaySystem(float delay)
        {
            await UniTask.Delay((int)(delay * 1000), cancellationToken: cancel.Token);
        }

        float curTime = 0;
        float time = 0.5f;
        Vector3 currentVelocity = Vector3.zero;
        private void MoveSummon()
        {
            if (curTime + time < Time.time)
            {
                SetNewTargetPosition();
                curTime = Time.time;
            }
            else
            {
                Physics.Linecast(transform.position,
                    transform.position + targetPosition * (summonManager.bossOne.SummonData.StaticElectricitySpeed * Time.deltaTime),
                    out RaycastHit hit, LayerMask.GetMask("Ground"));
                if (hit.collider != null)
                {
                    SetNewTargetPosition();
                }
                else
                {
                    float directionChangeSpeed = 3;
                    currentVelocity = Vector2.Lerp(currentVelocity.normalized, targetPosition, directionChangeSpeed * Time.deltaTime) * summonManager.bossOne.SummonData.StaticElectricitySpeed;

                    // 적을 현재 속도 방향으로 이동
                    //transform.position += currentVelocity * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, currentVelocity, summonManager.bossOne.SummonData.StaticElectricitySpeed * Time.deltaTime);
                    if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
                    {
                        SetNewTargetPosition();
                    }
                }
            }
        }

        private void SetNewTargetPosition()
        {
            Bounds areaBounds = areaCollider.bounds;

            Vector3 otherPos = summonManager.GetSummonPosition(0);
            if (otherPos == transform.position)
            {
                otherPos = summonManager.GetSummonPosition(1);
            }

            int maxTryCount = 50;
            int tryCount = 0;
            do
            {
                float randomX = UnityEngine.Random.Range(areaBounds.min.x, areaBounds.max.x);
                float randomY = UnityEngine.Random.Range(areaBounds.min.y, areaBounds.max.y);
                targetPosition = Vector3.Lerp(Vector3.zero, new Vector3(randomX, randomY, splineContainer.EvaluatePosition(1).z),
                    UnityEngine.Random.Range(0.1f, 1f));
                tryCount++;
            } while (Vector3.Distance(targetPosition, otherPos) < 20 && tryCount < maxTryCount);
        }

        private const string TILING_PROPERTY = "_Main_Tiling";
        private async void SetSummon()
        {
            Vector4 vector4 = summonManager.copiedForeshadowMaterial.GetVector(TILING_PROPERTY);
            vector4.x = summonManager.Direction * Mathf.Abs(summonManager.copiedForeshadowMaterial.GetVector(TILING_PROPERTY).x);
            summonManager.copiedForeshadowMaterial.SetVector(TILING_PROPERTY, vector4);

            if (!IsServer)
                return;

            try
            {
                MoveSpline(2);
                await DelaySystem(2);
                cancel.Token.ThrowIfCancellationRequested();

                SetCounterFXClientRPC(true);
                canMoveSummon = true;
                await DelaySystem(summonManager.bossOne.SummonData.StaticElectricityMoveDelay); // 자리를 잡은 후 일정 시간동안은 움직이도록 설정
                StopMoveSoundClientRPC();
                canMoveSummon = false;
                cancel.Token.ThrowIfCancellationRequested();

                summonManager.bossOne.sound.PlayForeshadowClientRPC(transform.position);
                summonManager.bossOne.sound.PlayStaticChargeClientRPC(transform.position);
                PlayDangerAreaManagerClientRpc();
                PlayChargingEffectClientRpc();
                networkAnimator.SetTrigger("Laser");
                await DelaySystem(summonManager.bossOne.SummonData.StaticElectricityAttackDelay);
                cancel.Token.ThrowIfCancellationRequested();

                canRollingThunder = true;
                PlayLaserEffectClientRpc();
                await DelaySystem(0.3f);
                damageArea.IsAttack = true;
                await DelaySystem(summonManager.bossOne.SummonData.StaticElectricityLaserAttackTime - 0.3f);
                damageArea.IsAttack = false;
                canRollingThunder = false;
                cancel.Token.ThrowIfCancellationRequested();
                StopLaserEffectClientRpc();

                DeactivateSummon().Forget();
            }
            catch { }
        }

        private void MoveSpline(float time)
        {
            tween = DOTween.To(() => 0f, x => transform.position = splineContainer.EvaluatePosition(x), 1f, time).SetEase(moveType);
        }

        private async UniTaskVoid DeactivateSummon()
        {
            networkAnimator.SetTrigger("Idle");
            await DelaySystem(1);

            summonManager.bossOne.sound.PlaySummonExplosionSignalClientRPC(transform.position);
            PlaySelfDestructSignalEffectClientRPC();
            SignalHitFX(summonManager.bossOne.SummonData.SelfDestructSignalTime).Forget();
            await DelaySystem(summonManager.bossOne.SummonData.SelfDestructSignalTime);
            Destruct();
        }

        private void Destruct()
        {
            SetCounterFXClientRPC(false);
            PlaySelfDestructEffectClientRPC(transform.position);
            summonManager.bossOne.sound.PlaySummonExplosionClientRPC(transform.position);
            StopMoveSoundClientRPC();

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
            SetActiveSelfClientRpc(false);
            ExplosionYShakeClientRPC();
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

        #region ClientRpc

        [ClientRpc]
        private void ExplosionYShakeClientRPC()
        {
            Vector3 dir = PlayManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId).transform.position - transform.position;
            dir.z = 0;
            CameraManager.Instance.ExplosionYCameraShake(dir, 0.05f);
        }

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
        private void StopMoveSoundClientRPC()
        {
            SoundManager.Instance.Stop(moveSoundInstance);
        }

        /// <summary>
        /// checkDangerArea가 true일 경우 공격 범위를 표시합니다.
        /// </summary>
        [ClientRpc]
        private void PlayDangerAreaManagerClientRpc()
        {
            for (int i = 0; i < dangerEffect.Length; i++)
            {
                dangerEffect[i].Play();
            }
            foreshadowEffect.Play();
        }

        [ClientRpc]
        private void PlayChargingEffectClientRpc()
        {
            for (int i = 0; i < laserEffect.Length; i++)
            {
                laserEffect[i].PlayCharging();
            }
        }

        [ClientRpc]
        private void PlayLaserEffectClientRpc()
        {
            for (int i = 0; i < laserEffect.Length; i++)
            {
                laserEffect[i].PlayLaser();
                laserFireInstance[i] = SoundManager.Instance.Play(summonManager.bossOne.sound.staticElectricityFire, SoundManager.Banks.SFX, 1, transform.position);
            }
        }

        [ClientRpc]
        private void StopLaserEffectClientRpc()
        {
            for (int i = 0; i < laserEffect.Length; i++)
            {
                laserEffect[i].StopLaser();
                SoundManager.Instance.SetParameter(laserFireInstance[i], "StaticEnd", 1);
            }
            for (int i = 0; i < dangerEffect.Length; i++)
            {
                dangerEffect[i].Stop();
            }
            foreshadowEffect.Stop();
        }

        public void GetDamage(int damage, PlayerName playerName)
        {
            if (selfDestructSignalEffect.IsAlive(true))
                return;

            if (curSummonHealth >= 0)
            {
                int damageValue = curSummonHealth - damage >= 0 ? damage : curSummonHealth;
                if (playerName == PlayerName.Ria)
                    DataSaveManager.Instance.CurPlayData.BossDamagedByRia += damageValue;
                else
                    DataSaveManager.Instance.CurPlayData.BossDamagedByNia += damageValue;
            }

            curSummonHealth = Mathf.Clamp(curSummonHealth - damage, 0,
                summonManager.bossOne.SummonData.StaticElectricityHealth);
            HitFXClientRPC();
            if (curSummonHealth == 0)
            {
                Destruct();
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

        [ClientRpc]
        public void SetActiveSelfClientRpc(bool active)
        {
            gameObject.SetActive(active);
            if (active)
                Enable();
        }

        [ClientRpc]
        private void HitFXClientRPC()
        {
            hitFXHandler.Play();
        }

        [ClientRpc]
        private void SetCounterFXClientRPC(bool active)
        {
            if (active)
                counterFXHandler.Play();
            else
                counterFXHandler.Stop();
        }

        #endregion
    }
}