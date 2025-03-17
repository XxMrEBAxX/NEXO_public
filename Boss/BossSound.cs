using Unity.Netcode;
using FMODUnity;
using UnityEngine;
using FMOD.Studio;
using MyBox;

namespace BirdCase
{
    public class BossSound : NetworkBehaviour
    {
        private BossOne bossOne;

        [OverrideLabel("내려찍기 사운드")]
        [SerializeField] EventReference downAttack;

        [OverrideLabel("손 움직이는 사운드")]
        [SerializeField] EventReference handMove;
        private EventInstance leftHandMoveInstance;
        private EventInstance rightHandMoveInstance;

        [OverrideLabel("손 휘두르기 사운드")]
        [SerializeField] EventReference handSwing;
        private EventInstance leftHandSwingInstance;
        private EventInstance rightHandSwingInstance;

        [OverrideLabel("잡혔을 때 사운드")]
        [SerializeField] EventReference handGrab;

        [OverrideLabel("전조 사운드")]
        [SerializeField] EventReference foreshadow;

        [OverrideLabel("레이저 차징 사운드")]
        [SerializeField] EventReference laserCharge;
        private EventInstance laserChargeInstance;

        [OverrideLabel("레이저 발사 사운드")]
        [SerializeField] EventReference laserFire;
        private EventInstance laserFireInstance;

        [OverrideLabel("무력화 사운드")]
        [SerializeField] EventReference neutralize;

        [OverrideLabel("무력화 후 털썩 사운드")]
        [SerializeField] EventReference neutralizeDown;

        [OverrideLabel("쫄몹 소환 사운드")]
        [SerializeField] EventReference summon;

        [OverrideLabel("쫄몹 움직임 사운드")]
        public EventReference summonMove;

        [OverrideLabel("정전기 차징 사운드")]
        public EventReference staticElectricityCharge;

        [OverrideLabel("정전기 발사 사운드")]
        public EventReference staticElectricityFire;

        [OverrideLabel("쫄몹 폭발 전조 사운드")]
        public EventReference summonExplosionSignal;

        [OverrideLabel("쫄몹 폭발 사운드")]
        public EventReference summonExplosion;

        private EventInstance electronicLeftChargeInstance;
        private EventInstance electronicRightChargeInstance;
        private EventInstance electronicLeftFireInstance;
        private EventInstance electronicRightFireInstance;

        [OverrideLabel("카운터 시작 사운드")]
        public EventReference counterStart;

        [OverrideLabel("카운터 공격 사운드")]
        public EventReference counterAttack;
        private EventInstance leftCounterAttackInstance;
        private EventInstance rightCounterAttackInstance;

        [OverrideLabel("격돌 공격 사운드")]
        public EventReference clashAttack;

        [OverrideLabel("격돌 공격 중 사운드")]
        public EventReference clashAttacking;
        private EventInstance clashAttackingInstance;

        [OverrideLabel("격돌 공격 성공 사운드")]
        public EventReference clashSuccess;

        [OverrideLabel("격돌 공격 실패 사운드")]
        public EventReference clashFail;

        [OverrideLabel("격돌 시작 사운드")]
        public EventReference clashStart;

        [OverrideLabel("2페이즈 돌입 사운드")]
        public EventReference phase2Start;

        [OverrideLabel("발악 시작 사운드")]
        public EventReference lastpuryStart;

        [OverrideLabel("발악 차징 사운드")]
        public EventReference lastpuryCharge;

        [OverrideLabel("발악 공격 사운드")]
        public EventReference lastpuryAttack;

        [OverrideLabel("보스 사망 사운드")]
        public EventReference bossDeath;

        [OverrideLabel("발악 회복 사운드")]
        public EventReference lastpuryHeal;

        [OverrideLabel("보스 포효 사운드")]
        public EventReference bossRoar;

        private void Awake()
        {
            bossOne = GetComponent<BossOne>();
        }

        private void Update()
        {
            PLAYBACK_STATE state;
            if (leftHandMoveInstance.getPlaybackState(out state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
            {
                Vector3 pos = bossOne.LeftBossParts.transform.position;
                pos.z = Camera.main.transform.position.z;
                leftHandMoveInstance.set3DAttributes(pos.To3DAttributes());
            }

            if (rightHandMoveInstance.getPlaybackState(out state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
            {
                Vector3 pos = bossOne.RightBossParts.transform.position;
                pos.z = Camera.main.transform.position.z;
                rightHandMoveInstance.set3DAttributes(pos.To3DAttributes());
            }

            if (leftHandSwingInstance.getPlaybackState(out state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
            {
                Vector3 pos = bossOne.LeftBossParts.transform.position;
                pos.z = Camera.main.transform.position.z;
                leftHandSwingInstance.set3DAttributes(pos.To3DAttributes());
            }

            if (rightHandSwingInstance.getPlaybackState(out state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
            {
                Vector3 pos = bossOne.RightBossParts.transform.position;
                pos.z = Camera.main.transform.position.z;
                rightHandSwingInstance.set3DAttributes(pos.To3DAttributes());
            }

            if (laserFireInstance.getPlaybackState(out state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
            {
                Vector3 pos = bossOne.LaserEffect.LaserEndEffect.transform.position;
                pos.z = Camera.main.transform.position.z;
                laserFireInstance.set3DAttributes(pos.To3DAttributes());
            }

            if (leftCounterAttackInstance.getPlaybackState(out state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
            {
                Vector3 pos = bossOne.LeftBossParts.transform.position;
                pos.z = Camera.main.transform.position.z;
                leftCounterAttackInstance.set3DAttributes(pos.To3DAttributes());
            }

            if (rightCounterAttackInstance.getPlaybackState(out state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
            {
                Vector3 pos = bossOne.RightBossParts.transform.position;
                pos.z = Camera.main.transform.position.z;
                rightCounterAttackInstance.set3DAttributes(pos.To3DAttributes());
            }
        }

        [ClientRpc]
        public void PlayLeftHandDownAttackClientRPC()
        {
            Vector3 pos = bossOne.LeftBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(downAttack, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayRightHandDownAttackClientRPC()
        {
            Vector3 pos = bossOne.RightBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(downAttack, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayLeftHandMoveClientRPC()
        {
            leftHandMoveInstance = SoundManager.Instance.Play(handMove, SoundManager.Banks.SFX, 1, bossOne.LeftBossParts.transform.position);
        }

        [ClientRpc]
        public void StopLeftHandMoveClientRPC()
        {
            SoundManager.Instance.SetParameter(leftHandMoveInstance, "HandMoveEnd", 1);
        }

        [ClientRpc]
        public void PlayRightHandMoveClientRPC()
        {
            rightHandMoveInstance = SoundManager.Instance.Play(handMove, SoundManager.Banks.SFX, 1, bossOne.RightBossParts.transform.position);
        }

        [ClientRpc]
        public void StopRightHandMoveClientRPC()
        {
            SoundManager.Instance.SetParameter(rightHandMoveInstance, "HandMoveEnd", 1);
        }

        [ClientRpc]
        public void PlayLeftHandSwingClientRPC()
        {
            Vector3 pos = bossOne.LeftBossParts.transform.position;
            pos.z = 0;
            leftHandSwingInstance = SoundManager.Instance.Play(handSwing, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void StopLeftHandSwingClientRPC()
        {
            SoundManager.Instance.Stop(leftHandSwingInstance);
        }

        [ClientRpc]
        public void PlayRightHandSwingClientRPC()
        {
            Vector3 pos = bossOne.RightBossParts.transform.position;
            pos.z = 0;
            rightHandSwingInstance = SoundManager.Instance.Play(handSwing, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void StopRightHandSwingClientRPC()
        {
            SoundManager.Instance.Stop(rightHandSwingInstance);
        }

        [ClientRpc]
        public void PlayLeftHandGrabClientRPC()
        {
            SoundManager.Instance.Play(handGrab, SoundManager.Banks.SFX, 1, bossOne.LeftBossParts.GrabPlayer.transform.position);
        }

        [ClientRpc]
        public void PlayRightHandGrabClientRPC()
        {
            SoundManager.Instance.Play(handGrab, SoundManager.Banks.SFX, 1, bossOne.RightBossParts.GrabPlayer.transform.position);
        }

        [ClientRpc]
        public void PlayForeshadowClientRPC(Vector3 position)
        {
            SoundManager.Instance.Play(foreshadow, SoundManager.Banks.SFX, 1, position);
        }

        [ClientRpc]
        public void PlayLaserChargeClientRPC()
        {
            laserChargeInstance = SoundManager.Instance.Play(laserCharge, SoundManager.Banks.SFX, 1, bossOne.LaserEffect.transform.position);
        }

        [ClientRpc]
        public void StopLaserChargeClientRPC()
        {
            SoundManager.Instance.Stop(laserChargeInstance);
        }

        [ClientRpc]
        public void PlayLaserFireClientRPC()
        {
            laserFireInstance = SoundManager.Instance.Play(laserFire, SoundManager.Banks.SFX, 1, bossOne.LaserEffect.LaserEndEffect.transform.position);
        }

        [ClientRpc]
        public void StopLaserFireClientRPC()
        {
            SoundManager.Instance.SetParameter(laserFireInstance, "LaserEnd", 1);
        }

        [ClientRpc]
        public void PlayNeutralizeClientRPC()
        {
            SoundManager.Instance.Play(neutralize, SoundManager.Banks.SFX, 1);
        }

        [ClientRpc]
        public void PlayLeftNeutralizeDownClientRPC()
        {
            Vector3 pos = bossOne.LeftBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(neutralizeDown, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayRightNeutralizeDownClientRPC()
        {
            Vector3 pos = bossOne.RightBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(neutralizeDown, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlaySummonClientRPC()
        {
            SoundManager.Instance.Play(summon, SoundManager.Banks.SFX, 1, new Vector3(0, 0, 20));
        }

        [ClientRpc]
        public void PlayStaticChargeClientRPC(Vector3 pos)
        {
            SoundManager.Instance.Play(staticElectricityCharge, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlaySummonExplosionSignalClientRPC(Vector3 pos)
        {
            SoundManager.Instance.Play(summonExplosionSignal, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlaySummonExplosionClientRPC(Vector3 pos)
        {
            SoundManager.Instance.Play(summonExplosion, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayElectronicChargeClientRPC()
        {
            Vector3 pos = bossOne.LeftBossParts.transform.position;
            pos.z = 0;
            Vector3 pos2 = bossOne.RightBossParts.transform.position;
            pos2.z = 0;
            electronicLeftChargeInstance = SoundManager.Instance.Play(laserCharge, SoundManager.Banks.SFX, 1, pos);
            electronicRightChargeInstance = SoundManager.Instance.Play(laserCharge, SoundManager.Banks.SFX, 1, pos2);
        }

        [ClientRpc]
        public void StopElectronicChargeClientRPC()
        {
            SoundManager.Instance.Stop(electronicLeftChargeInstance);
            SoundManager.Instance.Stop(electronicRightChargeInstance);
        }

        [ClientRpc]
        public void PlayElectronicFireClientRPC()
        {
            Vector3 pos = bossOne.LeftBossParts.transform.position;
            pos.z = 0;
            Vector3 pos2 = bossOne.RightBossParts.transform.position;
            pos2.z = 0;
            electronicLeftFireInstance = SoundManager.Instance.Play(laserFire, SoundManager.Banks.SFX, 1, pos);
            electronicRightFireInstance = SoundManager.Instance.Play(laserFire, SoundManager.Banks.SFX, 1, pos2);
        }

        [ClientRpc]
        public void StopElectronicFireClientRPC()
        {
            SoundManager.Instance.SetParameter(electronicLeftFireInstance, "LaserEnd", 1);
            SoundManager.Instance.SetParameter(electronicRightFireInstance, "LaserEnd", 1);
        }

        [ClientRpc]
        public void PlayCounterStartClientRPC()
        {
            SoundManager.Instance.Play(counterStart, SoundManager.Banks.SFX, 1);
        }

        [ClientRpc]
        public void PlayLeftCounterAttackClientRPC()
        {
            Vector3 pos = bossOne.LeftBossParts.transform.position;
            pos.z = 0;
            leftCounterAttackInstance = SoundManager.Instance.Play(counterAttack, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void StopLeftCounterAttackClientRPC()
        {
            SoundManager.Instance.Stop(leftCounterAttackInstance);
        }

        [ClientRpc]
        public void PlayRightCounterAttackClientRPC()
        {
            Vector3 pos = bossOne.RightBossParts.transform.position;
            pos.z = 0;
            rightCounterAttackInstance = SoundManager.Instance.Play(counterAttack, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void StopRightCounterAttackClientRPC()
        {
            SoundManager.Instance.Stop(rightCounterAttackInstance);
        }

        [ClientRpc]
        public void PlayLeftClashAttackClientRPC()
        {
            if (CameraManager.Instance.IsUIActive == false)
            {
                SoundManager.Instance.Play(clashAttack, SoundManager.Banks.SFX, 1);
                return;
            }
            Vector3 pos = bossOne.LeftBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(clashAttack, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayRightClashAttackClientRPC()
        {
            if (CameraManager.Instance.IsUIActive == false)
            {
                SoundManager.Instance.Play(clashAttack, SoundManager.Banks.SFX, 1);
                return;
            }
            Vector3 pos = bossOne.RightBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(clashAttack, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayLeftClashAttackingClientRPC()
        {
            if (CameraManager.Instance.IsUIActive == false)
            {
                clashAttackingInstance = SoundManager.Instance.Play(clashAttacking, SoundManager.Banks.SFX, 1);
                return;
            }
            Vector3 pos = bossOne.LeftBossParts.transform.position;
            pos.z = 0;
            clashAttackingInstance = SoundManager.Instance.Play(clashAttacking, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayRightClashAttackingClientRPC()
        {
            if (CameraManager.Instance.IsUIActive == false)
            {
                clashAttackingInstance = SoundManager.Instance.Play(clashAttacking, SoundManager.Banks.SFX, 1);
                return;
            }
            Vector3 pos = bossOne.RightBossParts.transform.position;
            pos.z = 0;
            clashAttackingInstance = SoundManager.Instance.Play(clashAttacking, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void StopClashAttackingClientRPC()
        {
            SoundManager.Instance.Stop(clashAttackingInstance);
        }

        [ClientRpc]
        public void PlayLeftClashSuccessClientRPC()
        {
            if (CameraManager.Instance.IsUIActive == false)
            {
                SoundManager.Instance.Play(clashSuccess, SoundManager.Banks.SFX, 1);
                return;
            }
            Vector3 pos = bossOne.LeftBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(clashSuccess, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayRightClashSuccessClientRPC()
        {
            if (CameraManager.Instance.IsUIActive == false)
            {
                SoundManager.Instance.Play(clashSuccess, SoundManager.Banks.SFX, 1);
                return;
            }
            Vector3 pos = bossOne.RightBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(clashSuccess, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayLeftClashFailClientRPC()
        {
            if (CameraManager.Instance.IsUIActive == false)
            {
                SoundManager.Instance.Play(clashFail, SoundManager.Banks.SFX, 1);
                return;
            }
            Vector3 pos = bossOne.LeftBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(clashFail, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayRightClashFailClientRPC()
        {
            if (CameraManager.Instance.IsUIActive == false)
            {
                SoundManager.Instance.Play(clashFail, SoundManager.Banks.SFX, 1);
                return;
            }
            Vector3 pos = bossOne.RightBossParts.transform.position;
            pos.z = 0;
            SoundManager.Instance.Play(clashFail, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayClashStartClientRPC()
        {
            SoundManager.Instance.Play(clashStart, SoundManager.Banks.SFX, 1);
        }

        [ClientRpc]
        public void PlayPhase2StartClientRPC()
        {
            SoundManager.Instance.Play(phase2Start, SoundManager.Banks.SFX, 1);
        }

        [ClientRpc]
        public void PlayLastpuryStartClientRPC()
        {
            SoundManager.Instance.Play(lastpuryStart, SoundManager.Banks.SFX, 1);
        }

        [ClientRpc]
        public void PlayLastpuryChargeClientRPC()
        {
            Vector3 pos = bossOne.HeadBone.position;
            pos.z = 0;
            SoundManager.Instance.Play(lastpuryCharge, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayLastpuryAttackClientRPC()
        {
            Vector3 pos = bossOne.HeadBone.position;
            pos.z = 0;
            SoundManager.Instance.Play(lastpuryAttack, SoundManager.Banks.SFX, 1, pos);
        }

        public void PlayBossDeath()
        {
            SoundManager.Instance.Play(bossDeath, SoundManager.Banks.SFX, 1);
        }

        [ClientRpc]
        public void PlayLastpuryHealClientRPC()
        {
            Vector3 pos = bossOne.HeadBone.position;
            pos.z = 0;
            SoundManager.Instance.Play(lastpuryHeal, SoundManager.Banks.SFX, 1, pos);
        }

        [ClientRpc]
        public void PlayBossRoarClientRPC()
        {
            SoundManager.Instance.Play(bossRoar, SoundManager.Banks.SFX, 1);
        }
    }
}
