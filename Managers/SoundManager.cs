using System.Collections.Generic;
using FMOD.Studio;
using FMODPlus;
using FMODUnity;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace BirdCase
{
    public class SoundManager : Singleton<SoundManager>
    {
        public enum Banks
        {
            Master,
            AMB,
            BGM,
            SFX
        }
        [SerializeField] private EventReference deadSnapshot;
        private EventInstance deadSnapshotInstance;

        [SerializeField] private EventReference mainMenuBGM;
        private EventInstance mainMenuBGMInstance;

        [SerializeField] private EventReference characterSelectionBGM;
        private EventInstance characterSelectionBGMInstance;

        [SerializeField] private EventReference ingameBGM;
        private EventInstance InGameBGMInstance;

        [SerializeField] private EventReference InGameAMB;
        private EventInstance InGameAMBInstance;

        [SerializeField] private EventReference uiSelectSound;

        private List<EventInstance> bgmInstances = new List<EventInstance>();
        private List<EventInstance> sfxInstances = new List<EventInstance>();
        private List<EventInstance> ambInstances = new List<EventInstance>();

        [Range(0, 2)] public float pitch = 1;

        public float x;
        public float y;
        public float z;

        public static float MainVolume = 1;
        public static float BgmVolume = 1;
        public static float SfxVolume = 1;
        public static float AmbVolume = 1;

        protected override void OnAwake()
        {
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            SetPitchBank(Banks.BGM, pitch);
            SetPitchBank(Banks.SFX, pitch);
            SetPitchBank(Banks.AMB, pitch);

            SetVolumeBank(Banks.BGM, MainVolume * BgmVolume);
            SetVolumeBank(Banks.SFX, MainVolume * SfxVolume);
            SetVolumeBank(Banks.AMB, MainVolume * AmbVolume);

            RemoveStoppedInstances(bgmInstances);
            RemoveStoppedInstances(sfxInstances);
            RemoveStoppedInstances(ambInstances);
        }

        public void PlayMainMenuBGM()
        {
            mainMenuBGMInstance = Play(mainMenuBGM, Banks.BGM);
            StopIngameAMB();
            StopIngameBGM();
        }

        public void StopMainMenuBGM()
        {
            Stop(mainMenuBGMInstance);
        }

        public void PlayCharacterSelectionBGM()
        {
            characterSelectionBGMInstance = Play(characterSelectionBGM, Banks.BGM);
        }

        public void StopCharacterSelectionBGM()
        {
            Stop(characterSelectionBGMInstance);
        }

        public void PlayPhase1BGM()
        {
            InGameBGMInstance = Play(ingameBGM, Banks.BGM);
            PlayIngameAMB();
        }

        public void PlayPhase2BGM()
        {
            SetParameter(InGameBGMInstance, "End", 1);
        }

        public void PlayClash()
        {
            SetParameter(InGameBGMInstance, "Clash", 1);
        }

        public void StopIngameBGM()
        {
            Stop(InGameBGMInstance);
        }

        public void PlayIngameAMB()
        {
            InGameAMBInstance = Play(InGameAMB, Banks.AMB);
        }

        public void StopIngameAMB()
        {
            Stop(InGameAMBInstance);
        }

        public void PlayUISelectSound()
        {
            Play(uiSelectSound, Banks.SFX);
        }

        private float curTime = 0;
        public void PlayTestSoundUISelectSound()
        {
            if (curTime + 1 < Time.time)
            {
                curTime = Time.time;
                Play(uiSelectSound, Banks.SFX);
            }
        }

        [ContextMenu("PlayDead")]
        public void PlayDead()
        {
            deadSnapshotInstance = RuntimeManager.CreateInstance(deadSnapshot);
            deadSnapshotInstance.start();
        }

        [ContextMenu("StopDead")]
        public void StopDead()
        {
            deadSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            deadSnapshotInstance.release();
        }

        private void RemoveStoppedInstances(List<EventInstance> instanceList)
        {
            for (int i = instanceList.Count - 1; i >= 0; i--)
            {
                PLAYBACK_STATE playbackState;
                instanceList[i].getPlaybackState(out playbackState);

                if (playbackState == PLAYBACK_STATE.STOPPED)
                {
                    instanceList[i].release();
                    instanceList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 재생할 사운드를 선택하여 재생합니다.
        /// </summary>
        /// <param name="position"> 이거 기입하면 RPC로 쏴줘야하는 놈들입니다. </param>
        public EventInstance Play(EventReference eventRef, Banks type, float volumeScale = 1.0f, Vector3 position = default)
        {
            if (position == default)
            {
                position.x = Camera.main.transform.position.x;
                position.y = Camera.main.transform.position.y;
            }
            position.z += Camera.main.transform.position.z;

            EventInstance instance = RuntimeManager.CreateInstance(eventRef);
            instance.set3DAttributes(position.To3DAttributes());
            switch(type)
            {
                case Banks.AMB:
                    instance.setVolume(volumeScale * MainVolume * AmbVolume);
                    break;
                case Banks.BGM:
                    instance.setVolume(volumeScale * MainVolume * BgmVolume);
                    break;
                case Banks.SFX:
                    instance.setVolume(volumeScale * MainVolume * SfxVolume);
                    break;
                default:
                    Debug.LogWarning("Unknown sound type: " + type);
                    break;
            }
            instance.start();

            switch (type)
            {
                case Banks.AMB:
                    ambInstances.Add(instance);
                    break;
                case Banks.BGM:
                    StopInstances(bgmInstances);
                    bgmInstances.Add(instance);
                    break;
                case Banks.SFX:
                    sfxInstances.Add(instance);
                    break;
                default:
                    Debug.LogWarning("Unknown sound type: " + type);
                    break;
            }
            return instance;
        }

        public void AddSoundInstance(EventInstance instance, Banks type)
        {
            switch (type)
            {
                case Banks.AMB:
                    ambInstances.Add(instance);
                    break;
                case Banks.BGM:
                    bgmInstances.Add(instance);
                    break;
                case Banks.SFX:
                    sfxInstances.Add(instance);
                    break;
                default:
                    Debug.LogWarning("Unknown sound type: " + type);
                    break;
            }
        }

        public void SetParameter(EventInstance instance, string paramName, float value, bool ignoreseekspeed = false)
        {
            if (instance.isValid())
            {
                instance.setParameterByName(paramName, value, ignoreseekspeed);
            }
        }

        public void Stop(EventInstance instance)
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }

        public void SetPitchBank(Banks bank, float pitch)
        {
            if (pitch < 0 || pitch > 2)
            {
                Debug.LogWarning("Pitch must be between 0 and 2");
                return;
            }
            switch (bank)
            {
                case Banks.AMB:
                    SetPitchForInstances(ambInstances, pitch);
                    break;
                case Banks.BGM:
                    SetPitchForInstances(bgmInstances, pitch);
                    break;
                case Banks.SFX:
                    SetPitchForInstances(sfxInstances, pitch);
                    break;
                default:
                    Debug.LogWarning("Unknown sound type: " + bank);
                    break;
            }
        }

        private void SetPitchForInstances(List<EventInstance> instanceList, float pitch)
        {
            foreach (var instance in instanceList)
            {
                instance.setPitch(pitch);
            }
        }

        public void SetVolumeBank(Banks bank, float volume)
        {
            if (volume < 0 || volume > 1)
            {
                Debug.LogWarning("Volume must be between 0 and 1");
                return;
            }
            switch (bank)
            {
                case Banks.AMB:
                    SetVolumeForInstances(ambInstances, volume);
                    break;
                case Banks.BGM:
                    SetVolumeForInstances(bgmInstances, volume);
                    break;
                case Banks.SFX:
                    SetVolumeForInstances(sfxInstances, volume);
                    break;
                default:
                    Debug.LogWarning("Unknown sound type: " + bank);
                    break;
            }
        }

        private void SetVolumeForInstances(List<EventInstance> instanceList, float volume)
        {
            foreach (var instance in instanceList)
            {
                instance.setVolume(volume);
            }
        }

        public void StopAllSounds()
        {
            StopInstances(bgmInstances);
            StopInstances(sfxInstances);
            StopInstances(ambInstances);
        }

        private void StopInstances(List<EventInstance> instanceList)
        {
            foreach (var instance in instanceList)
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                instance.release();
            }
            instanceList.Clear();
        }

        private void OnDestroy()
        {
            StopAllSounds();
            StopDead();
        }
    }
}
