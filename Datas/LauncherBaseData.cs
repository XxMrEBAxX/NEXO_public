using UnityEngine;
using MyBox;
using System;
using Unity.Netcode;

namespace BirdCase
{
    public enum ObjectSize : byte
    {
        LARGE = 0,
        MEDIUM = 1,
        SMALL = 2,
    }

    public interface IExplosionDamage
    {
        public int GetExplosionDamage(int damagedAmount);
        public int GetNeutralizeValue(int damagedAmount);
    }
    
    [CreateAssetMenu(menuName = "Datas/AttackProjectilesData/LauncherBaseData")]
    public class LauncherBaseData : ScriptableObject
    {        
        [Serializable]
        public class ExplosionData : INetworkSerializable
        {   
            [OverrideLabel("폭발 반경"), SerializeField]
            private float explosionRadius;
            public float ExplosionRadius => explosionRadius;
            
            [OverrideLabel("폭발 힘"), SerializeField]
            [Tooltip("데미지와 다르게, 물리관련 적용되는 힘입니다")]
            private float explosionForce;
            public float ExplosionForce => explosionForce;

            public static float CalculateExplosionForceBySize(ObjectSize size, float force) => force * (int)size * 0.5f;
            
            [OverrideLabel("위로 솟구치는 힘"), SerializeField, Range(0f, 1)]
            [Tooltip("1에 가까울수록 위로, 0에 가까울수록 좌우로 폭발에 힘을 받음")]
            private float upwardForce;
            public float UpwardForce => upwardForce;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref explosionForce);
                serializer.SerializeValue(ref explosionRadius);
                serializer.SerializeValue(ref upwardForce);
            }
        }
        
        [OverrideLabel("닿았을 때 영향을 받는 레이어"), SerializeField]
        [Tooltip("해당 레이어에 닿았을 때 일반 공격은 터지고, 특수 공격은 부착됩니다")]
        private LayerMask hitLayerMask;
        public LayerMask HitLayerMask => hitLayerMask;
        
        [OverrideLabel("살아있는 시간"), SerializeField]
        [Tooltip("위의 레이어에 닿지 못했을 때 발사 후 사라지기까지 걸리는 시간입니다")]
        private float lifeDuration;
        public float LifeDuration => lifeDuration;
        
        [OverrideLabel("폭발에 영향을 받는 레이어"), SerializeField]
        private LayerMask explosionLayerMask;
        public LayerMask ExplosionLayerMask => explosionLayerMask;
    }
}
