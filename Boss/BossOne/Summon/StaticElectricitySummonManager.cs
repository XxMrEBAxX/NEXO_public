using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    public class StaticElectricitySummonManager : MonoBehaviour
    {
        public BossOne bossOne;
        public BoxCollider SummonArea;
        [SerializeField] private StaticElectricitySummon[] staticElectricitySummons = new StaticElectricitySummon[2];
        private int summonAliveCount = 0;
        public int Direction {get; private set;} = 0;
        [SerializeField] private Material foreshadowMaterial;
        [HideInInspector] public Material copiedForeshadowMaterial;

        private void Awake()
        {
            copiedForeshadowMaterial = new Material(foreshadowMaterial);
            for (int i = 0; i < staticElectricitySummons.Length; i++)
            {
                staticElectricitySummons[i].SetForeshadowMaterial(copiedForeshadowMaterial);
            }
        }
        
        private void Start()
        {
            SetActiveClientRpc(false);

            for (int i = 0; i < staticElectricitySummons.Length; i++)
            {
                staticElectricitySummons[i].DiedAction += OnDied;
            }
        }

        public void Summon()
        {
            Direction = Random.Range(0, 2) == 0 ? 1 : -1;
            summonAliveCount = 2;
            SetActiveClientRpc(true);
        }

        public void Unsummon()
        {
            summonAliveCount = 0;
            SetActiveClientRpc(false);
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
            return staticElectricitySummons[index].transform.position;
        }

        [ClientRpc]
        public void SetActiveClientRpc(bool active)
        {
            for (int i = 0; i < staticElectricitySummons.Length; i++)
            {
                staticElectricitySummons[i].SetActiveSelfClientRpc(active);
            }
        }
    }
}
