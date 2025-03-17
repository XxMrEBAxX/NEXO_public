using System;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

namespace BirdCase
{
    [Serializable]
    public class Pair<TKey, TValue>
    {
        [field:SerializeField]
        [Tooltip("기준이 되는 변화된 Y값")]
        public TKey YValue;
        [field:SerializeField]
        [Tooltip("해당 Y값에 따른 추가 중력 값 (더해짐)")]
        public TValue AdditionalGravity;

        public Pair(TKey key, TValue value)
        {
            YValue = key;
            AdditionalGravity = value;
        }
    }
    public class LauncherParentData : ScriptableObject, IExplosionDamage
    {
        [OverrideLabel("기본 폭탄 데이터"), SerializeField]
        private LauncherBaseData launcherBaseData;
        public LauncherBaseData LauncherData => launcherBaseData;
        
        [OverrideLabel("폭발 데이터"), SerializeField] 
        protected LauncherBaseData.ExplosionData explosionData;
        public LauncherBaseData.ExplosionData DataOfExplosion => explosionData;
        
        [OverrideLabel("발사 힘"), SerializeField]
        private float shotPower;
        public float ShotPower => shotPower;
        
        [OverrideLabel("기본 중력"), SerializeField]
        [Tooltip("Launcher가 발사될 때 처음 받는 중력값입니다")]
        private float gravityScale = 1f;
        public float GravityScale => gravityScale;


        [Space(20), Header("움직인 y값에 따른 중력 추가 값")] 
        [OverrideLabel("상승 할 때의 값"), SerializeField]
        private Pair<float, float> gravityScaleUp;
        public Pair<float, float> GravityScaleUp => gravityScaleUp;
        
        [OverrideLabel("하강 할 때의 값"), SerializeField]
        private Pair<float, float> gravityScaleDown;
        public Pair<float, float> GravityScaleDown => gravityScaleDown;

        public virtual int GetExplosionDamage(int damagedAmount)
        {
            return 0;
        }
        
        public virtual int GetNeutralizeValue(int damagedAmount)
        {
            return 0;
        }
    }
}
