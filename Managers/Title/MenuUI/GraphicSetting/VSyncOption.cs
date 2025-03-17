using UnityEngine;

namespace BirdCase
{
    public class VSyncOption : MonoBehaviour
    {
        private const int DEFAULT_VSYNC_STATE = 0;
        public bool IsChangedVSync { get; private set; }
        public GameObject VSyncCheckMark;
        public GameObject VSyncUnCheckMark;

        private int currentVSync;
        private int prevVSync;

        #region SettingManager

        public void InitVSync()
        {
            SetPlayerPrefsVSync(DEFAULT_VSYNC_STATE);
            SetVSync(DEFAULT_VSYNC_STATE);

            currentVSync = DEFAULT_VSYNC_STATE;
            prevVSync = DEFAULT_VSYNC_STATE;

            VSyncCheckMark.SetActive(DEFAULT_VSYNC_STATE == 1);
            VSyncUnCheckMark.SetActive(DEFAULT_VSYNC_STATE == 0);

            IsChangedVSync = false;
        }

        public void ApplyVSync()
        {
            SetPlayerPrefsVSync(currentVSync);
            VSyncCheckMark.SetActive(currentVSync == 1);
            VSyncUnCheckMark.SetActive(currentVSync == 0);
            IsChangedVSync = false;
            prevVSync = currentVSync;
        }

        public void LoadVSync()
        {
            if (!PlayerPrefs.HasKey("VSync"))
            {
                InitVSync();
                return;
            }

            currentVSync = PlayerPrefs.GetInt("VSync");
            prevVSync = currentVSync;

            SetVSync(currentVSync);

            IsChangedVSync = false;
        }

        public void CancelVSync()
        {
            SetPlayerPrefsVSync(prevVSync);
            SetVSync(prevVSync);
            currentVSync = prevVSync;
            IsChangedVSync = false;
        }

        #endregion

        /// <summary>
        /// VSync 버튼을 누르면 VSync 설정을 변경합니다.
        /// </summary>
        public void VSyncButton()
        {
            currentVSync = currentVSync == 0 ? 1 : 0;
            SetVSync(currentVSync);
        }

        /// <summary>
        /// 0 : VSync 끔, 1 : VSync 켬
        /// </summary>
        /// <param name="vSync"></param>
        private void SetVSync(int vSync)
        {
            bool notActiveVSync = vSync == 0;
            QualitySettings.vSyncCount = vSync;
            VSyncCheckMark.SetActive(!notActiveVSync);
            VSyncUnCheckMark.SetActive(notActiveVSync);
            
            if (currentVSync == prevVSync)
            {
                IsChangedVSync = false;
                return;
            }
            
            IsChangedVSync = true;
        }

        private void SetPlayerPrefsVSync(int vSync)
        {
            PlayerPrefs.SetInt("VSync", vSync);
        }
    }
}