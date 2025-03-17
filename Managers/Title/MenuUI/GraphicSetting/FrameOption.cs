using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace BirdCase
{
    public class FrameOption : MonoBehaviour
    {
        public bool IsChangeFrame { get; private set; }

        [SerializeField] private TMP_Dropdown frameDropdown;
        private List<Resolution> options = new();

        private int defaultFrameIndex; // dropdown의 value를 저장한 후 그 중 현재 해상도를 기본값으로 설정합니다.
        private int prevFrameIndex;
        private int currentFrameIndex;

        #region SettingManager

        public void InitFrame()
        {
            InitUI();

            SetPlayerPrefsFrame(defaultFrameIndex);
            SetRefreshRate(defaultFrameIndex);

            prevFrameIndex = defaultFrameIndex;
            currentFrameIndex = defaultFrameIndex;
            frameDropdown.value = defaultFrameIndex;
            IsChangeFrame = false;
            
            frameDropdown.onValueChanged.AddListener(SetRefreshRate);
        }

        public void ApplyFrame()
        {
            SetPlayerPrefsFrame(currentFrameIndex);
            frameDropdown.value = currentFrameIndex;
            prevFrameIndex = currentFrameIndex;
            IsChangeFrame = false;
        }

        public void LoadFrame()
        {
            if (!PlayerPrefs.HasKey("Frame"))
            {
                InitFrame();
                return;
            }

            InitUI();
            currentFrameIndex = PlayerPrefs.GetInt("Frame");
            frameDropdown.value = currentFrameIndex;
            SetRefreshRate(currentFrameIndex);
            frameDropdown.onValueChanged.AddListener(SetRefreshRate);
        }

        public void CancelFrame()
        {
            currentFrameIndex = prevFrameIndex;
            frameDropdown.value = prevFrameIndex;
            SetPlayerPrefsFrame(prevFrameIndex);
            SetRefreshRate(prevFrameIndex);
            IsChangeFrame = false;
        }

        #endregion

        #region SetDropdownOption

        private void InitUI()
        {
            options.Clear();
            foreach (Resolution value in Screen.resolutions)
            {
                options.Add(value);
            }

            frameDropdown.options.Clear();
            frameDropdown.onValueChanged.RemoveAllListeners();

            HashSet<string> addedRefreshRates = new HashSet<string>();

            foreach (Resolution resolution in options)
            {
                float refreshRate = Mathf.Round((float)resolution.refreshRateRatio.value);
                string refreshRateText = refreshRate + "Hz";

                if (!addedRefreshRates.Contains(refreshRateText))
                {
                    TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                    optionData.text = refreshRateText;
                    frameDropdown.options.Add(optionData);
                    addedRefreshRates.Add(refreshRateText);
                }
            }

            SortDropdownOptions();
            FindClosestRefreshRate();

            frameDropdown.value = defaultFrameIndex;
            frameDropdown.RefreshShownValue();
        }

        /// <summary>
        /// dropdown의 옵션을 오름차순으로 정렬합니다.
        /// </summary>
        private void SortDropdownOptions()
        {
            var options = frameDropdown.options;
            var sortedOptions = options.OrderBy(option => ExtractNumber(option.text)).ToList();

            frameDropdown.options.Clear();
            frameDropdown.AddOptions(sortedOptions.Select(option => option.text).ToList());
            frameDropdown.RefreshShownValue();
        }

        /// <summary>
        /// 현재 리프레시 레이트를 찾습니다.
        /// </summary>
        private void FindClosestRefreshRate()
        {
            float currentRefreshRate = Mathf.Round(Screen.currentResolution.refreshRateRatio.numerator /
                                                   (float)Screen.currentResolution.refreshRateRatio.denominator);

            if (frameDropdown.options.Count > 0)
            {
                float closestDifference = float.MaxValue;
                for (int i = 0; i < frameDropdown.options.Count; i++)
                {
                    float optionRefreshRate = ExtractNumber(frameDropdown.options[i].text);
                    float difference = Mathf.Abs(optionRefreshRate - currentRefreshRate);

                    if (difference < closestDifference)
                    {
                        closestDifference = difference;
                        defaultFrameIndex = i;
                    }
                }
            }
        }

        /// <summary>
        /// dropdown의 text에서 숫자만 추출합니다.
        /// </summary>
        private int ExtractNumber(string text)
        {
            return int.Parse(text.Replace("Hz", ""));
        }

        #endregion

        private void SetRefreshRate(int index)
        {
            if (frameDropdown.options.Count <= index)
            {
                return;
            }

            currentFrameIndex = index;
            frameDropdown.value = index;
            Application.targetFrameRate = ExtractNumber(frameDropdown.options[index].text);
            IsChangeFrame = true;
        }

        private void SetPlayerPrefsFrame(int frameIndex)
        {
            PlayerPrefs.SetInt("Frame", frameIndex);
        }
    }
}