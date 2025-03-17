using UnityEngine;
using MyBox;
using System;

namespace BirdCase
{
    [CreateAssetMenu(menuName = "Datas/AttackProjectilesData/SpecialGunData")]
    public class SpecialGunData : ScriptableObject
    {
        [OverrideLabel("총알이 부딪쳤을 때 영향을 주는 레이어"), SerializeField]
        private LayerMask hitLayer;
        public LayerMask HitLayer => hitLayer;
        
        [Header("데미지 관련"), Space(20)]
        [OverrideLabel("기본 데미지"), SerializeField] 
        private int damage;
        
        [OverrideLabel("차징당 증가하는 데미지"), SerializeField]
        private int increasDamagePerDamaged;
        
        [OverrideLabel("최대 데미지"), SerializeField]
        private int maxDamage;

        [Header("무력화 관련"), Space(20)]
        [OverrideLabel("무력화 수치"), SerializeField]
        private int neutralizeValue;
        
        [OverrideLabel("차징당 증가하는 무력화 수치"), SerializeField]
        private int increaseNeutralizePerDamaged;
        
        [OverrideLabel("최대 무력화 수치"), SerializeField]
        private int maxNeutralizeValue;
        
        [Header("크기 관련"), Space(20)]
        [OverrideLabel("기본 크기"), SerializeField]
        private float size;
        public float DefaultSize => size;
        
        [OverrideLabel("차징당 증가하는 크기"), SerializeField]
        [Tooltip("데미지당 기존 사이즈의 크기의 배수가 더해집니다 (ex. 해당 값이 0.1이면 10%씩 증가)")]
        private float increaseSizePerDamaged;

        [Header("속도 및 거리 관련"), Space(20)]
        [OverrideLabel("날아가는 속도"), SerializeField] 
        private float speed;
        public float Speed => speed;
        
        [OverrideLabel("움직일 수 있는 최대 거리"), SerializeField] 
        private float maxMoveDistance;
        public float MaxMoveDistance => maxMoveDistance;
        
        public float LifeDuration => maxMoveDistance / speed;

        public float GetSize(int charzingAmount)
        {
            return 1 + (increaseSizePerDamaged * charzingAmount);
        }
        
        public int GetDamage(int charzingAmount)
        {
            return Mathf.Min(damage + (increasDamagePerDamaged * charzingAmount), maxDamage);
        }
        
        public int GetNeutralizeValue(int charzingAmount)
        {
            return Mathf.Min(neutralizeValue + (increaseNeutralizePerDamaged * charzingAmount), maxNeutralizeValue);
        }
    }
}
