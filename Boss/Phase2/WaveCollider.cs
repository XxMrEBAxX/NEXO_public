using System.Collections;
using FMOD.Studio;
using FMODUnity;
using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    public class WaveCollider : NetworkBehaviour
    {
        private BossOne bossOne;
        [SerializeField] private float speed = 1;
        [SerializeField] Transform leftCollider;
        [SerializeField] Transform rightCollider;
        private bool active = false;

        [SerializeField] private EventReference waveSound;
        private EventInstance leftWaveSound;
        private EventInstance rightWaveSound;

        private void Awake()
        {
            if (ReferenceEquals(leftCollider, null) || ReferenceEquals(rightCollider, null))
            {
                Debug.LogError("WaveCollider: Collider is not set");
            }

            bossOne = FindAnyObjectByType<BossOne>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (active && IsServer)
            {
                if (other.CompareTag("Player"))
                {
                    if (other.TryGetComponent(out PlayerBase player))
                    {
                        if (player.IsShieldActive)
                        {
                            StartCoroutine(WaitReduceShieldCoroutine(player));
                        }
                        player.StunClientRPC(bossOne.BossData.LastpuryStunTime);
                    }
                }
            }
        }

        private IEnumerator WaitReduceShieldCoroutine(PlayerBase player)
        {
            yield return new WaitForSeconds(0.2f);
            player.ReduceShieldCooldownClientRPC(bossOne.BossData.LastpuryShieldReduce);
        }

        private void FixedUpdate()
        {
            if (active)
            {
                leftCollider.position += Vector3.left * speed * TimeManager.Instance.GetDeltaTime();
                rightCollider.position += Vector3.right * speed * TimeManager.Instance.GetDeltaTime();
            }

            PLAYBACK_STATE state;
            if (leftWaveSound.getPlaybackState(out state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
            {
                Vector3 pos = leftCollider.position;
                pos.z = Camera.main.transform.position.z;
                leftWaveSound.set3DAttributes(pos.To3DAttributes());
            }

            if (rightWaveSound.getPlaybackState(out state) == FMOD.RESULT.OK && state == PLAYBACK_STATE.PLAYING)
            {
                Vector3 pos = rightCollider.position;
                pos.z = Camera.main.transform.position.z;
                rightWaveSound.set3DAttributes(pos.To3DAttributes());
            }
        }

        [ContextMenu("Active")]
        public void Active()
        {
            PlayWaveSoundClientRPC();
        }

        [ClientRpc]
        public void PlayWaveSoundClientRPC()
        {
            leftWaveSound = SoundManager.Instance.Play(waveSound, SoundManager.Banks.SFX, 1, leftCollider.position);
            rightWaveSound = SoundManager.Instance.Play(waveSound, SoundManager.Banks.SFX, 1, rightCollider.position);
            active = true;
        }

        [ClientRpc]
        public void StopWaveSoundClientRPC()
        {
            SoundManager.Instance.Stop(leftWaveSound);
            SoundManager.Instance.Stop(rightWaveSound);
            leftCollider.localPosition = Vector3.zero;
            rightCollider.localPosition = Vector3.zero;
            active = false;
        }

        [ContextMenu("Reset")]
        public void Reset()
        {
            StopWaveSoundClientRPC();
        }
    }
}
