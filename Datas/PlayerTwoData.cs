using MyBox;
using UnityEngine;

namespace BirdCase
{
    [CreateAssetMenu(menuName = "Datas/PlayerData/PlayerTwoData")]
    public class PlayerTwoData : PlayerData
    {
        [Space(20), Header("Player Two")] 
        [OverrideLabel("공격 오브젝트 풀의 프리팹"), SerializeField] 
        private GameObject attackObjectPrefab;
        public GameObject AttackObjectPrefab => attackObjectPrefab;
        
        [OverrideLabel("공격 후 다음 공격까지의 딜레이"),  SerializeField]
        private float postAttackDelay = 0.2f;
        public float PostAttackDelay => postAttackDelay;
    }
}
