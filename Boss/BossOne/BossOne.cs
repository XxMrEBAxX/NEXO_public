using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Threading;
using System.Collections;
using Unity.Netcode.Components;
using UnityEngine.Playables;
using USingleton.Utility;

namespace BirdCase
{
    public enum EBossParts
    {
        RIGHT_ARM,
        LEFT_ARM,
        HEAD
    }

    public enum EBossPattern
    {
        LEFT_HAND_LASER,
        RIGHT_HAND_LASER,
        LEFT_DOWN,
        RIGHT_DOWN,
        DOWN_TARGET,
        LEFT_GRAB,
        RIGHT_GRAB,
        LEFT_SWING,
        RIGHT_SWING,
        ELECTRONIC,
        ELECTRIC_WIRE,
        LEFT_COUNTER,
        RIGHT_COUNTER
    }

    public enum EBossSummonPattern
    {
        STATIC_ELECTRICITY,
        SELF_DESTRUCT,
        LEFT_HEAD_LASER,
        RIGHT_HEAD_LASER
    }

    public enum EBossSpecialPattern
    {
        CLASH_ATTACK,
        PHASE2,
        LASTPURY,
        GAMEOVER
    }

    public class BossOne : BossBase
    {
        public int currentGetDamage { get; set; }
        public GameObject Target;

        public BossParts LeftBossParts { get; private set; }
        public BossParts RightBossParts { get; private set; }
        //public BossPatternPositionManager BossPatternPosManager { get; private set; }

        private NetworkAnimator networkAnimator;

        [SerializeField] private BossOneWeightData bossWeightData;
        [SerializeField] private CalculateArea calculateArea;

        private WeightedRandom<EBossPattern> WeightedFirstRandomClass;
        private WeightedRandom<EBossPattern>[] WeightedOneRandomClass;
        private WeightedRandom<EBossPattern>[] WeightedTwoRandomClass;
        private WeightedRandom<EBossPattern>[] WeightedThreeRandomClass;
        private WeightedRandom<EBossPattern>[] WeightedFourRandomClass;

        private CancellationTokenSource cancel;
        private Tween currentTween;
        bool isNotGetNeutralize = false;
        private HitFXHandler hitFXHandler;
        private ClashManager clashManager;
        [SerializeField] private GameObject leftClashPos;
        [SerializeField] private GameObject rightClashPos;
        private ClashBar clashBar;
        [HideInInspector] public EBossParts lastDamagedBossParts = EBossParts.LEFT_ARM;

        [Space(10)]
        [SerializeField] private ParticleSystem leftClashParticle;
        [SerializeField] private ParticleSystem rightClashParticle;
        [SerializeField] private ParticleSystem leftClashParticle2;
        [SerializeField] private ParticleSystem rightClashParticle2;
        [SerializeField] private ParticleSystem clashSuccessParticle;
        [SerializeField] private ParticleSystem clashSuccessParticle2;
        [SerializeField] private ParticleSystem clashFailedParticle;
        [SerializeField] private ParticleSystem clashFailedParticle2;

        [Space(10)]

        [SerializeField] private Transform headBone;
        public Transform HeadBone => headBone;
        [SerializeField] private HeadLaser laserEffect;
        public HeadLaser LaserEffect => laserEffect;
        [SerializeField] private DamageArea headLaserDamageArea;
        public DamageArea HeadLaserDamageArea => headLaserDamageArea;

        public MeshDemolisherTool leftPlatForm;
        public MeshDemolisherTool rightPlatForm;
        public BossSound sound { get; private set; }

        private InGamePlayManager inGamePlayManager;
        [SerializeField] private GameObject magicaWindZone;

        [SerializeField] private BackgroundAnimation backgroundAnimation;
        public BackgroundAnimation BackgroundAnimation => backgroundAnimation;

        protected override void Awake()
        {
            base.Awake();
            networkAnimator = GetComponent<NetworkAnimator>();
            sound = GetComponent<BossSound>();
            cancel = new CancellationTokenSource();

            CalculateWeightData();

            foreach (BossParts part in GetComponentsInChildren<BossParts>())
            {
                if (part.BossPart == EBossParts.LEFT_ARM)
                {
                    LeftBossParts = part;
                }
                else if (part.BossPart == EBossParts.RIGHT_ARM)
                {
                    RightBossParts = part;
                }
            }

            staticElectricitySummonManager = FindFirstObjectByType<StaticElectricitySummonManager>();
            selfDestructManager = FindFirstObjectByType<SelfDestructManager>();
            electricWireSummonManager = FindFirstObjectByType<ElectricWireSummonManager>();
            hitFXHandler = GetComponentInChildren<HitFXHandler>();
            dissolveVFXHandler = GetComponentInChildren<DissolveVFXHandler>();
            clashManager = FindFirstObjectByType<ClashManager>();
            clashBar = FindFirstObjectByType<ClashBar>();
            waveCollider = FindFirstObjectByType<WaveCollider>();

            dissolveHandler.localScale = new Vector3(0, 0, 0);
        }
        protected override void Start()
        {
            base.Start();
            leftClashPos.SetActive(false);
            rightClashPos.SetActive(false);
            clashBar.gameObject.SetActive(false);
            DangerLaser.GetComponent<EffectTimingToolManager>().SetFixedTime(BossData.ElectricLaserDangerLaserTime);
            ElectronicLaser.GetComponent<EffectTimingToolManager>().SetFixedTime(BossData.ElectricLaserAttackTime);
            inGamePlayManager = InGamePlayManager.Instance as InGamePlayManager;
            clashBar.subtractValue = BossData.ClashSubtractValue;

            Messenger.RemoveMessage("Dead");
            Messenger.RegisterMessage("Dead", ShowHelpFirstDead);
        }

        private void OnDisable()
        {
            if (cancel != null)
            {
                cancel.Cancel();
                cancel.Dispose();
                cancel = null;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnDisable();
        }

        protected override void Update()
        {
            base.Update();

#if UNITY_EDITOR
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                GetDamage(50000, PlayerName.Ria);
            }

            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                GetNeutralize(50000, PlayerName.Ria);
            }
#endif

            if (Keyboard.current.spaceKey.wasPressedThisFrame && (leftClashPos.activeInHierarchy || rightClashPos.activeInHierarchy))
            {
                CheckClashInputServerRPC(NetworkManager.LocalClientId);
            }

            if (phase2BackgroundScroll)
            {
                float size = Mathf.Lerp(curSize, minSize, (inGamePlayManager.PlayTime - entryTime) / (inGamePlayManager.GameEndPlayTime - entryTime));
                skydome.localScale = new Vector3(size, size, size);
            }

            //ANCHOR - 여기서부턴 서버 환경
            if (!IsServer)
                return;

            if (!inGamePlayManager.IsCutSceneEndBoth)
                return;

            if (!isAppear)
            {
                isAppear = true;
                isBtPaused = true;
                StartCoroutine(AppearCoroutine());
            }

            if (!isBtPaused)
            {
                tree.Update();
            }
        }

        private bool isAppear = false;
        [SerializeField] private PlayableDirector appearDirector;
        private IEnumerator AppearCoroutine()
        {
            PlayAppearDirectorClientRPC();
            LeftBossParts.transform.position = LeftBossParts.handPositionManager.OriginPosition.position + Vector3.down * 30;
            RightBossParts.transform.position = RightBossParts.handPositionManager.OriginPosition.position + Vector3.down * 30;
            networkAnimator.SetTrigger("Appear");
            yield return new WaitForSeconds(1.6f);
            sound.PlayBossRoarClientRPC();
            yield return new WaitForSeconds(3.5f);
            LeftBossParts.MoveOriginPosition(3);
            RightBossParts.MoveOriginPosition(3);
            yield return new WaitForSeconds(3.9f);
            isBtPaused = false;
            inGamePlayManager.ActiveTimer = true;
            ActiveBossAppearClientRPC();
        }

        [ClientRpc]
        private void ActiveBossAppearClientRPC()
        {
            inGamePlayManager.IsBossAppear = true;
        }

        [ClientRpc]
        private void PlayAppearDirectorClientRPC()
        {
            appearDirector.Play();
            CameraManager.Instance.IsUIActive = false;
        }

        public void EndAppearDirectorReceiver()
        {
            CameraManager.Instance.IsUIActive = true;
            appearDirector.Stop();
        }

        [ClientRpc]
        public void SetPlayerPositionClientRPC(ulong clientId, Vector3 position)
        {
            if (InGamePlayManager.Instance.IsDebugMode)
            {
                FindObjectsByType<PlayerBase>(FindObjectsSortMode.None)[0].transform.position = position;
            }
            else
            {
                InGamePlayManager.Instance.GetPlayer(clientId).transform.position = position;
            }
        }

        private int targetIndex = -1;
        private int sameTargetCount = 0;

        public void FindTarget()
        {
            if (InGamePlayManager.Instance.IsDebugMode)
            {
                if (Target != null)
                    return;
                try
                {
                    Target = FindObjectsByType<PlayerBase>(FindObjectsSortMode.None)[0].gameObject;
                }
                catch
                {
                    Debug.LogError("어그로를 찾을 수 없습니다.");
                }
            }
            else
            {
                int index = UnityEngine.Random.Range(0, 2);
                if (sameTargetCount > 0 && targetIndex == index)
                    index = UnityEngine.Random.Range(0, 2);
                if (targetIndex == index)
                    sameTargetCount++;
                try
                {
                    Target = InGamePlayManager.Instance.GetAllPlayer()[index].gameObject;
                }
                catch
                {
                    Debug.LogError("어그로를 찾을 수 없습니다.");
                }
            }
        }

        private event Action patternCompleteCallback;
        public void BossPattern(EBossPattern eBossPattern, Action completeCallback)
        {
            patternCompleteCallback = completeCallback;

            if (eBossPattern == EBossPattern.ELECTRIC_WIRE && electricWireSummonManager.IsSummoning)
            {
                PatternComplete();
                return;
            }

            Debug.Log("Boss Pattern : " + eBossPattern);

            switch (eBossPattern)
            {
                case EBossPattern.LEFT_SWING:
                    SwingAttack();
                    break;
                case EBossPattern.RIGHT_SWING:
                    SwingAttack(true);
                    break;
                case EBossPattern.LEFT_HAND_LASER:
                    HandLaserAttack();
                    break;
                case EBossPattern.RIGHT_HAND_LASER:
                    HandLaserAttack(true);
                    break;
                case EBossPattern.ELECTRONIC:
                    ElectronicAttack().Forget();
                    break;
                case EBossPattern.LEFT_GRAB:
                    GrabAttack();
                    break;
                case EBossPattern.RIGHT_GRAB:
                    GrabAttack(true);
                    break;
                case EBossPattern.LEFT_DOWN:
                    DownAttack();
                    break;
                case EBossPattern.RIGHT_DOWN:
                    DownAttack(true);
                    break;
                case EBossPattern.DOWN_TARGET:
                    DownTargetAttack();
                    break;
                case EBossPattern.ELECTRIC_WIRE:
                    ElectricWireAttack().Forget();
                    break;
                case EBossPattern.LEFT_COUNTER:
                    CounterAttack();
                    break;
                case EBossPattern.RIGHT_COUNTER:
                    CounterAttack(true);
                    break;
                default:
                    break;
            }
        }
        private event Action summonPatternCompleteCallback;
        public void BossSummonPattern(EBossSummonPattern eBossSummonPattern, Action completeCallback)
        {
            Debug.Log("Boss Summon Pattern : " + eBossSummonPattern);

            summonPatternCompleteCallback = completeCallback;
            switch (eBossSummonPattern)
            {
                case EBossSummonPattern.LEFT_HEAD_LASER:
                    HeadLaser(false);
                    break;
                case EBossSummonPattern.RIGHT_HEAD_LASER:
                    HeadLaser(true);
                    break;
                case EBossSummonPattern.STATIC_ELECTRICITY:
                    StaticElectricity().Forget();
                    break;
                case EBossSummonPattern.SELF_DESTRUCT:
                    SelfDestruct().Forget();
                    break;
                default:
                    break;
            }
        }

        private event Action patternSpecialCompleteCallback;
        public void BossSpecialPattern(EBossSpecialPattern eBossSpecialPattern, Action completeCallback)
        {
            Debug.Log("Boss Special Pattern : " + eBossSpecialPattern);

            patternSpecialCompleteCallback = completeCallback;
            switch (eBossSpecialPattern)
            {
                case EBossSpecialPattern.CLASH_ATTACK:
                    ClashAttack();
                    break;
                case EBossSpecialPattern.PHASE2:
                    Phase2();
                    break;
                case EBossSpecialPattern.LASTPURY:
                    LastPury();
                    break;
                case EBossSpecialPattern.GAMEOVER:
                    GameOver();
                    break;
                default:
                    break;
            }
        }

        public void PlayHandPatternBodyAnimation(string trigger)
        {
            if (IsPhase2)
                return;

            networkAnimator.SetTrigger(trigger);
        }

        private void SwingAttack(bool isRight = false)
        {
            if (isRight)
            {
                PlayHandPatternBodyAnimation("SwingRight");
                RightBossParts.SwingAttack();
            }
            else
            {
                PlayHandPatternBodyAnimation("SwingLeft");
                LeftBossParts.SwingAttack();
            }
        }

        private void GrabAttack(bool isRight = false)
        {
            if (isRight)
            {
                PlayHandPatternBodyAnimation("GrabRight");
                RightBossParts.GrabAttack();
            }
            else
            {
                PlayHandPatternBodyAnimation("GrabLeft");
                LeftBossParts.GrabAttack();
            }
        }

        private void DownAttack(bool isRight = false)
        {
            if (isRight)
            {
                PlayHandPatternBodyAnimation("DownRight");
                RightBossParts.DownAttack();
            }
            else
            {
                PlayHandPatternBodyAnimation("DownLeft");
                LeftBossParts.DownAttack();
            }
        }

        private void CounterAttack(bool isRight = false)
        {
            SetIsNotNeutralizeChangeClientRPC(true);
            if (isRight)
            {
                PlayHandPatternBodyAnimation("CounterRight");
                RightBossParts.CounterAttack();
            }
            else
            {
                PlayHandPatternBodyAnimation("CounterLeft");
                LeftBossParts.CounterAttack();
            }
        }

        private void DownTargetAttack()
        {
            if (InGamePlayManager.Instance.IsDebugMode)
            {
                PlayerBase[] playerBase = FindObjectsByType<PlayerBase>(FindObjectsSortMode.None);
                if (playerBase.Length > 0)
                {
                    PlayHandPatternBodyAnimation("DownTarget");
                    LeftBossParts.DownTargetAttack(playerBase[0]);
                }
                else
                {
                    Debug.LogError("플레이어를 찾을 수 없습니다.");
                    PatternComplete();
                }
            }
            else
            {
                PlayHandPatternBodyAnimation("DownTarget");
                PlayerBase[] playerBases = InGamePlayManager.Instance.GetAllPlayer();
                if (playerBases[0].transform.position.x > playerBases[1].transform.position.x)
                {
                    LeftBossParts.DownTargetAttack(playerBases[1]);
                    RightBossParts.DownTargetAttack(playerBases[0]);
                }
                else
                {
                    LeftBossParts.DownTargetAttack(playerBases[0]);
                    RightBossParts.DownTargetAttack(playerBases[1]);
                }
            }
        }

        #region Clash

        private void ClashAttack()
        {
            SetIsNotNeutralizeChangeClientRPC(true);
            leftPlatForm.Appear(1);
            rightPlatForm.Appear(1);
            AllSummonDied();
            clashManager.endEvent += ClashAttackEndDirector;

            StartCoroutine(PlayClashCoroutine((float)TimeManager.Instance.ClientServerTimeOffset));
            PlayClashClientRPC(NetworkManager.ServerTime.TimeAsFloat);
        }

        [ClientRpc]
        private void PlayClashClientRPC(float time)
        {
            if (!IsServer)
            {
                float dif = time - NetworkManager.ServerTime.TimeAsFloat;
                StartCoroutine(PlayClashCoroutine(dif));
            }
        }

        private IEnumerator PlayClashCoroutine(float time)
        {
            if (time > 0)
                yield return new WaitForSeconds(time);

            networkAnimator.SetTrigger("ClashSignalLeft");
            LeftBossParts.networkAnimator.SetTrigger("HandClashSignal");
            RightBossParts.networkAnimator.SetTrigger("HandClashSignal");
            LeftBossParts.MovePosition(LeftBossParts.handPositionManager.ClashSignalPosition.position, 2.7f, BossData.ClashSignalEase);
            RightBossParts.MovePosition(LeftBossParts.handPositionManager.ClashSignalReversePosition.position, 2.7f, BossData.ClashSignalEase);
            clashManager.PlayClash();
            SoundManager.Instance.PlayClash();
            if (IsServer)
                sound.PlayClashStartClientRPC();
        }

        private void ClashAttackEndDirector()
        {
            LeftClashAttacking().Forget();
            clashManager.endEvent -= ClashAttackEndDirector;
        }

        private bool isClash = false;
        private bool isClashing = false;
        private PlayerBase clashPlayer;
        public PlayerBase ClashPlayer => clashPlayer;
        private bool isFailClash = false;
        private async UniTaskVoid LeftClashAttacking()
        {
            bool isClashSuccess = false;
            float upTime = 1.5f;
            LeftBossParts.MovePosition(LeftBossParts.handPositionManager.ClashSignalIdlePosition.position, upTime, BossData.ClashSignalEase);
            RightBossParts.MovePosition(LeftBossParts.handPositionManager.ClashSignalIdleReversePosition.position, upTime, BossData.ClashSignalEase);
            ShowHelpFirstLeftClashClientRPC();
            await UniTask.Delay(TimeSpan.FromSeconds(upTime));
            SetActiveLeftClashPosClientRPC(true);
            float waitTime = BossData.ClashEntryWaitTime;
            isClash = true;
            isClashing = false;

            float time = Time.time;
            while (Time.time - time < waitTime)
            {
                await UniTask.Yield();
                if (isClashing)
                {
                    isClashSuccess = true;
                    break;
                }
            }

            isClash = false;
            SetActiveLeftClashPosClientRPC(false);

            if (isClashSuccess)
            {
                ShowHelpFirstClashingClientRPC(ConnectionManager.GetClientRpcParams(PlayManager.Instance.GetAnotherPlayer(clashPlayer.OwnerClientId).OwnerClientId));
                SetLeftClashCameraClientRPC(true, ConnectionManager.GetClientRpcParams(clashPlayer.OwnerClientId));
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                networkAnimator.SetTrigger("ClashMiddle");
                LeftBossParts.networkAnimator.SetTrigger("HandClashMiddle");
                RightBossParts.networkAnimator.SetTrigger("HandClashMiddle");

                float downTime = 0.25f;
                LeftBossParts.MovePosition(LeftBossParts.handPositionManager.ClashMiddlePosition.position, downTime);
                RightBossParts.MovePosition(LeftBossParts.handPositionManager.ClashMiddleReversePosition.position, downTime);
                sound.PlayLeftHandSwingClientRPC();

                await UniTask.Delay(TimeSpan.FromSeconds(downTime));
                SetActiveClashBarClientRPC(true, ConnectionManager.GetClientRpcParams(clashPlayer.OwnerClientId));
                SetActiveLeftClashParticleClientRPC(true, clashPlayer.PlayerType == PlayerType.LASER);
                sound.PlayLeftClashAttackClientRPC();
                sound.PlayLeftClashAttackingClientRPC();

                float clashTime = BossData.ClashTime;
                time = Time.time;

                while (Time.time - time < clashTime)
                {
                    if (isFailClash)
                        break;
                    await UniTask.Yield();
                }

                SetActiveClashBarClientRPC(false, ConnectionManager.GetClientRpcParams(clashPlayer.OwnerClientId));
                SetActiveLeftClashParticleClientRPC(false, clashPlayer.PlayerType == PlayerType.LASER, isFailClash);
                sound.StopClashAttackingClientRPC();

                if (!isFailClash)
                {
                    sound.PlayLeftClashSuccessClientRPC();
                    networkAnimator.SetTrigger("ClashSuccess");
                    LeftBossParts.networkAnimator.SetTrigger("HandClashSuccess");
                    RightBossParts.networkAnimator.SetTrigger("HandClashSuccess");
                    LeftBossParts.MovePosition(LeftBossParts.transform.position + new Vector3(0, 10, 0), 0.125f);
                    RightBossParts.MovePosition(RightBossParts.transform.position + new Vector3(0, 10, 0), 0.125f);
                    await UniTask.Delay(500);
                    clashPlayer.ClashClientRPC(false);

                    if (!PlayManager.Instance.IsDebugMode)
                        clashPlayer.StunClientRPC(10);
                }
                else
                {
                    sound.PlayLeftClashFailClientRPC();
                    networkAnimator.SetTrigger("ClashFail");
                    LeftBossParts.networkAnimator.SetTrigger("HandClashFail");
                    RightBossParts.networkAnimator.SetTrigger("HandClashFail");

                    LeftBossParts.MovePosition(LeftBossParts.handPositionManager.ClashAttackPosition.position, downTime);
                    RightBossParts.MovePosition(LeftBossParts.handPositionManager.ClashAttackReversePosition.position, downTime);
                    Physics.Raycast(leftClashPos.transform.position, Vector3.down, out RaycastHit downHit, 100, LayerMask.GetMask("Ground"));
                    DOVirtual.DelayedCall(downTime * 0.5f, () => LeftBossParts.MeshDemolisherDisappear(leftPlatForm, 3f, 0));

                    clashPlayer.ClashClientRPC(false);
                    await UniTask.Delay(500);
                    clashPlayer.TakeDamage(100);
                    LeftBossParts.ClashDownAttack();
                    RightBossParts.ClashDownAttack();
                    ShowHelpFirstFailClashClientRPC();
                }
                SetRightClashCameraClientRPC(false, ConnectionManager.GetClientRpcParams(clashPlayer.OwnerClientId));
                SetActiveClashBarClientRPC(false, ConnectionManager.GetClientRpcParams(clashPlayer.OwnerClientId));

                await UniTask.Delay(3000);

                RightBossParts.MoveOriginPosition(1);
                LeftBossParts.MoveOriginPosition(1);
            }
            else
            {
                sound.PlayLeftHandSwingClientRPC();
                networkAnimator.SetTrigger("ClashFail");
                LeftBossParts.networkAnimator.SetTrigger("HandClashFail");
                RightBossParts.networkAnimator.SetTrigger("HandClashFail");

                float downTime = 0.25f;
                LeftBossParts.MovePosition(LeftBossParts.handPositionManager.ClashAttackPosition.position, downTime);
                RightBossParts.MovePosition(LeftBossParts.handPositionManager.ClashAttackReversePosition.position, downTime);
                DOVirtual.DelayedCall(downTime * 0.5f, () =>
                {
                    LeftBossParts.MeshDemolisherDisappear(leftPlatForm, 3f, 0);
                    sound.PlayLeftClashFailClientRPC();
                });

                await UniTask.Delay(TimeSpan.FromSeconds(downTime));
                LeftBossParts.ClashDownAttack();
                RightBossParts.ClashDownAttack();
                ShowHelpFirstFailClashClientRPC();

                await UniTask.Delay(3000);
                LeftBossParts.MoveOriginPosition(1);
                RightBossParts.MoveOriginPosition(1);
            }
            isFailClash = false;
            clashPlayer = null;
            await UniTask.Delay(1000);

            networkAnimator.SetTrigger("ClashSignalRight");
            LeftBossParts.networkAnimator.SetTrigger("HandClashSignal");
            RightBossParts.networkAnimator.SetTrigger("HandClashSignal");
            RightBossParts.MovePosition(RightBossParts.handPositionManager.ClashSignalPosition.position, 4f, BossData.ClashSignalEase);
            LeftBossParts.MovePosition(RightBossParts.handPositionManager.ClashSignalReversePosition.position, 4f, BossData.ClashSignalEase);
            sound.PlayClashStartClientRPC();

            await UniTask.Delay(4500);

            RightClashAttacking().Forget();
        }

        private async UniTaskVoid RightClashAttacking()
        {
            bool isClashSuccess = false;
            float upTime = 1.5f;
            RightBossParts.MovePosition(RightBossParts.handPositionManager.ClashSignalIdlePosition.position, upTime, BossData.ClashSignalEase);
            LeftBossParts.MovePosition(RightBossParts.handPositionManager.ClashSignalIdleReversePosition.position, upTime, BossData.ClashSignalEase);
            ShowHelpFirstRightClashClientRPC();
            await UniTask.Delay(TimeSpan.FromSeconds(upTime));
            SetActiveRightClashPosClientRPC(true);
            float waitTime = BossData.ClashEntryWaitTime;
            isClash = true;
            isClashing = false;

            float time = Time.time;
            while (Time.time - time < waitTime)
            {
                await UniTask.Yield();
                if (isClashing)
                {
                    isClashSuccess = true;
                    break;
                }
            }

            isClash = false;
            SetActiveRightClashPosClientRPC(false);

            if (isClashSuccess)
            {
                ShowHelpFirstClashingClientRPC(ConnectionManager.GetClientRpcParams(PlayManager.Instance.GetAnotherPlayer(clashPlayer.OwnerClientId).OwnerClientId));
                SetRightClashCameraClientRPC(true, ConnectionManager.GetClientRpcParams(clashPlayer.OwnerClientId));
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                networkAnimator.SetTrigger("ClashMiddle");
                LeftBossParts.networkAnimator.SetTrigger("HandClashMiddle");
                RightBossParts.networkAnimator.SetTrigger("HandClashMiddle");

                float downTime = 0.25f;
                RightBossParts.MovePosition(RightBossParts.handPositionManager.ClashMiddlePosition.position, downTime);
                LeftBossParts.MovePosition(RightBossParts.handPositionManager.ClashMiddleReversePosition.position, downTime);
                sound.PlayRightHandSwingClientRPC();

                await UniTask.Delay(TimeSpan.FromSeconds(downTime));
                SetActiveClashBarClientRPC(true, ConnectionManager.GetClientRpcParams(clashPlayer.OwnerClientId));
                SetActiveRightClashParticleClientRPC(true, clashPlayer.PlayerType == PlayerType.LASER);
                sound.PlayRightClashAttackClientRPC();
                sound.PlayRightClashAttackingClientRPC();

                float clashTime = BossData.ClashTime;
                time = Time.time;

                while (Time.time - time < clashTime)
                {
                    if (isFailClash)
                        break;
                    await UniTask.Yield();
                }

                SetActiveClashBarClientRPC(false, ConnectionManager.GetClientRpcParams(clashPlayer.OwnerClientId));
                SetActiveRightClashParticleClientRPC(false, clashPlayer.PlayerType == PlayerType.LASER, isFailClash);
                sound.StopClashAttackingClientRPC();

                if (!isFailClash)
                {
                    sound.PlayRightClashSuccessClientRPC();
                    networkAnimator.SetTrigger("ClashSuccess");
                    LeftBossParts.networkAnimator.SetTrigger("HandClashSuccess");
                    RightBossParts.networkAnimator.SetTrigger("HandClashSuccess");
                    LeftBossParts.MovePosition(LeftBossParts.transform.position + new Vector3(0, 10, 0), 0.125f);
                    RightBossParts.MovePosition(RightBossParts.transform.position + new Vector3(0, 10, 0), 0.125f);
                    await UniTask.Delay(500);
                    clashPlayer.ClashClientRPC(false);

                    if (!PlayManager.Instance.IsDebugMode)
                        clashPlayer.StunClientRPC(5);
                }
                else
                {
                    sound.PlayRightClashFailClientRPC();
                    networkAnimator.SetTrigger("ClashFail");
                    LeftBossParts.networkAnimator.SetTrigger("HandClashFail");
                    RightBossParts.networkAnimator.SetTrigger("HandClashFail");

                    RightBossParts.MovePosition(RightBossParts.handPositionManager.ClashAttackPosition.position, downTime);
                    LeftBossParts.MovePosition(RightBossParts.handPositionManager.ClashAttackReversePosition.position, downTime);

                    DOVirtual.DelayedCall(downTime * 0.5f, () => LeftBossParts.MeshDemolisherDisappear(rightPlatForm, 3f, 0));

                    clashPlayer.ClashClientRPC(false);
                    await UniTask.Delay(500);
                    clashPlayer.TakeDamage(100);
                    LeftBossParts.ClashDownAttack();
                    RightBossParts.ClashDownAttack();
                    ShowHelpFirstFailClashClientRPC();

                }
                SetRightClashCameraClientRPC(false, ConnectionManager.GetClientRpcParams(clashPlayer.OwnerClientId));
                SetActiveClashBarClientRPC(false, ConnectionManager.GetClientRpcParams(clashPlayer.OwnerClientId));

                await UniTask.Delay(3000);

                RightBossParts.MoveOriginPosition(1);
                LeftBossParts.MoveOriginPosition(1);
            }
            else
            {
                sound.PlayRightHandSwingClientRPC();
                networkAnimator.SetTrigger("ClashFail");
                LeftBossParts.networkAnimator.SetTrigger("HandClashFail");
                RightBossParts.networkAnimator.SetTrigger("HandClashFail");

                float downTime = 0.25f;
                RightBossParts.MovePosition(RightBossParts.handPositionManager.ClashAttackPosition.position, downTime);
                LeftBossParts.MovePosition(RightBossParts.handPositionManager.ClashAttackReversePosition.position, downTime);
                DOVirtual.DelayedCall(downTime * 0.5f, () =>
                {
                    LeftBossParts.MeshDemolisherDisappear(rightPlatForm, 3f, 0);
                    sound.PlayRightClashFailClientRPC();
                });

                await UniTask.Delay(TimeSpan.FromSeconds(downTime));
                LeftBossParts.ClashDownAttack();
                RightBossParts.ClashDownAttack();
                ShowHelpFirstFailClashClientRPC();

                await UniTask.Delay(3000);
                RightBossParts.MoveOriginPosition(1);
                LeftBossParts.MoveOriginPosition(1);
            }
            isFailClash = false;
            StopCounterVignetteClientRPC();
            clashPlayer = null;

            await UniTask.Delay(2000);
            SetIsNotNeutralizeChangeClientRPC(false);

            DOVirtual.DelayedCall(2, () =>
            {
                PatternSpecialComplete();
            });
        }

        [SerializeField] private PlayableDirector phase2;

        [ServerRpc(RequireOwnership = false)]
        private void FailClashServerRPC()
        {
            isFailClash = true;
        }

        [ClientRpc]
        private void SetActiveLeftClashPosClientRPC(bool isActive)
        {
            leftClashPos.SetActive(isActive);
        }

        [ClientRpc]
        private void SetActiveRightClashPosClientRPC(bool isActive)
        {
            rightClashPos.SetActive(isActive);
        }

        [ClientRpc]
        private void SetLeftClashCameraClientRPC(bool isActive, ClientRpcParams clientRpcParams = default)
        {
            if (isActive)
            {
                CameraManager.Instance.ChangeCamera(CameraManager.CameraType.LeftClash);
                CameraManager.Instance.IsUIActive = false;
            }
            else
            {
                CameraManager.Instance.ChangeCamera(CameraManager.CameraType.Player);
                CameraManager.Instance.IsUIActive = true;
            }
        }

        [ClientRpc]
        private void SetRightClashCameraClientRPC(bool isActive, ClientRpcParams clientRpcParams = default)
        {
            if (isActive)
            {
                CameraManager.Instance.ChangeCamera(CameraManager.CameraType.RightClash);
                CameraManager.Instance.IsUIActive = false;
            }
            else
            {
                CameraManager.Instance.ChangeCamera(CameraManager.CameraType.Player);
                CameraManager.Instance.IsUIActive = true;
            }
        }

        [ClientRpc]
        private void StopCounterVignetteClientRPC()
        {
            CameraManager.Instance.StopCounterVignette(1);
        }

        [ClientRpc]
        private void SetActiveClashBarClientRPC(bool isActive, ClientRpcParams clientRpcParams = default)
        {
            clashBar.gameObject.SetActive(isActive);
            if (isActive)
            {
                clashBar.FailClashEvent += FailClashServerRPC;
            }
            else
            {
                clashBar.FailClashEvent -= FailClashServerRPC;
                leftClashParticle.Stop();
            }
        }

        private Coroutine clashShakeCoroutine;
        [ClientRpc]
        private void SetActiveLeftClashParticleClientRPC(bool isActive, bool isPlayer1, bool isFailed = false)
        {
            if (isActive)
            {
                if (isPlayer1) leftClashParticle.Play(); else leftClashParticle2.Play();
                clashShakeCoroutine = StartCoroutine(ClashShakeCamera(10));
            }
            else
            {
                StopCoroutine(clashShakeCoroutine);
                clashShakeCoroutine = null;
                if (isPlayer1) leftClashParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); else leftClashParticle2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                if (isFailed)
                {
                    if (isPlayer1)
                    {
                        clashFailedParticle.transform.position = leftClashParticle.transform.position;
                        clashFailedParticle.transform.rotation = leftClashParticle.transform.rotation;
                        clashFailedParticle.Play();
                    }
                    else
                    {
                        clashFailedParticle2.transform.position = leftClashParticle2.transform.position;
                        clashFailedParticle2.transform.rotation = leftClashParticle2.transform.rotation;
                        clashFailedParticle2.Play();
                    }
                }
                else
                {
                    if (isPlayer1)
                    {
                        clashSuccessParticle.transform.position = leftClashParticle.transform.position;
                        clashSuccessParticle.transform.rotation = leftClashParticle.transform.rotation;
                        clashSuccessParticle.Play();
                    }
                    else
                    {
                        clashSuccessParticle2.transform.position = leftClashParticle2.transform.position;
                        clashSuccessParticle2.transform.rotation = leftClashParticle2.transform.rotation;
                        clashSuccessParticle2.Play();
                    }
                }
            }
        }

        private IEnumerator ClashShakeCamera(float time)
        {
            float startTime = Time.time;
            while (Time.time - startTime < time)
            {
                CameraManager.Instance.ClashCameraShake(0.45f);
                yield return new WaitForSeconds(0.4f);
            }
        }

        [ClientRpc]
        private void SetActiveRightClashParticleClientRPC(bool isActive, bool isPlayer1, bool isFailed = false)
        {
            if (isActive)
            {
                if (isPlayer1) rightClashParticle.Play(); else rightClashParticle2.Play();
                clashShakeCoroutine = StartCoroutine(ClashShakeCamera(10));
            }
            else
            {
                StopCoroutine(clashShakeCoroutine);
                clashShakeCoroutine = null;
                if (isPlayer1) rightClashParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); else rightClashParticle2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                if (isFailed)
                {
                    if (isPlayer1)
                    {
                        clashFailedParticle.transform.position = rightClashParticle.transform.position;
                        clashFailedParticle.transform.rotation = rightClashParticle.transform.rotation;
                        clashFailedParticle.Play();
                    }
                    else
                    {
                        clashFailedParticle2.transform.position = rightClashParticle2.transform.position;
                        clashFailedParticle2.transform.rotation = rightClashParticle2.transform.rotation;
                        clashFailedParticle2.Play();
                    }
                }
                else
                {
                    if (isPlayer1)
                    {
                        clashSuccessParticle.transform.position = rightClashParticle.transform.position;
                        clashSuccessParticle.transform.rotation = rightClashParticle.transform.rotation;
                        clashSuccessParticle.Play();
                    }
                    else
                    {
                        clashSuccessParticle2.transform.position = rightClashParticle2.transform.position;
                        clashSuccessParticle2.transform.rotation = rightClashParticle2.transform.rotation;
                        clashSuccessParticle2.Play();
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckClashInputServerRPC(ulong id)
        {
            if (!isClash)
                return;

            Collider[] players = null;
            Vector3 clashPos = Vector3.down * 1.6f;
            if (leftClashPos.activeInHierarchy)
            {
                players = Physics.OverlapSphere(leftClashPos.transform.position, 2);
                clashPos += leftClashPos.transform.position;
            }
            else if (rightClashPos.activeInHierarchy)
            {
                players = Physics.OverlapSphere(rightClashPos.transform.position, 2);
                clashPos += rightClashPos.transform.position;
            }

            foreach (Collider player in players)
            {
                if (player.TryGetComponent(out PlayerBase playerBase))
                {
                    if (InGamePlayManager.Instance.IsDebugMode)
                    {
                        isClashing = true;
                        isClash = false;
                        playerBase.ClashClientRPC(true, 1);
                        SetPlayerPositionClientRPC(playerBase.OwnerClientId, clashPos);
                        clashPlayer = playerBase;
                        break;
                    }

                    if (playerBase == InGamePlayManager.Instance.GetPlayer(id) && playerBase != clashPlayer)
                    {
                        isClashing = true;
                        isClash = false;
                        playerBase.ClashClientRPC(true, 1);
                        SetPlayerPositionClientRPC(playerBase.OwnerClientId, clashPos);
                        clashPlayer = playerBase;
                        break;
                    }
                }
            }
        }

        #endregion

        #region PHASE2

        private float entryTime = 0;
        private float curSize = 0;
        private float minSize = 0.4f;
        private bool phase2BackgroundScroll = false;
        [SerializeField] private Transform skydome;

        private void Phase2()
        {
            if (IsPhase2)
            {
                PatternComplete();
                return;
            }
            IsPhase2 = true;
            SetTransformLaserEffectClientRPC(EBossParts.HEAD);
            SetIsNotNeutralizeChangeClientRPC(true);
            AllSummonDied();
            sound.PlayPhase2StartClientRPC();

            CurrentHealth.Value = BossData.BossHealth;
            StartCoroutine(PlayPhase2Coroutine((float)TimeManager.Instance.ClientServerTimeOffset));
            PlayPhase2ClientRPC(NetworkManager.ServerTime.TimeAsFloat);
        }

        [ClientRpc]
        private void PlayPhase2ClientRPC(float time)
        {
            IsPhase2 = true;
            if (!IsServer)
            {
                float dif = time - NetworkManager.ServerTime.TimeAsFloat;
                StartCoroutine(PlayPhase2Coroutine(dif));
            }
        }

        private IEnumerator PlayPhase2Coroutine(float time)
        {
            if (time > 0)
                yield return new WaitForSeconds(time);
            phase2.Play();
            SoundManager.Instance.PlayPhase2BGM();
            dissolveHandler.position = Vector3.zero;
            backgroundAnimation.LightDownStart();
            dissolveHandler.DOScale(Vector3.one * 35, 3).SetEase(Ease.Linear).OnComplete(() =>
            {
                dissolveHandler.localScale = Vector3.zero;
                LeftBossParts.DissolveEnd();
                RightBossParts.DissolveEnd();
            });

            bossHealthBar.DecreaseOpacity(CurrentHealth.Value);
            hitFXHandler.IsPhase2 = true;
            networkAnimator.SetTrigger("Disappear");
            yield return new WaitForSeconds(9);
            PatternSpecialComplete();
            SetIsNotNeutralizeChangeClientRPC(false);
            phase2BackgroundScroll = true;
            InGamePlayManager inGamePlayManager = InGamePlayManager.Instance as InGamePlayManager;
            entryTime = inGamePlayManager.PlayTime;
            curSize = skydome.localScale.x;

            LeftBossParts.SetPhase2ColliderOff();
            RightBossParts.SetPhase2ColliderOff();
            ShowHelpFirstPhase2ClientRPC();
        }

        public void Phase2BossArriveEvent()
        {
            networkAnimator.SetTrigger("Phase2");
            LeftBossParts.MoveOriginPosition(5);
            RightBossParts.MoveOriginPosition(5);
        }

        #endregion

        #region GameOver
        [SerializeField] private PlayableDirector gameOverDirector;
        private void GameOver()
        {
            LeftBossParts.MovePosition(LeftBossParts.OriginPos + new Vector3(0, -20, 0), 3);
            RightBossParts.MovePosition(RightBossParts.OriginPos + new Vector3(0, -20, 0), 3);
            AllSummonDied();

            StartCoroutine(PlayGameOverCoroutine((float)TimeManager.Instance.ClientServerTimeOffset));
            PlayGameOverClientRPC(NetworkManager.ServerTime.TimeAsFloat);
        }

        [ClientRpc]
        private void PlayGameOverClientRPC(float time)
        {
            if (!IsServer)
            {
                float dif = time - NetworkManager.ServerTime.TimeAsFloat;
                StartCoroutine(PlayGameOverCoroutine(dif));
            }
        }

        private IEnumerator PlayGameOverCoroutine(float time)
        {
            if (time > 0)
                yield return new WaitForSeconds(time);
            gameOverDirector.Play();
            phase2BackgroundScroll = false;
            SoundManager.Instance.StopIngameBGM();
            SoundManager.Instance.StopDead();
            CameraManager.Instance.IsUIActive = false;
        }

        public void GameOverEvent()
        {
            skydome.DOScale(Vector3.one * 0.25f, 6).SetEase(Ease.Linear);
            DOVirtual.DelayedCall(7, () =>
            {
                SetAllPlayerControl(false);
                inGamePlayManager.GameEnd();
            });
        }
        #endregion

        #region LAST_PURY
        [SerializeField] private ParticleSystem lastPuryChargingParticle;
        [SerializeField] private ParticleSystem lastPuryWaveParticle;
        [SerializeField] private Transform dissolveHandler;
        private DissolveVFXHandler dissolveVFXHandler;
        private WaveCollider waveCollider;
        private bool isLastPury = false;
        public bool IsLastPury => isLastPury;
        [SerializeField] private PlayableDirector lastPuryDirector;
        [SerializeField] private ParticleSystem lastPuryHealParticle;
        [SerializeField] private CapsuleCollider headCollider;
        [SerializeField] private CapsuleCollider leftHandCollider;
        [SerializeField] private CapsuleCollider rightHandCollider;
        bool isLastPuryDirector = false;
        private void LastPury()
        {
            networkAnimator.SetTrigger("LastPury");
            AllSummonDied();
            SetIsNotNeutralizeChangeClientRPC(true);
            StartCoroutine(PlayLastPuryTimeLineCoroutine((float)TimeManager.Instance.ClientServerTimeOffset));
            PlayLastPuryTimeLineClientRPC(NetworkManager.ServerTime.TimeAsFloat);
            sound.PlayLastpuryStartClientRPC();

            CurrentHealth.Value = BossData.LastpuryHealth;
            isLastPuryDirector = true;
        }

        public void LastPuryStartEvent()
        {
            StartDissolveVFXHandlerClientRPC();
            LastPuryStart().Forget();
            isLastPury = true;
        }

        private async UniTaskVoid LastPuryStart()
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
                StartCoroutine(PlayLastPuryWaveParticleCoroutine((float)TimeManager.Instance.ClientServerTimeOffset));
                PlayLastPuryWaveClientRPC(NetworkManager.ServerTime.TimeAsFloat);
                await UniTask.Delay(TimeSpan.FromSeconds(TimeManager.Instance.ClientServerTimeOffset));
                sound.PlayLastpuryAttackClientRPC();
                GroundEmissionAnimationClientRPC();
                PushAllPLayer();
                StunAllPlayer();
                ShowHelpFirstLastPuryClientRPC();
                isLastPuryDirector = false;
                CameraManager.Instance.ExplosionYCameraShake(Vector3.down, 2);
                await UniTask.Delay(TimeSpan.FromSeconds(0.45f));
                leftPlatForm.Disappear(3, 0);
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
                rightPlatForm.Disappear(3, 0);

                await UniTask.Delay(TimeSpan.FromSeconds(BossData.LastpuryAttackCoolTime));
                cancel.Token.ThrowIfCancellationRequested();
                networkAnimator.SetTrigger("LastPuryAttack");
                lastStartTime = Time.time;
            }
            catch { }
        }

        public void LastPuryAttackEvent()
        {
            LastPuryAttack().Forget();
        }

        private float lastStartTime = 0;
        private async UniTaskVoid LastPuryAttack()
        {
            if (isDeath)
                return;
            try
            {
                StartCoroutine(PlayLastPuryChargingParticleCoroutine((float)TimeManager.Instance.ClientServerTimeOffset));
                PlayLastPuryChargingParticleClientRPC(NetworkManager.ServerTime.TimeAsFloat);
                await UniTask.Delay(TimeSpan.FromSeconds(TimeManager.Instance.ClientServerTimeOffset));
                sound.PlayLastpuryChargeClientRPC();
                await UniTask.Delay(TimeSpan.FromSeconds(2.4f));
                cancel.Token.ThrowIfCancellationRequested();

                StartCoroutine(PlayLastPuryWaveParticleCoroutine((float)TimeManager.Instance.ClientServerTimeOffset));
                PlayLastPuryWaveClientRPC(NetworkManager.ServerTime.TimeAsFloat);
                await UniTask.Delay(TimeSpan.FromSeconds(TimeManager.Instance.ClientServerTimeOffset));
                GroundEmissionAnimationClientRPC();
                waveCollider.Reset();
                waveCollider.Active();
                sound.PlayLastpuryAttackClientRPC();
                PushAllPLayer();
                CameraManager.Instance.ExplosionYCameraShake(Vector3.down, 1.5f);

                if (!electricWireSummonManager.IsSummoning)
                {
                    electricWireSummonManager.Summon().Forget();
                }

                cancel.Token.ThrowIfCancellationRequested();

                await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(BossData.LastpuryAttackCoolTime * 0.01f, BossData.LastpuryAttackCoolTime)));
                cancel.Token.ThrowIfCancellationRequested();

                if (Time.time - lastStartTime > BossData.LastpuryFailTime)
                {
                    LastPuryFail();
                    return;
                }
                networkAnimator.SetTrigger("LastPuryAttack");
            }
            catch { }
        }

        private void LastPuryFail()
        {
            networkAnimator.SetTrigger("Revive");
            PlayLastPuryHealParticleClientRPC();
            sound.PlayLastpuryHealClientRPC();
            isLastPury = false;
            waveCollider.Reset();
            PatternFailed(ECooperativePattern.LASTPURY);
        }

        [ClientRpc]
        private void PlayLastPuryHealParticleClientRPC()
        {
            lastPuryHealParticle.Play();
        }

        public void LastPuryFailedEvent()
        {
            SetIsNotNeutralizeChangeClientRPC(false);
            StopDissolveVFXHandlerClientRPC();
            PatternSpecialComplete();
        }

        private void PushAllPLayer()
        {
            if (PlayManager.Instance.IsDebugMode)
            {
                PlayerBase[] playerBases = FindObjectsByType<PlayerBase>(FindObjectsSortMode.None);
                foreach (PlayerBase player in playerBases)
                {
                    Vector3 pushDirection = new Vector3(BossData.LastpuryPushDirection.x * Mathf.Sign(player.transform.position.x - headBone.position.x), BossData.LastpuryPushDirection.y, 0);
                    pushDirection *= BossData.LastpuryPushForce;
                    player.AddForceClientRPC(pushDirection, ForceMode.Impulse, BossData.LastpuryPushDenyTime);
                }
            }
            else
            {
                foreach (PlayerBase player in PlayManager.Instance.GetAllPlayer())
                {
                    Vector3 pushDirection = new Vector3(BossData.LastpuryPushDirection.x * Mathf.Sign(player.transform.position.x - headBone.position.x), BossData.LastpuryPushDirection.y, 0);
                    pushDirection *= BossData.LastpuryPushForce;
                    player.AddForceClientRPC(pushDirection, ForceMode.Impulse, BossData.LastpuryPushDenyTime);
                }
            }
        }

        private void StunAllPlayer()
        {
            if (PlayManager.Instance.IsDebugMode)
            {
                PlayerBase[] playerBases = FindObjectsByType<PlayerBase>(FindObjectsSortMode.None);
                foreach (PlayerBase player in playerBases)
                {
                    player.StunClientRPC(3);
                }
            }
            else
            {
                foreach (PlayerBase player in PlayManager.Instance.GetAllPlayer())
                {
                    player.StunClientRPC(3);
                }
            }
        }

        [ClientRpc]
        private void PlayLastPuryChargingParticleClientRPC(float time)
        {
            if (!IsServer)
            {
                float dif = time - NetworkManager.ServerTime.TimeAsFloat;
                StartCoroutine(PlayLastPuryChargingParticleCoroutine(dif));
            }
        }
        private IEnumerator PlayLastPuryChargingParticleCoroutine(float time)
        {
            if (time > 0)
                yield return new WaitForSeconds(time);
            lastPuryChargingParticle.Play();
        }

        [ClientRpc]
        private void StartDissolveVFXHandlerClientRPC()
        {
            dissolveHandler.GetComponent<FollowTransform>().IsFollow = true;
            dissolveVFXHandler.Play();
            dissolveHandler.DOScale(new Vector3(8.8f, 8.8f, 8.8f), 1);

            bossHealthBar.DecreaseOpacity(CurrentHealth.Value);
        }

        [ClientRpc]
        private void StopDissolveVFXHandlerClientRPC()
        {
            dissolveVFXHandler.Stop();
            dissolveHandler.DOScale(Vector3.zero, 1);
            hitFXHandler.IsLastPury = false;
            headCollider.radius = 4.5f;
            leftHandCollider.enabled = true;
            rightHandCollider.enabled = true;
        }

        [ClientRpc]
        private void PlayLastPuryWaveClientRPC(float time)
        {
            if (!IsServer)
            {
                float dif = time - NetworkManager.ServerTime.TimeAsFloat;
                StartCoroutine(PlayLastPuryWaveParticleCoroutine(dif));
            }
        }

        private IEnumerator PlayLastPuryWaveParticleCoroutine(float time)
        {
            if (time > 0)
                yield return new WaitForSeconds(time);
            lastPuryWaveParticle.Play();
        }

        [ClientRpc]
        private void PlayLastPuryTimeLineClientRPC(float time)
        {
            if (!IsServer)
            {
                float dif = time - NetworkManager.ServerTime.TimeAsFloat;
                StartCoroutine(PlayLastPuryTimeLineCoroutine(dif));
            }
        }

        private IEnumerator PlayLastPuryTimeLineCoroutine(float time)
        {
            if (time > 0)
                yield return new WaitForSeconds(time);
            lastPuryDirector.Play();
            hitFXHandler.IsLastPury = true;
            headCollider.radius = 2;
            leftHandCollider.enabled = false;
            rightHandCollider.enabled = false;
        }

        [SerializeField] private PlayableDirector deathDirector;
        [SerializeField] private ParticleSystem headBrokenParticle;
        private bool isDeath = false;
        private void Death()
        {
            isDeath = true;
            OnDisable();

            DeathClientRPC(NetworkManager.ServerTime.TimeAsFloat);
            DeathProduction((float)TimeManager.Instance.ClientServerTimeOffset).Forget();
        }

        [ClientRpc]
        private void DeathClientRPC(float time)
        {
            phase2BackgroundScroll = false;
            SoundManager.Instance.StopIngameBGM();
            SoundManager.Instance.StopDead();
            CameraManager.Instance.IsUIActive = false;
            magicaWindZone.SetActive(false);
            inGamePlayManager.BossDefeat();
            sound.PlayBossDeath();
            if (!IsServer)
            {
                float dif = time - NetworkManager.ServerTime.TimeAsFloat;
                DeathProduction(dif).Forget();
            }
        }

        private async UniTaskVoid DeathProduction(float time)
        {
            //ANCHOR - ClientRPC 환경입니다.
            if (time > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(time));

            electricWireSummonManager.Unsummon();
            lastPuryChargingParticle.Stop();
            networkAnimator.SetTrigger("Death");
            deathDirector.Play();
            headBrokenParticle.Play();
            CameraManager.Instance.IsUIActive = false;
            await UniTask.Delay(TimeSpan.FromSeconds(2.3f));
            skydome.DOScale(Vector3.one * 8, 6).SetEase(Ease.Linear);
            dissolveHandler.DOScale(new Vector3(70, 70, 70), 20);
            backgroundAnimation.LightUpStart();
            headBone.gameObject.SetActive(false);
            LeftBossParts.MovePosition(LeftBossParts.OriginPos + new Vector3(0, -20, 0), 2);
            RightBossParts.MovePosition(RightBossParts.OriginPos + new Vector3(0, -20, 0), 2);
        }

        public void StatisticsEvent()
        {
            DataSaveManager.Instance.CurPlayData.IsBossKilled = true;
            InGamePlayManager inGamePlayManager = InGamePlayManager.Instance as InGamePlayManager;
            inGamePlayManager.GameEnd();
        }

        private void SetAllPlayerControl(bool control)
        {
            if (PlayManager.Instance.IsDebugMode)
            {
                PlayerBase[] playerBases = FindObjectsByType<PlayerBase>(FindObjectsSortMode.None);
                foreach (PlayerBase player in playerBases)
                {
                    player.PlayerControlAllClientRPC(control);
                }
            }
            else
            {
                foreach (PlayerBase player in PlayManager.Instance.GetAllPlayer())
                {
                    player.PlayerControlAllClientRPC(control);
                }
            }
        }

        #endregion

        #region StaticElectricity Pattern / SelfDestruct Pattern / ElectricWire Pattern

        private StaticElectricitySummonManager staticElectricitySummonManager;

        /// <summary>
        /// 보스가 정전기 공격을 하는 소환수 2마리 소환합니다.
        /// </summary>
        private async UniTaskVoid StaticElectricity()
        {
            try
            {
                await UniTask.Delay(1000);
                sound.PlaySummonClientRPC();
                cancel.Token.ThrowIfCancellationRequested();

                staticElectricitySummonManager.Summon();
            }
            catch { }
        }

        private SelfDestructManager selfDestructManager;
        /// <summary>
        /// 자폭하는 소환수 2마리를 소환합니다.
        /// </summary>
        private async UniTaskVoid SelfDestruct()
        {
            try
            {
                await UniTask.Delay(1000);
                sound.PlaySummonClientRPC();
                cancel.Token.ThrowIfCancellationRequested();

                selfDestructManager.Summon();
            }
            catch { }
        }

        private ElectricWireSummonManager electricWireSummonManager;

        /// <summary>
        /// 전선을 떨어뜨리는 공격을 진행합니다.
        /// </summary>
        private async UniTaskVoid ElectricWireAttack()
        {
            try
            {
                PlayHandPatternBodyAnimation("Summon");

                await UniTask.Delay(100);
                cancel.Token.ThrowIfCancellationRequested();
                electricWireSummonManager.Summon().Forget();

                await UniTask.Delay(2000);
                cancel.Token.ThrowIfCancellationRequested();

                PlayHandPatternBodyAnimation("SummonEnd");
                await UniTask.Delay(1000);
                cancel.Token.ThrowIfCancellationRequested();
                PatternComplete();
            }
            catch { }
        }

        #endregion

        private void HandLaserAttack(bool isRight = false)
        {
            if (IsPhase2)
            {
                Debug.LogWarning("Phase2에서는 사용할 수 없는 패턴입니다.");
                PatternComplete();
                return;
            }

            if (isRight)
            {
                PlayHandPatternBodyAnimation("HandLaserRight");
                RightBossParts.HandLaser();
            }
            else
            {
                PlayHandPatternBodyAnimation("HandLaserLeft");
                LeftBossParts.HandLaser();
            }
        }

        private void HeadLaser(bool isRight)
        {
            if (isRight)
            {
                networkAnimator.SetTrigger("HeadLaserRight");
            }
            else
            {
                networkAnimator.SetTrigger("HeadLaserLeft");
            }
            //ANCHOR - y좌표가 땅 표면 위치여야함
            Vector3 leftPosition = new Vector3(-35, -4.5f, 0);
            Vector3 rightPosition = new Vector3(35, -4.5f, 0);
            Transform tf = laserEffect.transform;
            Vector3 targetPosition = isRight ? leftPosition : rightPosition;
            Vector3 currentPosition = isRight ? rightPosition : leftPosition;
            tf.forward = (currentPosition - tf.position).normalized;
            PlayLaserChargingEffectClientRPC();

            DOVirtual.DelayedCall(BossData.HeadLaserSignalTime, () =>
            {
                PlayLaserEffectClientRPC();
                ShakeExplosionX(Mathf.RoundToInt(BossData.HeadLaserAttackTime / 0.15f) - 3, 0.15f, 0.1f).Forget();
                headLaserDamageArea.IsAttack = true;
                DOVirtual.DelayedCall(BossData.HeadLaserAttackTime * 0.85f, () =>
                {
                    sound.StopLaserFireClientRPC();
                });
                DOVirtual.DelayedCall(BossData.HeadLaserAttackTime * 0.9f, () =>
                {
                    headLaserDamageArea.IsAttack = false;
                });

                DOVirtual.DelayedCall(0.6f, () =>
                {
                    DOTween.To(() => currentPosition, x =>
                    {
                        tf.forward = (x - tf.position).normalized;
                        Physics.Raycast(tf.position, tf.forward, out RaycastHit hit, 100, LayerMask.GetMask("Ground"));
                    }, targetPosition, BossData.HeadLaserAttackTime).SetUpdate(UpdateType.Fixed)
                    .OnComplete(() =>
                    {
                        DOVirtual.DelayedCall(0.5f, () =>
                        {
                            PatternSummonComplete();
                        });
                    });
                });
            });
        }

        [SerializeField] private ParticleSystem DangerLaser;
        [SerializeField] private ParticleSystem ElectronicLaser;

        private async UniTaskVoid ElectronicAttack()
        {
            try
            {
                PlayHandPatternBodyAnimation("Electronic");
                LeftBossParts.networkAnimator.SetTrigger("HandElectronic");
                RightBossParts.networkAnimator.SetTrigger("HandElectronic");

                LeftBossParts.MovePosition(LeftBossParts.handPositionManager.ElectricAttackPosition.position, BossData.ElectricLaserMoveTime);
                RightBossParts.MovePosition(RightBossParts.handPositionManager.ElectricAttackPosition.position, BossData.ElectricLaserMoveTime);
                await DelaySystem(BossData.ElectricLaserSignalTime);

                SetActiveDangerLaserOnlyClientRPC(true);
                sound.PlayForeshadowClientRPC(new Vector3(0, -4, 0));
                sound.PlayElectronicChargeClientRPC();
                await DelaySystem(TimeManager.Instance.ClientServerTimeOffsetAsFloat);
                SetActiveDangerLaser(true);
                await DelaySystem(BossData.ElectricLaserDangerLaserTime);

                SetActiveElectronicLaserOnlyClientRPC(true);
                sound.PlayElectronicFireClientRPC();
                await DelaySystem(TimeManager.Instance.ClientServerTimeOffsetAsFloat);
                SetActiveElectronicLaser(true);
                ShakeExplosionX(Mathf.RoundToInt(BossData.ElectricLaserAttackTime / 0.15f) - 2, 0.15f, 0.35f).Forget();
                await DelaySystem(BossData.ElectricLaserAttackTime * 0.9f);
                sound.StopElectronicFireClientRPC();
                await DelaySystem(BossData.ElectricLaserAttackTime * 0.1f);
                ElectronicLaser.GetComponent<DamageArea>().IsAttack = false;

                await DelaySystem(BossData.ElectricLaserDelayTime);
                LeftBossParts.MoveOriginPosition(BossData.ElectricLaserMoveTime);
                RightBossParts.MoveOriginPosition(BossData.ElectricLaserMoveTime);

                PatternComplete();
            }
            catch { }
        }

        private async UniTaskVoid ShakeExplosionX(int repeat, float time, float strength)
        {
            for (int i = 0; i < repeat; i++)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(time));
                ExplosionXShakeClientRPC(Vector3.left, strength);
            }
        }

        [ClientRpc]
        public void SetIsNotNeutralizeChangeClientRPC(bool isNotGetNeutralize)
        {
            this.isNotGetNeutralize = isNotGetNeutralize;
            if (isNotGetNeutralize)
                bossNeutralizeBar.ChangeNeutralizeBarColorBlack();
            else
                bossNeutralizeBar.ChangeNeutralizeBarColorOrigin();
        }

        public async UniTaskVoid Neutralize()
        {
            OnDisable();
            ShowHelpFirstNeutralizeClientRPC();
            currentTween.Kill();
            SetActiveDangerLaserOnlyClientRPC(false);
            SetActiveDangerLaser(false);
            SetActiveElectronicLaserOnlyClientRPC(false);
            SetActiveElectronicLaser(false);
            if (!IsPhase2)
            {
                StopLaserEffectClientRPC();
                headLaserDamageArea.IsAttack = false;
            }
            StopCounterVignetteClientRPC();
            sound.PlayNeutralizeClientRPC();
            sound.StopLeftHandMoveClientRPC();
            sound.StopRightHandMoveClientRPC();
            sound.StopElectronicFireClientRPC();
            sound.StopElectronicChargeClientRPC();
            LeftBossParts.Neutralize();
            RightBossParts.Neutralize();
            if (lastDamagedBossParts == EBossParts.LEFT_ARM)
                LeftBossParts.PlayNeutralizeEffectClientRPC();
            else if (lastDamagedBossParts == EBossParts.RIGHT_ARM)
                RightBossParts.PlayNeutralizeEffectClientRPC();

            DOTween.To(() => TimeManager.Instance.TimeScale, x => TimeManager.Instance.TimeScale = x, TimeManager.Instance.OriginTimeScale, 0.2f);
            DOTween.To(() => TimeManager.Instance.PlayerTimeScale, x => TimeManager.Instance.PlayerTimeScale = x, TimeManager.Instance.OriginTimeScale, 0.2f);
            if (lastDamagedBossParts == EBossParts.LEFT_ARM)
            {
                PlayHandPatternBodyAnimation("NeutralizeLeft");
            }
            else if (lastDamagedBossParts == EBossParts.RIGHT_ARM)
            {
                PlayHandPatternBodyAnimation("NeutralizeRight");
            }

            LeftBossParts.MoveNeutralizePosition(0.7f, true);
            RightBossParts.MoveNeutralizePosition(0.7f, true);

            await UniTask.Delay(1000);

            LeftBossParts.MoveNeutralizePosition(0.7f);
            RightBossParts.MoveNeutralizePosition(0.7f);
            await UniTask.Delay(2000);

            await UniTask.Delay((int)(BossData.BossNeutralizeTime * 1000));

            LeftBossParts.MoveOriginPosition(1.8f);
            RightBossParts.MoveOriginPosition(1.8f);
            await UniTask.Delay(3000);
            PatternComplete();
            PatternSummonComplete();

            CurrentNeutralize.Value = BossData.BossNeutralize;
            cancel = new CancellationTokenSource();
        }

        private async UniTask DelaySystem(float delay)
        {
            await UniTask.Delay((int)(delay * 1000), cancellationToken: cancel.Token);
            cancel.Token.ThrowIfCancellationRequested();
        }

        private void CalculateWeightData()
        {
            BossOneWeightData.WeightData[] weightData = new BossOneWeightData.WeightData[4];
            weightData[0] = bossWeightData.OneWeightData;
            weightData[1] = bossWeightData.TwoWeightData;
            weightData[2] = bossWeightData.ThreeWeightData;
            weightData[3] = bossWeightData.FourWeightData;

            int arraySize = 4 * 6;
            BossOneWeightData.WeightAreaData[] weightAreaData = new BossOneWeightData.WeightAreaData[arraySize];
            int cnt = 0;
            for (int i = 0; i < weightData.Length; i++)
            {
                weightAreaData[cnt] = new BossOneWeightData.WeightAreaData();
                weightAreaData[cnt++] = weightData[i].AreaOneWeightAreaData;
                weightAreaData[cnt++] = weightData[i].AreaTwoWeightAreaData;
                weightAreaData[cnt++] = weightData[i].AreaThreeWeightAreaData;
                weightAreaData[cnt++] = weightData[i].AreaFourWeightAreaData;
                weightAreaData[cnt++] = weightData[i].AreaFiveWeightAreaData;
                weightAreaData[cnt++] = weightData[i].AreaSixWeightAreaData;
            }


            WeightedFirstRandomClass = new WeightedRandom<EBossPattern>();
            WeightedFirstRandomClass.Add(EBossPattern.LEFT_HAND_LASER, bossWeightData.FirstWeightData.LeftLaserWeight);
            WeightedFirstRandomClass.Add(EBossPattern.RIGHT_HAND_LASER, bossWeightData.FirstWeightData.RightLaserWeight);
            WeightedFirstRandomClass.Add(EBossPattern.LEFT_DOWN, bossWeightData.FirstWeightData.LeftDownWeight);
            WeightedFirstRandomClass.Add(EBossPattern.RIGHT_DOWN, bossWeightData.FirstWeightData.RightDownWeight);
            WeightedFirstRandomClass.Add(EBossPattern.DOWN_TARGET, bossWeightData.FirstWeightData.TargetDownWeight);
            WeightedFirstRandomClass.Add(EBossPattern.LEFT_GRAB, bossWeightData.FirstWeightData.LeftGrabWeight);
            WeightedFirstRandomClass.Add(EBossPattern.RIGHT_GRAB, bossWeightData.FirstWeightData.RightGrabWeight);
            WeightedFirstRandomClass.Add(EBossPattern.LEFT_SWING, bossWeightData.FirstWeightData.LeftSwingWeight);
            WeightedFirstRandomClass.Add(EBossPattern.RIGHT_SWING, bossWeightData.FirstWeightData.RightSwingWeight);
            WeightedFirstRandomClass.Add(EBossPattern.ELECTRONIC, bossWeightData.FirstWeightData.ElectronicWeight);
            WeightedFirstRandomClass.Add(EBossPattern.ELECTRIC_WIRE, bossWeightData.FirstWeightData.ElectronicWireWeight);

            WeightedOneRandomClass = new WeightedRandom<EBossPattern>[6];
            for (int i = 0; i < 6; i++)
            {
                WeightedOneRandomClass[i] = new WeightedRandom<EBossPattern>();
                WeightedOneRandomClass[i].Add(EBossPattern.LEFT_HAND_LASER, weightAreaData[i].LeftLaserWeight);
                WeightedOneRandomClass[i].Add(EBossPattern.RIGHT_HAND_LASER, weightAreaData[i].RightLaserWeight);
                WeightedOneRandomClass[i].Add(EBossPattern.LEFT_DOWN, weightAreaData[i].LeftDownWeight);
                WeightedOneRandomClass[i].Add(EBossPattern.RIGHT_DOWN, weightAreaData[i].RightDownWeight);
                WeightedOneRandomClass[i].Add(EBossPattern.DOWN_TARGET, weightAreaData[i].TargetDownWeight);
                WeightedOneRandomClass[i].Add(EBossPattern.LEFT_GRAB, weightAreaData[i].LeftGrabWeight);
                WeightedOneRandomClass[i].Add(EBossPattern.RIGHT_GRAB, weightAreaData[i].RightGrabWeight);
                WeightedOneRandomClass[i].Add(EBossPattern.LEFT_SWING, weightAreaData[i].LeftSwingWeight);
                WeightedOneRandomClass[i].Add(EBossPattern.RIGHT_SWING, weightAreaData[i].RightSwingWeight);
                WeightedOneRandomClass[i].Add(EBossPattern.ELECTRONIC, weightAreaData[i].ElectronicWeight);
                WeightedOneRandomClass[i].Add(EBossPattern.ELECTRIC_WIRE, weightAreaData[i].ElectronicWireWeight);
            }

            WeightedTwoRandomClass = new WeightedRandom<EBossPattern>[6];
            for (int i = 0; i < 6; i++)
            {
                WeightedTwoRandomClass[i] = new WeightedRandom<EBossPattern>();
                WeightedTwoRandomClass[i].Add(EBossPattern.LEFT_HAND_LASER, weightAreaData[i + 6].LeftLaserWeight);
                WeightedTwoRandomClass[i].Add(EBossPattern.RIGHT_HAND_LASER, weightAreaData[i + 6].RightLaserWeight);
                WeightedTwoRandomClass[i].Add(EBossPattern.LEFT_DOWN, weightAreaData[i + 6].LeftDownWeight);
                WeightedTwoRandomClass[i].Add(EBossPattern.RIGHT_DOWN, weightAreaData[i + 6].RightDownWeight);
                WeightedTwoRandomClass[i].Add(EBossPattern.DOWN_TARGET, weightAreaData[i + 6].TargetDownWeight);
                WeightedTwoRandomClass[i].Add(EBossPattern.LEFT_GRAB, weightAreaData[i + 6].LeftGrabWeight);
                WeightedTwoRandomClass[i].Add(EBossPattern.RIGHT_GRAB, weightAreaData[i + 6].RightGrabWeight);
                WeightedTwoRandomClass[i].Add(EBossPattern.LEFT_SWING, weightAreaData[i + 6].LeftSwingWeight);
                WeightedTwoRandomClass[i].Add(EBossPattern.RIGHT_SWING, weightAreaData[i + 6].RightSwingWeight);
                WeightedTwoRandomClass[i].Add(EBossPattern.ELECTRONIC, weightAreaData[i + 6].ElectronicWeight);
                WeightedTwoRandomClass[i].Add(EBossPattern.ELECTRIC_WIRE, weightAreaData[i + 6].ElectronicWireWeight);
            }

            WeightedThreeRandomClass = new WeightedRandom<EBossPattern>[6];
            for (int i = 0; i < 6; i++)
            {
                WeightedThreeRandomClass[i] = new WeightedRandom<EBossPattern>();
                WeightedThreeRandomClass[i].Add(EBossPattern.LEFT_HAND_LASER, weightAreaData[i + 12].LeftLaserWeight);
                WeightedThreeRandomClass[i].Add(EBossPattern.RIGHT_HAND_LASER, weightAreaData[i + 12].RightLaserWeight);
                WeightedThreeRandomClass[i].Add(EBossPattern.LEFT_DOWN, weightAreaData[i + 12].LeftDownWeight);
                WeightedThreeRandomClass[i].Add(EBossPattern.RIGHT_DOWN, weightAreaData[i + 12].RightDownWeight);
                WeightedThreeRandomClass[i].Add(EBossPattern.DOWN_TARGET, weightAreaData[i + 12].TargetDownWeight);
                WeightedThreeRandomClass[i].Add(EBossPattern.LEFT_GRAB, weightAreaData[i + 12].LeftGrabWeight);
                WeightedThreeRandomClass[i].Add(EBossPattern.RIGHT_GRAB, weightAreaData[i + 12].RightGrabWeight);
                WeightedThreeRandomClass[i].Add(EBossPattern.LEFT_SWING, weightAreaData[i + 12].LeftSwingWeight);
                WeightedThreeRandomClass[i].Add(EBossPattern.RIGHT_SWING, weightAreaData[i + 12].RightSwingWeight);
                WeightedThreeRandomClass[i].Add(EBossPattern.ELECTRONIC, weightAreaData[i + 12].ElectronicWeight);
                WeightedThreeRandomClass[i].Add(EBossPattern.ELECTRIC_WIRE, weightAreaData[i + 12].ElectronicWireWeight);
            }

            WeightedFourRandomClass = new WeightedRandom<EBossPattern>[6];
            for (int i = 0; i < 6; i++)
            {
                WeightedFourRandomClass[i] = new WeightedRandom<EBossPattern>();
                WeightedFourRandomClass[i].Add(EBossPattern.LEFT_HAND_LASER, weightAreaData[i + 18].LeftLaserWeight);
                WeightedFourRandomClass[i].Add(EBossPattern.RIGHT_HAND_LASER, weightAreaData[i + 18].RightLaserWeight);
                WeightedFourRandomClass[i].Add(EBossPattern.LEFT_DOWN, weightAreaData[i + 18].LeftDownWeight);
                WeightedFourRandomClass[i].Add(EBossPattern.RIGHT_DOWN, weightAreaData[i + 18].RightDownWeight);
                WeightedFourRandomClass[i].Add(EBossPattern.DOWN_TARGET, weightAreaData[i + 18].TargetDownWeight);
                WeightedFourRandomClass[i].Add(EBossPattern.LEFT_GRAB, weightAreaData[i + 18].LeftGrabWeight);
                WeightedFourRandomClass[i].Add(EBossPattern.RIGHT_GRAB, weightAreaData[i + 18].RightGrabWeight);
                WeightedFourRandomClass[i].Add(EBossPattern.LEFT_SWING, weightAreaData[i + 18].LeftSwingWeight);
                WeightedFourRandomClass[i].Add(EBossPattern.RIGHT_SWING, weightAreaData[i + 18].RightSwingWeight);
                WeightedFourRandomClass[i].Add(EBossPattern.ELECTRONIC, weightAreaData[i + 18].ElectronicWeight);
                WeightedFourRandomClass[i].Add(EBossPattern.ELECTRIC_WIRE, weightAreaData[i + 18].ElectronicWireWeight);
            }
        }

        private bool isFirst = true;
        private EBossPattern previousPattern;
        public void WeightPattern(Action completeCallback)
        {
            int areaNumber = 0;
            int count = 0;
            int MAX_COUNT = 50;
            EBossPattern pattern;

            if (Target != null)
            {
                areaNumber = (int)calculateArea.CalculateAreaNumber(Target.transform.position);
            }
            else
            {
                do
                {
                    pattern = WeightedFirstRandomClass.GetRandomItem();
                    if (count++ > MAX_COUNT)
                    {
                        Debug.Assert(false, "INFINITY LOOP");
                        return;
                    }
                } while (previousPattern == pattern);
                BossPattern(pattern, completeCallback);
                return;
            }

            if (isFirst)
            {
                isFirst = false;
                pattern = WeightedFirstRandomClass.GetRandomItem();
                previousPattern = pattern;
                BossPattern(pattern, completeCallback);
                return;
            }

            float percent = CurrentHealth.Value / (float)BossData.BossHealth;
            do
            {
                if (!IsPhase2)
                {
                    if (percent > 0.5f)
                    {
                        pattern = WeightedOneRandomClass[areaNumber - 1].GetRandomItem();
                    }
                    else
                    {
                        pattern = WeightedTwoRandomClass[areaNumber - 1].GetRandomItem();
                    }
                }
                else
                {
                    if (percent > 0.5f)
                    {
                        pattern = WeightedThreeRandomClass[areaNumber - 1].GetRandomItem();
                    }
                    else
                    {
                        pattern = WeightedFourRandomClass[areaNumber - 1].GetRandomItem();
                    }
                }
                if (count++ > MAX_COUNT)
                {
                    Debug.Assert(false, "INFINITY LOOP");
                    return;
                }

                if (pattern == EBossPattern.ELECTRIC_WIRE && electricWireSummonManager.IsSummoning)
                    pattern = previousPattern;

            } while (pattern == previousPattern);

            previousPattern = pattern;
            BossPattern(pattern, completeCallback);
        }

        private void AllSummonDied()
        {
            electricWireSummonManager.Unsummon();
            selfDestructManager.Unsummon();
            staticElectricitySummonManager.Unsummon();
        }

        #region Client Rpc

        public override void GetDamage(int damage, PlayerName playerName)
        {
            if (isDeath || isLastPuryDirector)
                return;

            base.GetDamage(damage, playerName);
            HitFXClientRPC();

            if (isLastPury)
            {
                if (CurrentHealth.Value <= 0)
                {
                    Death();
                }
            }
        }

        public override void GetNeutralize(int neutralize, PlayerName playerName)
        {
            if (isNotGetNeutralize || IsNeutralize)
                return;

            base.GetNeutralize(neutralize, playerName);

            if (CurrentNeutralize.Value <= 0)
                Neutralize().Forget();
        }

        [ClientRpc]
        public void PlayLaserEffectClientRPC()
        {
            laserEffect.PlayLaser();
            sound.PlayLaserFireClientRPC();
        }

        [ClientRpc]
        public void PlayLaserChargingEffectClientRPC()
        {
            laserEffect.PlayCharging();
            sound.PlayLaserChargeClientRPC();
        }

        [ClientRpc]
        private void StopLaserEffectClientRPC()
        {
            laserEffect.StopLaser();
            sound.StopLaserFireClientRPC();
            sound.StopLaserChargeClientRPC();
        }

        [ClientRpc]
        private void SetActiveDangerLaserOnlyClientRPC(bool isActive)
        {
            if (IsServer)
                return;
            SetActiveDangerLaser(isActive);
        }

        private void SetActiveDangerLaser(bool isActive)
        {
            if (isActive)
            {
                DangerLaser.Play();
            }
            else
            {
                DangerLaser.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        [ClientRpc]
        private void SetActiveElectronicLaserOnlyClientRPC(bool isActive)
        {
            if (IsServer)
                return;
            SetActiveElectronicLaser(isActive);
        }

        private void SetActiveElectronicLaser(bool isActive)
        {
            if (isActive)
            {
                ElectronicLaser.Play();
                if (IsServer)
                    ElectronicLaser.GetComponent<DamageArea>().IsAttack = true;
            }
            else
            {
                ElectronicLaser.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                if (IsServer)
                    ElectronicLaser.GetComponent<DamageArea>().IsAttack = false;
            }
        }

        [ClientRpc]
        public void SetTransformLaserEffectClientRPC(EBossParts eBossParts)
        {
            if (eBossParts == EBossParts.LEFT_ARM)
            {
                laserEffect.FollowTransform(LeftBossParts.HandLaserPosition);
            }
            else if (eBossParts == EBossParts.RIGHT_ARM)
            {
                laserEffect.FollowTransform(RightBossParts.HandLaserPosition);
            }
            else if (eBossParts == EBossParts.HEAD)
            {
                laserEffect.FollowTransform(headBone);
                laserEffect.FollowOffset(3);
            }
        }

        [ClientRpc]
        private void HitFXClientRPC()
        {
            hitFXHandler.Play();
        }

        [ClientRpc]
        private void ExplosionXShakeClientRPC(Vector3 dir, float power)
        {
            dir.z = 0;
            CameraManager.Instance.ExplosionXCameraShake(dir, power);
        }

        [ClientRpc]
        public void GroundEmissionAnimationClientRPC()
        {
            backgroundAnimation.GroundLightStart();
        }

        #endregion

        public void PatternComplete()
        {
            patternCompleteCallback?.Invoke();
        }

        public void PatternSummonComplete()
        {
            summonPatternCompleteCallback?.Invoke();
        }

        public void PatternSpecialComplete()
        {
            patternSpecialCompleteCallback?.Invoke();
        }

        #region HelpWindow
        public bool IsHelpFirstCounter = false;
        public bool IsHelpFirstCounterSuccess = false;

        bool isHelpFirstLeftClash = false;
        [ClientRpc]
        public void ShowHelpFirstLeftClashClientRPC()
        {
            if (!isHelpFirstLeftClash)
            {
                isHelpFirstLeftClash = true;
                HelpWindowUI.Instance.RequestHelpText(HelpWindowUI.HelpType.LEFT_CLASH);
            }
        }

        bool isHelpFirstRightClash = false;
        [ClientRpc]
        public void ShowHelpFirstRightClashClientRPC()
        {
            if (!isHelpFirstRightClash)
            {
                isHelpFirstRightClash = true;
                HelpWindowUI.Instance.RequestHelpText(HelpWindowUI.HelpType.RIGHT_CLASH);
            }
        }

        bool isHelpFirstClashing = false;
        [ClientRpc]
        public void ShowHelpFirstClashingClientRPC(ClientRpcParams clientRpcParams = default)
        {
            if (!isHelpFirstClashing)
            {
                isHelpFirstClashing = true;
                HelpWindowUI.Instance.RequestHelpText(HelpWindowUI.HelpType.CLASHING);
            }
        }

        bool isHelpFirstFailClash = false;
        [ClientRpc]
        public void ShowHelpFirstFailClashClientRPC()
        {
            if (!isHelpFirstFailClash)
            {
                isHelpFirstFailClash = true;
                HelpWindowUI.Instance.RequestHelpText(HelpWindowUI.HelpType.CLASH_FAIL);
            }
        }

        bool isHelpFirstDead = false;
        public void ShowHelpFirstDead()
        {
            if (!isHelpFirstDead)
            {
                isHelpFirstDead = true;
                HelpWindowUI.Instance.RequestHelpText(HelpWindowUI.HelpType.DEAD);
            }
        }

        bool isHelpFirstNeutralize = false;
        [ClientRpc]
        public void ShowHelpFirstNeutralizeClientRPC()
        {
            if (!isHelpFirstNeutralize)
            {
                isHelpFirstNeutralize = true;
                HelpWindowUI.Instance.RequestHelpText(HelpWindowUI.HelpType.NEUTRALIZE);
            }
        }

        bool isHelpFirstPhase2 = false;
        [ClientRpc]
        public void ShowHelpFirstPhase2ClientRPC()
        {
            if (!isHelpFirstPhase2)
            {
                isHelpFirstPhase2 = true;
                HelpWindowUI.Instance.RequestHelpText(HelpWindowUI.HelpType.PHASE2);
            }
        }

        bool isHelpFirstLastPury = false;
        [ClientRpc]
        public void ShowHelpFirstLastPuryClientRPC()
        {
            if (!isHelpFirstLastPury)
            {
                isHelpFirstLastPury = true;
                HelpWindowUI.Instance.RequestHelpText(HelpWindowUI.HelpType.LASTPURY);
            }
        }
        #endregion
    }
}
