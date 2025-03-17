using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    public enum ECooperativePattern
    {
        CLASH,
        PHASE2,
        LASTPURY
    }

    public class BossBase : NetworkBehaviour, IBossGetDamage, IAffectByExplosion
    {
        protected BossHealthBar bossHealthBar;
        public BossHealthBar BossHealthBar => bossHealthBar;

        protected BossNeutralizeBar bossNeutralizeBar;
        public BossNeutralizeBar BossNeutralizeBar => bossNeutralizeBar;

        [SerializeField] protected BehaviorTree tree;
        public BehaviorTree Tree => tree;

        [SerializeField] private BossData bossData;
        public BossData BossData => bossData;

        [SerializeField] private SummonData summonData;
        public SummonData SummonData => summonData;

        public bool IsBattleState = false;
        public bool IsPhase2 { get; protected set; } = false;

        public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>(0);
        public NetworkVariable<int> CurrentNeutralize = new NetworkVariable<int>(0);
        public bool IsNeutralize => CurrentNeutralize.Value <= 0;

        public int CurrentGetDamage { get; set; }

        protected bool isBtPaused = false;

        private Dictionary<ECooperativePattern, bool> cooperativePatternDic =
            new Dictionary<ECooperativePattern, bool>();

        #region CheckBossPattern

        /// <summary>
        /// 현재 패턴 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="cooperativePatternIndex : 이 패턴의 현재 상태를 반환합니다."></param>
        /// <returns></returns>
        public bool IsPatternExecuted(ECooperativePattern cooperativePatternIndex)
        {
            return cooperativePatternDic[cooperativePatternIndex];
        }

        /// <summary>
        /// 패턴이 실패한 경우 상태를 false로 변경해주기 위한 함수
        /// </summary>
        /// <param name="cooperativePatternIndex : 이 패턴을 false로 변경해줍니다."></param>
        public void PatternFailed(ECooperativePattern cooperativePatternIndex)
        {
            Debug.Log("Pattern Fail : " + cooperativePatternIndex);
            cooperativePatternDic[cooperativePatternIndex] = false;
        }

        /// <summary>
        /// 패턴이 성공한 경우 true로 변경해주기 위한 함수
        /// </summary>
        /// <param name="cooperativePatternIndex : 이 패턴을 true로 변경합니다."></param>
        public void PatternExecuted(ECooperativePattern cooperativePatternIndex)
        {
            Debug.Log("Pattern Pass : " + cooperativePatternIndex);
            cooperativePatternDic[cooperativePatternIndex] = true;
        }

        private void RegisterPatternKey()
        {
            cooperativePatternDic.Add(ECooperativePattern.CLASH, false);
            cooperativePatternDic.Add(ECooperativePattern.PHASE2, false);
            cooperativePatternDic.Add(ECooperativePattern.LASTPURY, false);
        }

        /// <summary>
        /// 체력을 지정된 비율로 변경합니다.
        /// 보스의 패턴이 실패했을 경우를 위한 함수 입니다.
        /// </summary>
        /// <param name="ratio : 변경할 비율입니다."></param>
        public void HPRatioModified(int ratio)
        {
            float onePercent = BossData.BossHealth * 0.01f;
            CurrentHealth.Value = (int)onePercent * ratio;
        }

        #endregion


        public virtual void GetDamage(int damage, PlayerName playerName)
        {
            if (CurrentHealth.Value > 0)
            {
                int damageValue = CurrentHealth.Value - damage >= 0 ? damage : CurrentHealth.Value;
                if (playerName == PlayerName.Ria)
                    DataSaveManager.Instance.CurPlayData.BossDamagedByRia += damageValue;
                else
                    DataSaveManager.Instance.CurPlayData.BossDamagedByNia += damageValue;
            }
            else
            {
                return;
            }

            int currentHealth = Mathf.Clamp(CurrentHealth.Value - damage, 0, BossData.BossHealth);
            CurrentHealth.Value = currentHealth;
            SetLineHealthBarClientRPC(currentHealth, damage);
        }

        public virtual void GetNeutralize(int neutralize, PlayerName playerName)
        {
            if (CurrentNeutralize.Value >= 0)
            {
                int neutralizeValue = CurrentNeutralize.Value - neutralize >= 0 ? neutralize : CurrentNeutralize.Value;
                if (playerName == PlayerName.Ria)
                    DataSaveManager.Instance.CurPlayData.BossNeutralizedByRia += neutralizeValue;
                else
                    DataSaveManager.Instance.CurPlayData.BossNeutralizedByNia += neutralizeValue;
            }

            CurrentNeutralize.Value = Mathf.Clamp(CurrentNeutralize.Value - neutralize, 0, BossData.BossNeutralize);
        }

        public void AffectByExplosion(Vector3 explosionCenterPosition, LauncherBaseData.ExplosionData explosionData, int damage, int neutralizeValue, PlayerName playerName)
        {
            ObjectSize size = GetObjectSize();
            GetDamage(damage, playerName);
            GetNeutralize(neutralizeValue, playerName);
        }

        public ObjectSize GetObjectSize() => ObjectSize.LARGE;

        protected virtual void Awake()
        {
            bossHealthBar = FindFirstObjectByType<BossHealthBar>();
            bossNeutralizeBar = FindFirstObjectByType<BossNeutralizeBar>();
            RegisterPatternKey();
        }

        private void OnNeutralizeChanged(int prev, int current)
        {
            BossNeutralizeBar.SetNeutralizeBar(current, bossData.BossNeutralize);
        }

        protected virtual void Start()
        {
            CurrentNeutralize.OnValueChanged += OnNeutralizeChanged;

            if (!IsServer)
                return;

            CurrentHealth.Value = bossData.BossHealth;
            CurrentNeutralize.Value = bossData.BossNeutralize;

            tree = tree.Clone();
            tree.Bind(this);
        }

        protected virtual void Update()
        {
            
        }

        [ClientRpc]
        private void SetLineHealthBarClientRPC(int health, int damage)
        {
            BossHealthBar.SetLineHealth(health, Mathf.Lerp(1, 15, damage / 700f));
        }
    }
}