using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace BirdCase
{
    public class ResolutionOption : MonoBehaviour
    {
        public bool IsChangeResolution { get; private set; }

        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private FullScreenOption fullScreenOption;

        private List<Resolution> resolutions = new();
        private int defaultResolutionIndex;
        private int prevResolutionIndex;
        private int currentResolutionIndex;

        /// <summary>
        /// 전체화면 설정에서 전체화면일 때 드롭다운의 값을 변경합니다.
        /// </summary>
        public void SetResolutionForFullScreenSetting(bool isFullScreen)
        {
            if (isFullScreen)
            {
                // 전체화면으로 설정할 때 현재 해상도 크기로 Dropdown을 설정
                Resolution currentResolution = Screen.currentResolution;
                int matchingIndex = -1;
                for (int i = 0; i < dropdown.options.Count; i++)
                {
                    string optionText = dropdown.options[i].text;
                    string resolutionText = currentResolution.width + "x" + currentResolution.height;

                    if (optionText == resolutionText)
                    {
                        matchingIndex = i;
                        break;
                    }
                }

                if (matchingIndex != -1)
                {
                    dropdown.value = matchingIndex;
                    dropdown.interactable = false;
                    dropdown.RefreshShownValue();
                }
            }
            else
            {
                int index = PlayerPrefs.GetInt("Resolution");
                dropdown.value = index;
                dropdown.interactable = true;
                dropdown.RefreshShownValue();
            }
        }

        #region SettingManager

        public void InitResolution()
        {
            InitUI();
            SetPlayerPrefsResolution(defaultResolutionIndex);
            prevResolutionIndex = defaultResolutionIndex;
            currentResolutionIndex = defaultResolutionIndex;
            dropdown.value = defaultResolutionIndex;
            IsChangeResolution = false;
            dropdown.onValueChanged.AddListener(SetResolution);
        }

        public void ApplyResolution()
        {
            prevResolutionIndex = currentResolutionIndex;
            SetPlayerPrefsResolution(currentResolutionIndex);
            IsChangeResolution = false;
        }

        public void LoadResolution()
        {
            if (PlayerPrefs.HasKey("Resolution"))
            {
                InitResolution();
                return;
            }

            InitUI();
            currentResolutionIndex = PlayerPrefs.GetInt("Resolution");
            SetResolution(currentResolutionIndex);
            prevResolutionIndex = currentResolutionIndex;
            dropdown.value = currentResolutionIndex;
            IsChangeResolution = false;
            dropdown.onValueChanged.AddListener(SetResolution);
        }

        public void CancelResolution()
        {
            currentResolutionIndex = prevResolutionIndex;
            SetPlayerPrefsResolution(prevResolutionIndex);
            SetResolution(prevResolutionIndex);
            IsChangeResolution = false;
        }

        #endregion

        #region SetDropdownOption

        private void InitUI()
        {
            resolutions.Clear();
            foreach (Resolution value in Screen.resolutions)
            {
                resolutions.Add(value);
            }

            dropdown.options.Clear();
            dropdown.onValueChanged.RemoveAllListeners();

            HashSet<string> addedResolutions = new HashSet<string>();

            foreach (Resolution resolution in resolutions)
            {
                // 16:9 비율만 남기기
                float aspectRatio = (float)resolution.width / resolution.height;
                if (Mathf.Abs(aspectRatio - (16f / 9f)) > 0.01f)
                {
                    continue;
                }

                string resolutionText = resolution.width + "x" + resolution.height;
                if (!addedResolutions.Contains(resolutionText))
                {
                    TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                    optionData.text = resolutionText;
                    dropdown.options.Add(optionData);
                    addedResolutions.Add(resolutionText);

                    if (resolution.width == Screen.width && resolution.height == Screen.height)
                    {
                        defaultResolutionIndex = dropdown.options.Count - 1;
                    }
                }
            }

            dropdown.value = defaultResolutionIndex;
            dropdown.RefreshShownValue();
        }

        #endregion

        private void SetResolution(int index)
        {
            if (PlayerPrefs.GetInt("Resolution") == index)
                IsChangeResolution = false;
            else
                IsChangeResolution = true;

            string resolutionText = dropdown.options[index].text;
            string[] dimensions = resolutionText.Split('x');

            if (dimensions.Length != 2)
                return;

            FullScreenMode fullScreenMode = fullScreenOption.GetFullScreenState()
                ? FullScreenMode.ExclusiveFullScreen
                : FullScreenMode.Windowed;

            if (int.TryParse(dimensions[0], out int width) && int.TryParse(dimensions[1], out int height))
            {
                Screen.SetResolution(width, height, fullScreenMode);
            }
        }

        private void SetPlayerPrefsResolution(int resolutionIndex)
        {
            PlayerPrefs.SetInt("Resolution", resolutionIndex);
        }
    }
}