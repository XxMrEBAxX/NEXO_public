using UnityEngine;

namespace BirdCase
{
    public class FullScreenOption : MonoBehaviour
    {        
        // FullScreenState -> 0: 창모드, 1: 전체화면
        private const int DEFAULT_FULL_SCREEN_STATE = 1;
        public bool IsChangedFullScreen { get; private set; }

        [SerializeField] private ResolutionOption resolutionOption;
        [SerializeField] private GameObject checkMark;
        [SerializeField] private GameObject unCheckMark;
        
        private bool resolutionOptionCheckFullScreen = DEFAULT_FULL_SCREEN_STATE == 1;
        private int prevFullScreenIndex;
        private int currentFullScreenIndex;
        
        public void FullScreenButton()
        {
            SetFullScreenOption(currentFullScreenIndex == 0 ? 1 : 0);
        }
        
        public bool GetFullScreenState()
        {
            return resolutionOptionCheckFullScreen;
        }
        
        #region SettingManager

        public void InitFullScreen()
        {
            if (!PlayerPrefs.HasKey("FullScreenState"))
            {
                SetPlayerPrefsFullScreen(DEFAULT_FULL_SCREEN_STATE);
                SetFullScreenOption(DEFAULT_FULL_SCREEN_STATE);
            }

            prevFullScreenIndex = DEFAULT_FULL_SCREEN_STATE;
            currentFullScreenIndex = DEFAULT_FULL_SCREEN_STATE;
            IsChangedFullScreen = false;
        }

        public void ApplyFullScreen()
        {
            SetPlayerPrefsFullScreen(currentFullScreenIndex);
            prevFullScreenIndex = currentFullScreenIndex;            
            IsChangedFullScreen = false;
        }
        
        public void LoadFullScreen()
        {
            if(!PlayerPrefs.HasKey("FullScreenState"))
            {
                InitFullScreen();
                return;
            }
            
            currentFullScreenIndex = PlayerPrefs.GetInt("FullScreenState");
            SetFullScreenOption(currentFullScreenIndex);
            prevFullScreenIndex = currentFullScreenIndex;
            IsChangedFullScreen = false;
        }

        public void CancelFullScreen()
        {
            SetFullScreenOption(prevFullScreenIndex);
            currentFullScreenIndex = prevFullScreenIndex;           
            IsChangedFullScreen = false;
        }

        #endregion
        
        #region SetFullScreenSetting

        private void SetFullScreenOption(int setFullScreenIndex)
        {
            currentFullScreenIndex = setFullScreenIndex;
            switch (setFullScreenIndex)
            {
                case 0:
                    SetWindowSetting();
                    break;
                case 1:
                    SetFullScreenSetting();
                    break;
            }

            if (currentFullScreenIndex != prevFullScreenIndex)
            {
                IsChangedFullScreen = true;
            }
            else
            {
                IsChangedFullScreen = false;
            }
        }
        
        private void SetWindowSetting()
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;     
            Screen.fullScreen = false;
            
            resolutionOptionCheckFullScreen = false;
            resolutionOption.SetResolutionForFullScreenSetting(false);

            checkMark.SetActive(false);
            unCheckMark.SetActive(true);
        }

        private void SetFullScreenSetting()
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;            
            Screen.fullScreen = true;
            
            resolutionOptionCheckFullScreen = true;
            resolutionOption.SetResolutionForFullScreenSetting(true);

            checkMark.SetActive(true);
            unCheckMark.SetActive(false);
        }

        #endregion

        private void SetPlayerPrefsFullScreen(int fullScreenState)
        {
            PlayerPrefs.SetInt("FullScreenState", fullScreenState);
        }
    }
}