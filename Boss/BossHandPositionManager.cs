using UnityEngine;

namespace BirdCase
{
    public class BossHandPositionManager : MonoBehaviour
    {
        public Transform OriginPosition;
        public Transform OriginPhase2Position;
        public Transform HandLaserPosition;
        public Transform DownTargetAttackPosition;
        public Transform LaserPosition;
        public Transform SwingAttackPosition;
        public Transform ElectricAttackPosition;
        public Transform GrabSuccessPosition;
        public Transform GrabSuccessAttackPosition;
        public Transform GrabSuccessAttackNotPlatformPosition;
        public Transform ClashSignalPosition;
        public Transform ClashSignalReversePosition;
        public Transform ClashSignalIdlePosition;
        public Transform ClashSignalIdleReversePosition;
        public Transform ClashMiddlePosition;
        public Transform ClashMiddleReversePosition;
        public Transform ClashAttackPosition;
        public Transform ClashAttackReversePosition;
        public Transform CounterAttackPosition; 
        public Transform NeutralPosition;

        private void Start()
        {
            if(OriginPosition == null)
                Debug.LogError("OriginPosition is not set in BossHandPositionManager");

            if(OriginPhase2Position == null)
                Debug.LogError("OriginPhase2Position is not set in BossHandPositionManager");

            if(HandLaserPosition == null)
                Debug.LogError("HandLaserPosition is not set in BossHandPositionManager");

            if(DownTargetAttackPosition == null)
                Debug.LogError("DownTargetAttackPosition is not set in BossHandPositionManager");

            if(LaserPosition == null)
                Debug.LogError("LaserPosition is not set in BossHandPositionManager");

            if(SwingAttackPosition == null)
                Debug.LogError("SwingAttackPosition is not set in BossHandPositionManager");

            if(ElectricAttackPosition == null)
                Debug.LogError("ElectricAttackPosition is not set in BossHandPositionManager");
            
            if(GrabSuccessPosition == null)
                Debug.LogError("GrabSuccessPosition is not set in BossHandPositionManager");

            if(GrabSuccessAttackPosition == null)
                Debug.LogError("GrabSuccessAttackPosition is not set in BossHandPositionManager");

            if(GrabSuccessAttackNotPlatformPosition == null)
                Debug.LogError("GrabSuccessAttackNotPlatformPosition is not set in BossHandPositionManager");

            if(ClashSignalPosition == null)
                Debug.LogError("ClashSignalPosition is not set in BossHandPositionManager");
            
            if(ClashSignalReversePosition == null)
                Debug.LogError("ClashSignalReversePosition is not set in BossHandPositionManager");

            if(ClashSignalIdlePosition == null)
                Debug.LogError("ClashSignalIdlePosition is not set in BossHandPositionManager");

            if(ClashSignalIdleReversePosition == null)
                Debug.LogError("ClashSignalIdleReversePosition is not set in BossHandPositionManager");

            if(ClashMiddlePosition == null)
                Debug.LogError("ClashMiddlePosition is not set in BossHandPositionManager");
            
            if(ClashMiddleReversePosition == null)
                Debug.LogError("ClashMiddleReversePosition is not set in BossHandPositionManager");

            if(ClashAttackPosition == null)
                Debug.LogError("ClashAttackPosition is not set in BossHandPositionManager");

            if(ClashAttackReversePosition == null)
                Debug.LogError("ClashAttackReversePosition is not set in BossHandPositionManager");
            
            if(CounterAttackPosition == null)
                Debug.LogError("CounterAttackPosition is not set in BossHandPositionManager");

            if(NeutralPosition == null)
                Debug.LogError("NeutralPosition is not set in BossHandPositionManager");
        }
    }
}
