using UnityEngine;
using MyBox;
using System;

namespace BirdCase
{
    [CreateAssetMenu(menuName = "Datas/AttackProjectilesData/DefaultGunData")]
    public class DefaultGunData : ScriptableObject
    {
        [OverrideLabel("총알이 부딪쳤을 때 사라지는 레이어"), SerializeField]
        private LayerMask hitLayer;
        public LayerMask HitLayer => hitLayer;
        
        [OverrideLabel("데미지"), SerializeField] 
        private int damage;
        public int Damage => damage;

        [OverrideLabel("무력화 수치"), SerializeField]
        private int neutralizeValue;
        public int NeutralizeValue => neutralizeValue;

        [OverrideLabel("날아가는 속도"), SerializeField] 
        private float speed;
        public float Speed => speed;
        
        [OverrideLabel("움직일 수 있는 최대 거리"), SerializeField] 
        private float maxMoveDistance;
        public float MaxMoveDistance => maxMoveDistance;
        
        public float LifeDuration => maxMoveDistance / speed;
    }
}
