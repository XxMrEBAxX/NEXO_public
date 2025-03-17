using UnityEngine;

namespace BirdCase
{
    public class BossPartsDamage : MonoBehaviour, IBossGetDamage, IAffectByExplosion
    {
        private BossParts bossParts;
        public int CurrentGetDamage { get; set; }

        private void OnTriggerStay(Collider other)
        {
            bossParts.OnTriggerStay(other);
        }

        private void Start()
        {
            bossParts = GetComponentInParent<BossParts>();
        }
        public void GetDamage(int damage, PlayerName playerName)
        {
            bossParts.GetDamage(damage, playerName);
        }
        public void GetNeutralize(int neutralize, PlayerName playerName)
        {
            bossParts.GetNeutralize(neutralize, playerName);
        }

        public void AffectByExplosion(Vector3 explosionCenterPosition,
            LauncherBaseData.ExplosionData explosionData, int damage, int neutralizeValue, PlayerName playerName)
        {
            bossParts.AffectByExplosion(explosionCenterPosition, explosionData, damage, neutralizeValue, playerName);
        }

        public ObjectSize GetObjectSize()
        {
            return bossParts.GetObjectSize();
        }
    }
}
