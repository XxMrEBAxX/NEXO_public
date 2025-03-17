using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    public class SelfDestructManager : NetworkBehaviour
    {        
        public BossOne bossOne;
        [SerializeField] private SelfDestructSummon[] selfDestructSummons = new SelfDestructSummon[2];
        private int summonAliveCount = 0;
        
        private void Start()
        {
            SetActiveSummons(false);
            
            if (!IsServer)
                return;

            for (int i = 0; i < selfDestructSummons.Length; i++)
            {
                selfDestructSummons[i].DiedAction += OnDied;
            }
            bossOne = FindFirstObjectByType<BossOne>();
        }

        public void Summon()
        {
            summonAliveCount = 2;
            SetActiveSummons(true);
            for (int i = 0; i < selfDestructSummons.Length; i++)
            {
                selfDestructSummons[i].Enable();
            }
        }

        public void Unsummon()
        {
            summonAliveCount = 0;
            SetActiveSummons(false);
        }

        public void SetActiveSummons(bool active)
        {
            for (int i = 0; i < selfDestructSummons.Length; i++)
            {
                selfDestructSummons[i].SetActiveSelfClientRpc(active);
            }
        }

        public void OnDied()
        {
            summonAliveCount--;
            if (summonAliveCount <= 0)
            {
                bossOne.PatternSummonComplete();
            }
        }

        public Vector3 GetSummonPosition(int index)
        {
            return selfDestructSummons[index].transform.position;
        }
    }
}
