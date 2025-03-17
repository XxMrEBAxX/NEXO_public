using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace BirdCase
{
    [CreateAssetMenu(menuName = "Datas/PlayerData/PlayerOneData")]
    public class PlayerOneData : PlayerData
    {   
        [OverrideLabel("첫 차징 시간"), SerializeField]
        [Tooltip("처음 우클릭을 누르기 시작하고 차징공격이 성립되기까지의 시간입니다. 해당 시간이 지나기 전에 우클릭을 떼면 차징이 성립되지 않습니다")]
        private float chargeFirstTime = 0.6f;
        public float ChargeFirstTime => chargeFirstTime;
        
        [OverrideLabel("차징 단계 증가 시간"), SerializeField]
        private float chargeStepTime = 0.5f;
        public float ChargeStepTime => chargeStepTime;
 
        [OverrideLabel("차징 최대 탄약"), SerializeField, ReadOnly]
        private int maxChargeAmmoSize = 3;
        public int MaxChargeAmmoSize => maxChargeAmmoSize;
        
        [OverrideLabel("차징 완료 후 자동으로 발사되는 시간"), SerializeField]
        [Tooltip("차징 완료 후 우클릭을 떼지 않아도 해당 시간이 지나면 자동으로 발사됩니다")]
        private float autoShotCountdown = 2f;
        public float AutoShotCountDown => autoShotCountdown;
        
        [Space(20), Header("Player One")] 
        [OverrideLabel("공격 오브젝트 풀의 프리팹"), SerializeField] 
        private GameObject attackObjectPrefab;
        public GameObject AttackObjectPrefab => attackObjectPrefab;
        
        [OverrideLabel("연사공격 후 다음 공격까지의 딜레이"),  SerializeField]
        private float postAutoAttackDelay = 0.3f;
        public float PostAutoAttackDelay => postAutoAttackDelay;
        
        [OverrideLabel("단발공격 후 다음 공격까지의 딜레이"),  SerializeField]
        private float postSingleAttackDelay = 0.1f;
        public float PostSingleAttackDelay => postSingleAttackDelay;
        
    }
}
