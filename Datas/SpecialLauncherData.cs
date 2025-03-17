using UnityEngine;
using MyBox;
using System;

namespace BirdCase
{
    [CreateAssetMenu(menuName = "Datas/AttackProjectilesData/SpecialLauncherData")]
    public class SpecialLauncherData : LauncherParentData
    {
        [Space(20)]
        [OverrideLabel("터지는 시간"), SerializeField]
        private float timeToExplode;
        public float TimeToExplode => timeToExplode;
        
        [OverrideLabel("받은 데미지의 기준"), SerializeField]
        [Tooltip("받은 데미지당 감소되는 시간, 증가하는 데미지를 계산할 때 기준점으로 쓰입니다")]
        private int damageStandard = 1;
        public int DamageStandard => damageStandard;
        
        [OverrideLabel("받은 데미지당 감소되는 시간"), SerializeField]
        private float timeDecreasePerDamage;
        public float TimeDecreasePerDamage => timeDecreasePerDamage;
        
        [Header("데미지 관련"), Space(20)]
        [OverrideLabel("폭발 기본 데미지"), SerializeField]
        private int explosionDamage;
        
        [OverrideLabel("받은 데미지당 증가하는 데미지"), SerializeField]
        private int increasDamagePerDamaged;
            
        [OverrideLabel("최대 데미지"), SerializeField]
        private int maxDamage;
        
        [Header("무력화 관련"), Space(20)]
        [OverrideLabel("무력화 수치"), SerializeField]
        private int neutralizeValue;
        
        [OverrideLabel("받은 데미지당 증가하는 무력화 수치"), SerializeField]
        private int increaseNeutralizePerDamaged;
        
        [OverrideLabel("최대 무력화 수치"), SerializeField]
        private int maxNeutralizeValue;
        
        [Header("폭탄 크기 관련"), Space(20)]
        [OverrideLabel("기본 크기"), SerializeField]
        private float size;
        public float Size => size;
        public Vector3 OriSize { get; set; }

        [OverrideLabel("받은 데미지당 증가하는 크기"), SerializeField]
        [Tooltip("데미지당 기존 사이즈의 크기의 배수가 더해집니다 (ex. 해당 값이 0.1이면 10%씩 증가)")]
        private float increaseSizePerDamaged;
        public float IncreaseSizePerDamaged => increaseSizePerDamaged;
        
        [OverrideLabel("크기가 증가하는 시간"), SerializeField]
        [Tooltip("아래 그래프의 가로축(x)의 1이 해당 시간입니다. (아래 그래프의 x는 0~1까지로 설정)")]
        private float timeToIncreaseSize;
        public float TimeToIncreaseSize => timeToIncreaseSize;
        
        [OverrideLabel("크기 증가 그래프"), SerializeField]
        [Tooltip("x축은 시간, y축은 크기 증가량입니다. (x와 y둘 다 끝부분은 1로 설정해주셔야 위의 두 값이 제대로 적용됩니다, 중간값은 상관없어용)")]
        private AnimationCurve sizeIncreaseCurve;
        public AnimationCurve SizeIncreaseCurve => sizeIncreaseCurve;
        
        [OverrideLabel("최대 크기 배율"), SerializeField]
        [Tooltip("기존 사이즈 대비 몇배까지가 최대 크기인지 설정합니다 (ex. 1.5면 기본 크기의 1.5배까지 커집니다)")]
        private float maxSize;
        public float MaxSize => maxSize;
        
        [Header("폭발 크기 관련"), Space(20)]
        [OverrideLabel("기본 크기"), SerializeField]
        private float explosionSize = 1;
        
        [OverrideLabel("받은 데미지당 증가하는 크기"), SerializeField]
        [Tooltip("데미지당 기존 사이즈의 크기의 배수가 더해집니다 (ex. 해당 값이 0.1이면 10%씩 증가)")]
        private float increaseExplosionSizePerDamaged;
        
        [OverrideLabel("최대 폭발 크기"), SerializeField]
        [Tooltip("기존 사이즈 대비 몇배까지가 최대 크기인지 설정합니다 (ex. 1.5면 기본 크기의 1.5배까지 커집니다)")]
        private float maxExplosionSize;
        
        public float GetExplosionSize(int damagedAmount)
        {
            return Mathf.Min(explosionData.ExplosionRadius * explosionSize + ((explosionData.ExplosionRadius * increaseExplosionSizePerDamaged) * (damagedAmount / DamageStandard)),
                explosionData.ExplosionRadius * explosionSize * maxExplosionSize);
        }
        
        public override int GetExplosionDamage(int damagedAmount)
        {
            return Mathf.Min(Mathf.RoundToInt(explosionDamage + (increasDamagePerDamaged * (damagedAmount / (float)DamageStandard))), maxDamage);
        }
        
        public override int GetNeutralizeValue(int damagedAmount)
        {
            return Mathf.Min(Mathf.RoundToInt(neutralizeValue + (increaseNeutralizePerDamaged * (damagedAmount / (float)DamageStandard))), maxNeutralizeValue);
        }
    }
}
