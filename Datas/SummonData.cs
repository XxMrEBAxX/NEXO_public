using UnityEngine;
using MyBox;

namespace BirdCase
{
    [CreateAssetMenu(menuName = "Datas/BossData/SummonData")]
    public class SummonData : ScriptableObject
    {
        [Separator("짤몹 스테이터스"), Header("짤몹 정전기 스테이터스")]
        [OverrideLabel("체력"), SerializeField]
        private int staticElectricityHealth = 5;
        public int StaticElectricityHealth => staticElectricityHealth;
        
        [OverrideLabel("공격력"), SerializeField]
        private int staticElectricityDamage = 1;
        public int StaticElectricityDamage => staticElectricityDamage;
        
        [OverrideLabel("공격 딜레이 시간"), SerializeField]
        private float staticElectricityAttackDelay = 1.5f;
        public float StaticElectricityAttackDelay => staticElectricityAttackDelay;
        
        [OverrideLabel("이동 속도"), SerializeField]
        private float staticElectricitySpeed = 6.0f;
        public float StaticElectricitySpeed => staticElectricitySpeed;
        
        [OverrideLabel("회전 속도"), SerializeField]
        private float staticElectricityRotateSpeed = 0.5f;
        public float StaticElectricityRotateSpeed => staticElectricityRotateSpeed;
        
        [OverrideLabel("패턴 전조 전 이동 시간"), SerializeField]
        private float staticElectricityMoveDelay = 3.0f;
        public float StaticElectricityMoveDelay => staticElectricityMoveDelay;
        
        [OverrideLabel("레이저 공격 지속 시간"), SerializeField]
        private float staticElectricityLaserAttackTime = 5.0f;
        public float StaticElectricityLaserAttackTime => staticElectricityLaserAttackTime;
        
        
        [Separator, Header("짤몹 자폭 스테이터스")]
        [OverrideLabel("체력"), SerializeField]
        private int selfDestructHealth = 5;
        public int SelfDestructHealth => selfDestructHealth;
        
        [OverrideLabel("공격력"), SerializeField]
        private int selfDestructDamage = 1;
        public int SelfDestructDamage => selfDestructDamage;
        
        [OverrideLabel("이동 속도"), SerializeField]
        private float selfDestructSpeed = 6.0f;
        public float SelfDestructSpeed => selfDestructSpeed;
        
        [OverrideLabel("추적 시간"), SerializeField]
        private float selfDestructTrackingTime = 5.0f;
        public float SelfDestructTrackingTime => selfDestructTrackingTime;
        
        [OverrideLabel("추적 시 플레이어와 최소 거리"), SerializeField]
        private float selfDestructPlayerTrackingMinDistance = 1.0f;
        public float SelfDestructPlayerTrackingMinDistance => selfDestructPlayerTrackingMinDistance;
        
        [OverrideLabel("추적이 끝난 후 공격 전조 시간"), SerializeField]
        private float selfDestructSignalTime = 0.5f;
        public float SelfDestructSignalTime => selfDestructSignalTime;
        
        
        [Separator, Header("짤몹 전선 스테이터스")]
        [OverrideLabel("체력"), SerializeField]
        private int electricWireSummonHealth = 5;
        public int ElectricWireSummonHealth => electricWireSummonHealth;
        
        [OverrideLabel("공격력"), SerializeField]
        private int electricWireAttackPower = 1;
        public int ElectricWireAttackPower => electricWireAttackPower;
        
        [OverrideLabel("떨어지는 속도"), SerializeField]
        private float electricWireDropSpeed = 15f;
        public float ElectricWireDropSpeed =>  electricWireDropSpeed;
        
        [OverrideLabel("전선 스턴 시간"), SerializeField]
        private float electricWireStunTime = 3.0f;
        public float ElectricWireStunTime => electricWireStunTime;
        
        [OverrideLabel("생성 높이"), SerializeField]
        private float electricWireSetCreateHeight = 8.0f;
        public float ElectricWireSetCreateHeight => electricWireSetCreateHeight;
        
        [OverrideLabel("전선 생성 간격 (보기)"),Tooltip("전선 생성 후 [변수]초가 지난 후 다음 전선이 생성 됩니다."), SerializeField]
        private float electricWireSetInterval = 0.2f;
        public float ElectricWireSetInterval => electricWireSetInterval;
        
    }
}
