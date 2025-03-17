using FMODUnity;
using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    public class AnimationReceiver : NetworkBehaviour
    {
        private BossOne bossOne;
        [SerializeField] private EventReference leftPhase2Sound;
        [SerializeField] private EventReference rightPhase2Sound;

        private void Start()
        {
            bossOne = GetComponentInParent<BossOne>();
        }

        public void LastPuryEvent()
        {
            if (!IsServer)
                return;
            bossOne.LastPuryStartEvent();
        }

        public void LastPuryAttackEvent()
        {
            if (!IsServer)
                return;
            bossOne.LastPuryAttackEvent();
        }

        public void LastPuryFailedEvent()
        {
            if (!IsServer)
                return;
            bossOne.LastPuryFailedEvent();
        }

        public void Phase2LeftSound()
        {
            Vector3 pos = bossOne.LeftBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(leftPhase2Sound, SoundManager.Banks.SFX, 1, pos);
            bossOne.BackgroundAnimation.GroundLightStart();
        }

        public void Phase2RightSound()
        {
            Vector3 pos = bossOne.RightBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(rightPhase2Sound, SoundManager.Banks.SFX, 1, pos);
        }
    }
}
