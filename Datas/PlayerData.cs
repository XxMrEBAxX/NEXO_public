using MyBox;
using UnityEngine;

namespace BirdCase
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Datas/PlayerData/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [SerializeField]
        private PlayerDefaultData playerDefaultData;
        public PlayerDefaultData PlayerDefaultData => playerDefaultData;
        
        [Space(20), Header("Attack")] 
        [OverrideLabel("최대 탄창 수"), SerializeField]
        private int maxAmmoSize = 5;
        public int MaxAmmoSize => maxAmmoSize;
        
        [OverrideLabel("재장전 시간"), SerializeField]
        [Tooltip("해당 시간이 끝난 후, 밑의 이펙트시간까지 끝나야 공격이 가능합니다")]
        private float reloadTime = 1.5f;
        public float ReloadTime => reloadTime;
        
        [OverrideLabel("재장전 이후 탄환 충전 이펙트 시간"), SerializeField]
        [Tooltip("해당 시간에도 공격이 불가능합니다")]
        private float reloadEffectTime = 0.5f;
        public float ReloadEffectTime => reloadEffectTime;
        
        [Space(20), Header("Special Attack")]
        [OverrideLabel("차징공격 성공 시 쿨타임"), SerializeField]
        private float specialAttackCoolTime = 1f;
        public float SpecialAttackCoolTime => specialAttackCoolTime;
    }
}
