using UnityEngine;

namespace BirdCase
{
    public class BossDamage : MonoBehaviour, IBossGetDamage, IAffectByExplosion
    {
        private BossOne bossOne;
        public int CurrentGetDamage { get; set; }

        private void Start()
        {
            bossOne = GetComponentInParent<BossOne>();
        }
        public void GetDamage(int damage, PlayerName playerName)
        {
            bossOne.GetDamage(damage, playerName);
        }

        public void GetNeutralize(int neutralize, PlayerName playerName)
        {
            bossOne.GetNeutralize(neutralize, playerName);
        }

        public void AffectByExplosion(Vector3 explosionCenterPosition,
            LauncherBaseData.ExplosionData explosionData, int damage, int neutralizeValue, PlayerName playerName)
        {
            bossOne.AffectByExplosion(explosionCenterPosition, explosionData, damage, neutralizeValue, playerName);
        }

        public ObjectSize GetObjectSize()
        {
            return bossOne.GetObjectSize();
        }
    }
}
