using UnityEngine;
using DG.Tweening;
using MyBox;

namespace BirdCase
{
    [CreateAssetMenu(menuName = "Datas/BossData/BossOneData")]
    public class BossData : ScriptableObject
    {
        [Separator("스테이터스"), OverrideLabel("보스 전체 체력"), ReadOnly]
        public int BossHealth;

        [OverrideLabel("보스 한 줄 체력"), SerializeField]
        private int bossLineHealth = 100;
        public int BossLineHealth => bossLineHealth;

        [OverrideLabel("보스 줄 수"), SerializeField]
        private int bossLineHealthCount = 10;
        public int BossLineHealthCount => bossLineHealthCount;

        [OverrideLabel("보스 무력화 게이지"), SerializeField]
        private int bossNeutralize = 1000;
        public int BossNeutralize => bossNeutralize;

        [OverrideLabel("보스 무력화 시간"), SerializeField]
        private float bossNeutralizeTime = 5.0f;
        public float BossNeutralizeTime => bossNeutralizeTime;

        [Separator("체력바 관련 설정"), OverrideLabel("흔들기 시간"), SerializeField]
        private float healthBarShakeDuration = 0.5f;
        public float HealthBarShakeDuration => healthBarShakeDuration;

        [OverrideLabel("체력바 흔들기 시작 방향"), Tooltip("흔들리기 시작할 때 방향과 높이를 지정합니다. ex) [변수]의 높이만큼 증가하고 위아래로 움직입니다. 음수로 지정하면 아래, 양수로 지정하면 위로 움직이고 위아래로 흔들립니다."), SerializeField]
        private float healthBarPunchYPosition = -10;
        public float HealthBarPunchYPosition => healthBarPunchYPosition;

        [OverrideLabel("흔들기 강도"), SerializeField]
        private int healthBarShakeStrength = 10;
        public int HealthBarShakeStrength => healthBarShakeStrength;

        [OverrideLabel("흔들기 그래프"), SerializeField]
        private Ease healthBarShakeEase = Ease.OutElastic;
        public Ease HealthBarShakeEase => healthBarShakeEase;

        [OverrideLabel("입은 피해 감소 시간 (보기)"), Tooltip("입은 데미지가 현재 체력으로 맞쳐지는 시간 ex) 100의 데미지를 받았다면 [변수]초동안 100만큼 서서히 감소합니다. "), SerializeField]
        private float healthBarDamagedDecreaseDuration = 0.1f;
        public float HealthBarDamagedDecreaseDuration => healthBarDamagedDecreaseDuration;


        [Separator("보스 패턴 데이터 설정")] [Header("보스 타격 공격")] [OverrideLabel("전조 손 이동 시간"), SerializeField]
        private float swingAttackSignalMoveTime = 0.7f;
        public float SwingAttackSignalMoveTime => swingAttackSignalMoveTime;

        [OverrideLabel("전조 손 이동 그래프"), SerializeField]
        private Ease swingAttackSignalEase = Ease.Linear;
        public Ease SwingAttackSignalEase => swingAttackSignalEase;

        [OverrideLabel("전조 손 이동량"), SerializeField, Range(0, 50)]
        private float swingAttackSignalMoveAmount = 37;
        public float SwingAttackSignalMoveAmount => swingAttackSignalMoveAmount;

        [OverrideLabel("연계 전조 손 이동 시간"), SerializeField]
        private float swingAttackCombineSignalMoveTime = 0.2f;
        public float SwingAttackLinkedSignalMoveTime => swingAttackCombineSignalMoveTime;

        [OverrideLabel("공격 이동 시간"), SerializeField]
        private float swingAttackMoveTime = 1.3f;
        public float SwingAttackMoveTime => swingAttackMoveTime;

        [OverrideLabel("종료 후 딜레이"), SerializeField]
        private float swingAttackRestTime = 1.0f;
        public float SwingAttackRestTime => swingAttackRestTime;

        [OverrideLabel("보스 타격 & 잡기 시 데미지"), SerializeField]
        private int attackDamage = 1;
        public int AttackDamage => attackDamage;

        [Separator, Header("보스 내려 찍기 공격")] [OverrideLabel("전조 손 이동 시간"), SerializeField]
        private float downAttackSignalMoveTime = 1.0f;
        public float DownAttackSignalMoveTime => downAttackSignalMoveTime;

        [OverrideLabel("전조 손 이동 그래프"), SerializeField]
        private Ease downAttackSignalEase = Ease.Linear;
        public Ease DownAttackSignalEase => downAttackSignalEase;

        [OverrideLabel("공격 이동 시간"), SerializeField]
        private float downAttackMoveTime = 0.2f;
        public float DownAttackMoveTime => downAttackMoveTime;

        [OverrideLabel("공격 이동 그래프"), SerializeField]
        private Ease downAttackMoveEase = Ease.Linear;
        public Ease DownAttackMoveEase => downAttackMoveEase;

        [OverrideLabel("내려찍기 플랫폼 리스폰 시간"), SerializeField]
        private float downPlatformRespawnTime = 20f;
        public float DownPlatformRespawnTime => downPlatformRespawnTime;

        [OverrideLabel("종료 후 딜레이"), SerializeField]
        private float downAttackRestTime = 1.5f;
        public float DownAttackRestTime => downAttackRestTime;

        [OverrideLabel("내려찍기 파동 데미지"), SerializeField]
        private int downAttackWaveDamage = 1;
        public int DownAttackWaveDamage => downAttackWaveDamage;


        [Separator, Header("보스 어글자 찍기 공격")] [OverrideLabel("전조 손 이동 시간"), SerializeField]
        private float downTargetAttackSignalMoveTime = 1.0f;
        public float DownTargetAttackSignalMoveTime => downTargetAttackSignalMoveTime;

        [OverrideLabel("전조 손 이동 그래프"), SerializeField]
        private Ease downTargetAttackSignalEase = Ease.Linear;
        public Ease DownTargetAttackSignalEase => downTargetAttackSignalEase;

        [OverrideLabel("따라가는 시간"), SerializeField, Range(0, 10)]
        private float downTargetAttackFollowTime = 3.0f;
        public float DownTargetAttackFollowTime => downTargetAttackFollowTime;

        [OverrideLabel("따라가는 속도"), SerializeField, Range(0, 10)]
        private float downTargetAttackFollowSpeed = 6.0f;
        public float DownTargetAttackFollowSpeed => downTargetAttackFollowSpeed;

        [OverrideLabel("공격 이동 시간"), SerializeField]
        private float downTargetAttackMoveTime = 0.2f;
        public float DownTargetAttackMoveTime => downTargetAttackMoveTime;

        [OverrideLabel("플랫폼 재생성 시간"), SerializeField]
        private float downTargetPlatformRespawnTime = 20f;
        public float DownTargetPlatformRespawnTime => downTargetPlatformRespawnTime;

        [OverrideLabel("종료 후 딜레이"), SerializeField]
        private float downTargetAttackRestTime = 1.5f;
        public float DownTargetAttackRestTime => downTargetAttackRestTime;

        [OverrideLabel("어글자 찍기 파동 데미지"), SerializeField]
        private int downTargetAttackWaveDamage = 1;
        public int DownTargetAttackWaveDamage => downTargetAttackWaveDamage;


        [Separator, Header("보스 잡기 공격")] [OverrideLabel("전조 이동 시간"), SerializeField]
        private float grabSignalMoveTime = 1.0f;
        public float GrabSignalMoveTime => grabSignalMoveTime;

        [OverrideLabel("전조 이동 그래프"), SerializeField]
        private Ease grabSignalEase = Ease.Linear;
        public Ease GrabSignalEase => grabSignalEase;

        [OverrideLabel("잡기 이동량"), SerializeField, Range(0, 50)]
        private float grabSignalMoveAmount = 37;
        public float GrabSignalMoveAmount => grabSignalMoveAmount;

        [OverrideLabel("잡기 이동 시간"), SerializeField]
        private float grabMoveTime = 0.8f;
        public float GrabMoveTime => grabMoveTime;

        [OverrideLabel("잡기 이동 그래프"), SerializeField]
        private Ease grabMoveEase = Ease.Linear;
        public Ease GrabMoveEase => grabMoveEase;

        [OverrideLabel("잡기 후 올리는 시간"), SerializeField]
        private float grabMoveUpTime = 0.8f;
        public float GrabMoveUpTime => grabMoveUpTime;

        [OverrideLabel("잡기 후 올리는 이동 그래프"), SerializeField]
        private Ease grabMoveUpEase = Ease.Linear;
        public Ease GrabMoveUpEase => grabMoveUpEase;

        [OverrideLabel("잡기 후 내려찍기 이동 시간"), SerializeField]
        private float grabDownAttackMoveTime = 0.2f;
        public float GrabDownAttackMoveTime => grabDownAttackMoveTime;

        [OverrideLabel("잡기 후 내려찍기 데미지"), SerializeField]
        private int grabDownAttackDamage = 2;
        public int GrabDownAttackDamage => grabDownAttackDamage;

        [OverrideLabel("잡기 후 내려찍기 이동 그래프"), SerializeField]
        private Ease grabDownAttackMoveEase = Ease.Linear;
        public Ease GrabDownAttackMoveEase => grabDownAttackMoveEase;

        [OverrideLabel("잡기 플랫폼 리스폰 시간"), SerializeField]
        private float grabPlatformRespawnTime = 20f;
        public float GrabPlatformRespawnTime => grabPlatformRespawnTime;


        [OverrideLabel("종료 후 딜레이"), SerializeField]
        private float grabRestTime = 1.5f;
        public float GrabRestTime => grabRestTime;

        [OverrideLabel("잡기 풀려난 후 힘"), SerializeField]
        private float grabReboundForce = 40f;
        public float GrabReboundForce => grabReboundForce;


        [Separator, Header("보스 휩쓸기(카운터) 공격")] [OverrideLabel("전조 손 이동 시간"), SerializeField]
        private float counterAttackSignalMoveTime = 1.0f;
        public float CounterAttackSignalMoveTime => counterAttackSignalMoveTime;

        [OverrideLabel("전조 손 이동 그래프"), SerializeField]
        private Ease counterAttackSignalEase = Ease.Linear;
        public Ease CounterAttackSignalEase => counterAttackSignalEase;

        [OverrideLabel("공격 이동 시간"), SerializeField]
        private float counterAttackMoveTime = 3.0f;
        public float CounterAttackMoveTime => counterAttackMoveTime;

        [OverrideLabel("공격 이동 그래프"), SerializeField]
        private Ease counterAttackMoveEase = Ease.Linear;
        public Ease CounterAttackMoveEase => counterAttackMoveEase;

        [OverrideLabel("카운터 시 이동 시간"), SerializeField]
        private float counterAttackMoveBackTime = 0.5f;
        public float CounterAttackMoveBackTime => counterAttackMoveBackTime;

        [OverrideLabel("카운터 시 이동 그래프"), SerializeField]
        private Ease counterAttackMoveBackEase = Ease.Linear;
        public Ease CounterAttackMoveBackEase => counterAttackMoveBackEase;

        [OverrideLabel("카운터 데미지 요구치"), SerializeField]
        private int requiredCounterDamage = 1000;
        public int RequiredCounterDamage => requiredCounterDamage;

        [OverrideLabel("카운터 성공 시 무력화 시간"), SerializeField]
        private float counterAttackGroggyTime = 5.0f;
        public float CounterAttackGroggyTime => counterAttackGroggyTime;

        [OverrideLabel("카운터 플랫폼 재생성 시간"), SerializeField]
        private float counterAttackPlatformRespawnTime = 40f;
        public float CounterAttackPlatformRespawnTime => counterAttackPlatformRespawnTime;

        [OverrideLabel("카운터 불릿 타임 스케일"), SerializeField]
        private AnimationCurve counterAttackBulletTimeCurve;
        public AnimationCurve CounterAttackBulletTimeCurve => counterAttackBulletTimeCurve;

        [OverrideLabel("카운터 불릿 플레이어 타임 스케일"), SerializeField]
        private AnimationCurve counterAttackBulletPlayerTimeCurve;
        public AnimationCurve CounterAttackBulletPlayerTimeCurve => counterAttackBulletPlayerTimeCurve;


        [Separator, Header("보스 레이저 (손) 이동 공격")] [OverrideLabel("전조 이동 시간"), SerializeField]
        private float headLaserSignalTime = 1.0f;
        public float HeadLaserSignalTime => headLaserSignalTime;

        [OverrideLabel("전조 이동 그래프"), SerializeField]
        private Ease headLaserSignalEase = Ease.Linear;
        public Ease HeadLaserSignalEase => headLaserSignalEase;

        [OverrideLabel("보스 차징 이펙트 대기 시간"), SerializeField]
        private float headLaserChargeTime = 1.5f;
        public float HeadLaserChargeTime => headLaserChargeTime;

        [OverrideLabel("공격 시간"), SerializeField]
        private float headLaserAttackTime = 5.0f;
        public float HeadLaserAttackTime => headLaserAttackTime;

        [OverrideLabel("종료 후 돌아오는 시간"), SerializeField]
        private float headLaserReturnTime = 1.0f;
        public float HeadLaserReturnTime => headLaserReturnTime;


        [Separator, Header("보스 전기 방출 공격")] [OverrideLabel("손 이동 시간"), SerializeField]
        private float electricLaserMoveTime = 1.0f;
        public float ElectricLaserMoveTime => electricLaserMoveTime;

        [OverrideLabel("전조 시간"), SerializeField]
        private float electricLaserSignalTime = 1.0f;
        public float ElectricLaserSignalTime => electricLaserSignalTime;

        [OverrideLabel("위험 구역 표시 시간"), SerializeField]
        private float electricLaserDangerLaserTime = 1.5f;
        public float ElectricLaserDangerLaserTime => electricLaserDangerLaserTime;

        [OverrideLabel("지속 시간"), SerializeField]
        private float electricLaserAttackTime = 2.0f;
        public float ElectricLaserAttackTime => electricLaserAttackTime;

        [OverrideLabel("종료 후 딜레이"), SerializeField]
        private float electricLaserDelayTime = 1.0f;
        public float ElectricLaserDelayTime => electricLaserDelayTime;


        [Separator, Header("격돌")]
        [OverrideLabel("격돌 전조 이동 그래프"), SerializeField]
        private Ease clashSignalEase = Ease.Linear;
        public Ease ClashSignalEase => clashSignalEase;

        [OverrideLabel("격돌 진입 대기 시간"), SerializeField]
        private float clashEntryWaitTime = 6.0f;
        public float ClashEntryWaitTime => clashEntryWaitTime;

        [OverrideLabel("격돌 시간"), SerializeField]
        private float clashTime = 5.0f;
        public float ClashTime => clashTime;

        [OverrideLabel("격돌 바 다는 속도"), SerializeField]
        private float clashSubtractValue = 20;
        public float ClashSubtractValue => clashSubtractValue;

        [Separator, Header("발악")]

        // [OverrideLabel("발악 체력"), SerializeField]
        // private int lastpuryHealth = 10000;
        // public int LastpuryHealth => lastpuryHealth;
        [OverrideLabel("발악 체력"), ReadOnly]
        public int LastpuryHealth;

        [OverrideLabel("발악 줄 수"), SerializeField]
        private int lastpuryHealthCount = 20;
        public int LastpuryHealthCount => lastpuryHealthCount;

        [OverrideLabel("발악 공격 텀"), SerializeField, Range(3, 10)]
        private float lastpuryAttackCoolTime = 4;
        public float LastpuryAttackCoolTime => lastpuryAttackCoolTime;

        [OverrideLabel("패턴 실패할 시간"), SerializeField]
        private float lastpuryFailTime = 40;
        public float LastpuryFailTime => lastpuryFailTime;

        [OverrideLabel("발악 밀쳐지는 방향"), SerializeField]
        private Vector2 lastpuryPushDirection = new Vector2(0.7f, 0.3f);
        public Vector2 LastpuryPushDirection => lastpuryPushDirection;

        [OverrideLabel("발악 밀쳐지는 힘"), SerializeField]
        private float lastpuryPushForce = 50;
        public float LastpuryPushForce => lastpuryPushForce;

        [OverrideLabel("발악 밀쳐질 때 못 움직이는 시간"), SerializeField]
        private float lastpuryPushDenyTime = 0.3f;
        public float LastpuryPushDenyTime => lastpuryPushDenyTime;

        [OverrideLabel("발악 공격 기절 시간"), SerializeField]
        private float lastpuryStunTime = 3;
        public float LastpuryStunTime => lastpuryStunTime;

        [OverrideLabel("쉴드 돌려주는 쿨타임 (1 == 바로 초기화)"), SerializeField, Range(0, 1)]
        private float lastpuryShieldReduce = 0.75f;
        public float LastpuryShieldReduce => lastpuryShieldReduce;

        private void OnValidate()
        {
            BossHealth = bossLineHealth * bossLineHealthCount;
            LastpuryHealth = bossLineHealth * lastpuryHealthCount;
        }
    }
}
