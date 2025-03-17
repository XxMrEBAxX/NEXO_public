using System;
using MyBox;
using UnityEngine;

namespace BirdCase
{
    [CreateAssetMenu(menuName = "Datas/AttackProjectilesData/LauncherData")]
    public class DefaultLauncherData : LauncherParentData
    {
        [Space(20)]
        [OverrideLabel("폭발 데미지"), SerializeField]
        private int explosionDamage;
        
        [OverrideLabel("무력화 수치"), SerializeField]
        private int neutralizeValue;
        public int NeutralizeValue => neutralizeValue;

        public override int GetExplosionDamage(int damagedAmount)
        {
            return explosionDamage;
        }
        
        public override int GetNeutralizeValue(int damagedAmount)
        {
            return neutralizeValue;
        }
    }
}
