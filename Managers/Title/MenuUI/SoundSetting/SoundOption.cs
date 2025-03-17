using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class SoundOption : MonoBehaviour
    {
        private readonly float DEfAULT_MAIN_VOLME = 1f;
        private readonly float DEFAULT_BGM_VOLUME = 1f;
        private readonly float DEFAULT_SFX_VOLUME = 1f;
        public bool IsChangedSound { get; private set; }

        public Slider MainSlider;
        public Slider BgmSlider;
        public Slider SfxSlider;

        private float currentMainVolume;
        private float prevMainVolume;

        private float currentBGMVolume;
        private float prevBGMVolume;

        private float currentSFXVolume;
        private float prevSFXVolume;
        
        #region SettingManager

        public void InitSound()
        {
            SetPlayerPrefsBGMSound(DEfAULT_MAIN_VOLME);
            prevMainVolume = DEfAULT_MAIN_VOLME;
            currentMainVolume = DEfAULT_MAIN_VOLME;

            SetPlayerPrefsBGMSound(DEFAULT_BGM_VOLUME);
            prevBGMVolume = DEFAULT_BGM_VOLUME;
            currentBGMVolume = DEFAULT_BGM_VOLUME;

            SetPlayerPrefsSFXSound(DEFAULT_SFX_VOLUME);
            prevSFXVolume = DEFAULT_SFX_VOLUME;
            currentSFXVolume = DEFAULT_SFX_VOLUME;

            SetSound(DEfAULT_MAIN_VOLME, DEFAULT_BGM_VOLUME, DEFAULT_SFX_VOLUME);
            
            IsChangedSound = false;
        }
        
        public void LoadSound()
        {
            if (!PlayerPrefs.HasKey("MainVolume") || !PlayerPrefs.HasKey("BGMVolume") || !PlayerPrefs.HasKey("SFXVolume"))
            {
                InitSound();
                return;
            }

            currentMainVolume = PlayerPrefs.GetFloat("MainVolume");
            prevMainVolume = currentMainVolume;

            currentBGMVolume = PlayerPrefs.GetFloat("BGMVolume");
            prevBGMVolume = currentBGMVolume;

            currentSFXVolume = PlayerPrefs.GetFloat("SFXVolume");
            prevSFXVolume = currentSFXVolume;

            SetSound(currentMainVolume, currentBGMVolume, currentSFXVolume);

            IsChangedSound = false;
        }
        
        public void ApplySound()
        {
            SetPlayerPrefsMainSound(currentMainVolume);
            prevMainVolume = currentMainVolume;
            
            SetPlayerPrefsBGMSound(currentBGMVolume);
            prevBGMVolume = currentBGMVolume;
            
            SetPlayerPrefsSFXSound(currentSFXVolume);
            prevSFXVolume = currentSFXVolume;

            IsChangedSound = false;
        }

        public void CancelSound()
        {
            SetSound(prevMainVolume, prevBGMVolume, prevSFXVolume);

            currentMainVolume = prevMainVolume;
            currentBGMVolume = prevBGMVolume;
            currentSFXVolume = prevSFXVolume;

            IsChangedSound = false;
        }

        #endregion

        #region SoundSetting

        private void SetSound(float mainVolume, float bgmVolume, float sfxVolume)
        {
            SoundManager.MainVolume = mainVolume;
            MainSlider.value = mainVolume;
            
            SoundManager.BgmVolume = bgmVolume;
            BgmSlider.value = bgmVolume;
            
            SoundManager.SfxVolume = sfxVolume;
            SfxSlider.value = sfxVolume;
        }

        public void RecordMainVolume()
        {
            IsChangedSound = true;
            currentMainVolume = Mathf.Floor(MainSlider.value * 100f) / 100f;
            SoundManager.MainVolume = currentMainVolume;
        }

        public void RecordBGMVolume()
        {
            IsChangedSound = true;
            currentBGMVolume = Mathf.Floor(BgmSlider.value * 100f) / 100f;
            SoundManager.BgmVolume = currentBGMVolume;
        }

        public void RecordSFXVolume()
        {
            IsChangedSound = true;
            currentSFXVolume = Mathf.Floor(SfxSlider.value * 100f) / 100f;
            SoundManager.SfxVolume = currentSFXVolume;
        }

        #endregion

        #region PlayerPrefs

        private void SetPlayerPrefsMainSound(float value)
        {
            PlayerPrefs.SetFloat("MainVolume", value);
        }

        private void SetPlayerPrefsBGMSound(float value)
        {
            PlayerPrefs.SetFloat("BGMVolume", value);
        }

        private void SetPlayerPrefsSFXSound(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        #endregion
    }
}