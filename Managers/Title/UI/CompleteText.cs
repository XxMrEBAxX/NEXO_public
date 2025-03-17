using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class CompleteText : MonoBehaviour
    {
        private const float TOGGLE_ON_DELAY = 0.5f;
        
        [SerializeField]
        private TMP_ColorGradient unSelectedColor;
        [SerializeField]
        private TMP_ColorGradient selectedColor;

        private int completeCount = 0;
        private int currentToggleLength = 0;
        private Toggle[] toggle;
        private TextMeshProUGUI[] toggleText;
        private TextMeshProUGUI explanationText;
        private TextMeshProUGUI justText;
        private TypingText justTextTypingText;
        private TypingText explanationTextTypingText;

        public bool IsCompleteAll => completeCount == currentToggleLength;
        public bool IsToggleOn { get; private set; } = false;

        private void Awake()
        {
            justText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            justTextTypingText = justText.GetComponent<TypingText>();
            explanationText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            explanationTextTypingText = explanationText.GetComponent<TypingText>();
            
            toggle = GetComponentsInChildren<Toggle>();
            toggleText = new TextMeshProUGUI[toggle.Length];
            for(int i = 0; i < toggle.Length; i++)
            {
                toggleText[i] = toggle[i].GetComponentInChildren<TextMeshProUGUI>();
            }

            Reset();
        }

        public void SetText(string text)
        {
            justText.text = text;
            justTextTypingText.Typing();
            explanationTextTypingText.Clear();
            Reset();
        }

        public void SetText(string text, params string[] toggleTexts)
        {
            explanationText.text = text;
            explanationTextTypingText.Typing();
            justTextTypingText.Clear();
            Reset();
            currentToggleLength = toggleTexts.Length;
            for(int i = 0; i < toggleTexts.Length; i++)
            {
                toggleText[i].text = toggleTexts[i];
            }
            ToggleOn().Forget();
        }

        private async UniTaskVoid ToggleOn()
        {
            IsToggleOn = true;
            await UniTask.WaitUntil(() => explanationTextTypingText.TypingCompleted, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            for(int i = 0; i < currentToggleLength; i++)
            {
                toggle[i].gameObject.SetActive(true);
                await UniTask.Delay(TimeSpan.FromSeconds(TOGGLE_ON_DELAY), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
            IsToggleOn = false;
        } 
        
        public void Complete(int index)
        {
            completeCount++;
            toggle[index].isOn = true;
        }

        public bool IsComplete(int index)
        {
            return toggle[index].isOn;
        }

        private void Reset()
        {
            completeCount = 0;
            for(int i = 0; i < toggle.Length; i++)
            {
                toggle[i].isOn = false;
                toggle[i].gameObject.SetActive(false);
            }
        }

        public void SetSelected(int index)
        {
            bool isOn = toggle[index].isOn;
            if (isOn)
            {
                toggleText[index].color = selectedColor.bottomLeft;
                toggleText[index].colorGradientPreset = selectedColor;
                toggleText[index].fontStyle = FontStyles.Strikethrough;
            }
            else
            {
                toggleText[index].color = unSelectedColor.bottomLeft;
                toggleText[index].colorGradientPreset = unSelectedColor;
                toggleText[index].fontStyle = FontStyles.Normal;
            }
        }
    }
}
