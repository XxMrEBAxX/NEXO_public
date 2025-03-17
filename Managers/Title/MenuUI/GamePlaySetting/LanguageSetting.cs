using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace BirdCase
{
    public class LanguageSetting : MonoBehaviour
    {
        private readonly string[] LANGUAGE = { "한국어", "English", "简体字", "日本語", "Deutsch", "Italiano", "Français", "Español", "język polski", "Türkçe" };
        // 중국, 일본, 독일, 이탈, 프랑스, 스페인, 폴란드, 터키
        private readonly string[] LANGUAGE_IDENTIFIER = { "ko", "en", "zh", "ja", "de", "it", "fr-FR", "es", "pl", "tr" };
        private const int DEFAULT_LANGUAGE = 0;
        
        public bool IsChangedLanguage { get; private set; } // Manager에서 값이 변경되었는 지 확인합니다.

        [SerializeField] private TMP_Dropdown dropdown;

        private int currentLanguageIndex;
        private int prevLanguageIndex;

        #region SettingManager

        /// <summary>
        /// 언어 설정 초기화 함수
        /// </summary>
        public void InitSetting()
        {
            LanguageDropdownSetting();
            
            SetPlayerPrefsLanguage(DEFAULT_LANGUAGE);
            LoadLocate(LANGUAGE_IDENTIFIER[DEFAULT_LANGUAGE]);
            currentLanguageIndex = DEFAULT_LANGUAGE;
            prevLanguageIndex = DEFAULT_LANGUAGE;
            
            IsChangedLanguage = false;
            dropdown.onValueChanged.AddListener(OnDropdownEvent);
        }

        /// <summary>
        /// 현재 선택한 언어로 저장합니다.
        /// </summary>
        public void ApplyLanguage()
        {
            SetPlayerPrefsLanguage(currentLanguageIndex);
            prevLanguageIndex = currentLanguageIndex;
            IsChangedLanguage = false;
        }

        public void LoadLanguage()
        {
            if (!PlayerPrefs.HasKey("Language"))
            {
                InitSetting();
                return;
            }

            LanguageDropdownSetting();
            currentLanguageIndex = PlayerPrefs.GetInt("Language");
            prevLanguageIndex = currentLanguageIndex;
            dropdown.value = currentLanguageIndex;
            LoadLocate(LANGUAGE_IDENTIFIER[currentLanguageIndex]);
            IsChangedLanguage = false;
            
            dropdown.onValueChanged.AddListener(OnDropdownEvent);
        }

        /// <summary>
        /// 언어 변경을 취소합니다.
        /// </summary>
        public void CancelLanguage()
        {
            LoadLocate(LANGUAGE_IDENTIFIER[prevLanguageIndex]);
            SetPlayerPrefsLanguage(prevLanguageIndex);
            dropdown.value = prevLanguageIndex;
            currentLanguageIndex = prevLanguageIndex;
            IsChangedLanguage = false;
        }

        #endregion

        /// <summary>
        /// 현재 언어를 반환합니다.
        /// </summary>
        public string CurrentLanguage()
        {
            if (!PlayerPrefs.HasKey("Language"))
            {
                return LANGUAGE[DEFAULT_LANGUAGE];
            }

            return LANGUAGE[PlayerPrefs.GetInt("Language")];
        }

        private void LanguageDropdownSetting()
        {
            dropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();
            foreach (string str in LANGUAGE)
            {
                optionList.Add(new TMP_Dropdown.OptionData(str));
            }

            dropdown.AddOptions(optionList);
        }
        
        private void OnDropdownEvent(int index)
        {
            currentLanguageIndex = index;
            if (currentLanguageIndex == prevLanguageIndex) // 이전과 같은 언어를 선택했을 때 변경되지 않도록 설정
            {
                LoadLocate(LANGUAGE_IDENTIFIER[index]);
                IsChangedLanguage = false;
                return;
            }

            LoadLocate(LANGUAGE_IDENTIFIER[index]);
            dropdown.value = index;
        }

        private void LoadLocate(string languageIdentifier)
        {
            IsChangedLanguage = true;

            LocaleIdentifier localeCode = new LocaleIdentifier(languageIdentifier);
            for (var i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                Locale aLocale = LocalizationSettings.AvailableLocales.Locales[i];
                LocaleIdentifier anIdentifier = aLocale.Identifier;
                if (anIdentifier == localeCode)
                {
                    LocalizationSettings.SelectedLocale = aLocale;
                    break;
                }
            }
        }

        private void SetPlayerPrefsLanguage(int languageIndex)
        {
            PlayerPrefs.SetInt("Language", languageIndex);
        }
    }
}