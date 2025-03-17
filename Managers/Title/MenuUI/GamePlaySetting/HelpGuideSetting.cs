using UnityEngine;

namespace BirdCase
{
    public class HelpGuideSetting : MonoBehaviour
    {
        private const int DEFAULT_HELPGUIDE = 1;

        public bool IsChangedHelpGuide { get; private set; }

        public GameObject CheckMark;
        public GameObject UnCheckMark;

        private int currentHelpGuide;
        private int prevHelpGuide;

        #region SettingManager

        public void InitHelpGuide()
        {
            SetPlayerPrefsHelpGuide(DEFAULT_HELPGUIDE);
            
            currentHelpGuide = DEFAULT_HELPGUIDE;
            prevHelpGuide = DEFAULT_HELPGUIDE;
            
            CheckMark.SetActive(DEFAULT_HELPGUIDE == 1);
            UnCheckMark.SetActive(DEFAULT_HELPGUIDE == 0);
            
            IsChangedHelpGuide = false;
        }

        public void ApplyHelpGuide()
        {
            SetPlayerPrefsHelpGuide(currentHelpGuide);
            
            CheckMark.SetActive(currentHelpGuide == 1);
            UnCheckMark.SetActive(currentHelpGuide == 0);
            
            IsChangedHelpGuide = false;
            prevHelpGuide = currentHelpGuide;
        }

        public void LoadHelpGuide()
        {
            if (!PlayerPrefs.HasKey("HelpGuide"))
            {
                InitHelpGuide();
                return;
            }

            currentHelpGuide = PlayerPrefs.GetInt("HelpGuide");
            prevHelpGuide = currentHelpGuide;
            CheckMark.SetActive(currentHelpGuide == 1);
            UnCheckMark.SetActive(currentHelpGuide == 0);
        }

        public void CancelHelpGuide()
        {
            SetPlayerPrefsHelpGuide(prevHelpGuide);
            CheckMark.SetActive(prevHelpGuide == 1);
            UnCheckMark.SetActive(prevHelpGuide == 0);
            IsChangedHelpGuide = false;
            currentHelpGuide = prevHelpGuide;
        }

        #endregion

        /// <summary>
        /// ShakeScreen 버튼을 누르면 ShakeScreen 설정을 변경합니다.
        /// </summary>
        public void HelpGuideButton()
        {
            currentHelpGuide = currentHelpGuide == 0 ? 1 : 0;

            if (prevHelpGuide == currentHelpGuide)
            {
                IsChangedHelpGuide = false;

                CheckMark.SetActive(prevHelpGuide == 1);
                UnCheckMark.SetActive(prevHelpGuide == 0);

                return;
            }

            IsChangedHelpGuide = true;

            CheckMark.SetActive(currentHelpGuide == 1);
            UnCheckMark.SetActive(currentHelpGuide == 0);
        }

        private void SetPlayerPrefsHelpGuide(int helpGuide)
        {
            PlayerPrefs.SetInt("HelpGuide", helpGuide);
        }
    }
}