using UnityEngine;

namespace BirdCase
{
    public class SettingPopupUI : MonoBehaviour
    {
        [SerializeField] private GameObject gamePlayPopup;
        [SerializeField] private GameObject graphicPopup;
        [SerializeField] private GameObject soundPopup;
        [SerializeField] private GameObject keyMappingPopup;

        #region ActivePopupSetting

        public void SetGamePlayPopup()
        {
            gamePlayPopup.SetActive(true);
            graphicPopup.SetActive(false);
            soundPopup.SetActive(false);
            keyMappingPopup.SetActive(false);
        }

        public void SetGraphicPopup()
        {
            gamePlayPopup.SetActive(false);
            graphicPopup.SetActive(true);
            soundPopup.SetActive(false);
            keyMappingPopup.SetActive(false);
        }

        public void SetSoundPopup()
        {
            gamePlayPopup.SetActive(false);
            graphicPopup.SetActive(false);
            soundPopup.SetActive(true);
            keyMappingPopup.SetActive(false);
        }

        public void SetKeyMappingPopup()
        {
            gamePlayPopup.SetActive(false);
            graphicPopup.SetActive(false);
            soundPopup.SetActive(false);
            keyMappingPopup.SetActive(true);
        }

        #endregion
    }
}