using UnityEngine;

namespace BirdCase
{
    public class ShakeScreenSetting : MonoBehaviour
    {
        private const int DEFAULT_SHAKESCREEN = 1;

        public bool IsChangedShakeScreen { get; private set; }
        public GameObject CheckMark;
        public GameObject UnCheckMark;

        private bool isShakeState = true; // 기본 상태와 같은 값 유지
        private int currentShakeScreen;
        private int prevShakeScreen;
        
        #region SettingManager

        public void InitShakeScreen()
        {
            SetPlayerPrefsShakeScreen(DEFAULT_SHAKESCREEN);
            SetShakeScreen(DEFAULT_SHAKESCREEN);
            
            currentShakeScreen = DEFAULT_SHAKESCREEN;
            prevShakeScreen = DEFAULT_SHAKESCREEN;
            
            CheckMark.SetActive(DEFAULT_SHAKESCREEN == 1);
            UnCheckMark.SetActive(DEFAULT_SHAKESCREEN == 0);
            
            isShakeState = DEFAULT_SHAKESCREEN == 1;
            IsChangedShakeScreen = false;
        }

        public void ApplyShakeScreen()
        {
            SetPlayerPrefsShakeScreen(currentShakeScreen);
            
            CheckMark.SetActive(currentShakeScreen == 1);
            UnCheckMark.SetActive(currentShakeScreen == 0);
            
            prevShakeScreen = currentShakeScreen;
            isShakeState = currentShakeScreen == 1;
            IsChangedShakeScreen = false;
        }

        public void LoadShakeScreen()
        {
            if (!PlayerPrefs.HasKey("ShakeScreen"))
            {
                InitShakeScreen();
                return;
            }

            currentShakeScreen = PlayerPrefs.GetInt("ShakeScreen");
            prevShakeScreen = currentShakeScreen;
            
            CheckMark.SetActive(currentShakeScreen == 1);
            UnCheckMark.SetActive(currentShakeScreen == 0);
            SetShakeScreen(currentShakeScreen);
            
            isShakeState = currentShakeScreen == 1;
            IsChangedShakeScreen = false;
        }

        public void CancelShakeScreen()
        {
            SetPlayerPrefsShakeScreen(prevShakeScreen);
            SetShakeScreen(prevShakeScreen);
            
            CheckMark.SetActive(prevShakeScreen == 1);
            UnCheckMark.SetActive(prevShakeScreen == 0);
            
            currentShakeScreen = prevShakeScreen;
            isShakeState = prevShakeScreen == 1;
            IsChangedShakeScreen = false;
        }

        #endregion

        /// <summary>
        /// ShakeScreen 버튼을 누르면 ShakeScreen 설정을 변경합니다.
        /// </summary>
        public void ShakeScreenButton()
        {
            currentShakeScreen = currentShakeScreen == 0 ? 1 : 0;
            SetShakeScreen(currentShakeScreen);
        }

        /// <summary>
        /// 0 : 화면 흔들림 끔, 1 : 화면 흔들림 켬
        /// </summary>
        /// <param name="shakeScreen"></param>
        private void SetShakeScreen(int shakeScreen)
        {
            bool isShakeScreen = shakeScreen == 1;
            
            if (isShakeScreen == isShakeState)
            {
                CheckMark.SetActive(isShakeScreen);
                UnCheckMark.SetActive(!isShakeScreen);
                IsChangedShakeScreen = false;
                return;
            }
            
            CameraManager.IsActiveShake = isShakeScreen;
            CheckMark.SetActive(isShakeScreen);
            UnCheckMark.SetActive(!isShakeScreen);
            IsChangedShakeScreen = true;
        }

        private void SetPlayerPrefsShakeScreen(int shakeScreen)
        {
            PlayerPrefs.SetInt("ShakeScreen", shakeScreen);
        }
    }
}