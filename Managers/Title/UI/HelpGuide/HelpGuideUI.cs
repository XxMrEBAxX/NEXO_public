using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;

namespace BirdCase
{
    public class HelpWindowUI : Singleton<HelpWindowUI>
    {
        public enum HelpType
        {
            COUNTER = 0,
            COUNTER_SUCCESS,
            LEFT_CLASH,
            RIGHT_CLASH,
            CLASHING,
            CLASH_FAIL,
            PLATFORM_DESTROY,
            DEAD,
            NEUTRALIZE,
            PHASE2,
            LASTPURY
        }

        [SerializeField] private GameObject helpWindow;
        private TMP_Text helpWindowText;
        private Queue<Tuple<string, float>> helpTextQueue = new Queue<Tuple<string, float>>();
        private UniTask helpTextTask;

        private string saveHelpText;
        private float helpTextDuration;
        private bool isProcessingQueue;

        private HelpGuideData helpGuideData;
        private ActivePanelEffect activePanelEffect;

        /// <summary>
        /// 도움창에 출력할 텍스트를 호출 받으면 대기열에 넣습니다.
        /// </summary>
        /// <param name="text">도움창에 표시할 Text 입니다.</param>
        /// <param name="duration">도움창이 얼마만큼 켜져 있을 지의 시간 입니다.</param>
        public void RequestHelpText(HelpType index)
        {
            if (PlayerPrefs.GetInt("HelpGuide") == 0)
                return;

            //helpTextQueue.Enqueue(new Tuple<string, float>(text, duration));
            //helpTextQueue.Enqueue(new Tuple<string, float>(helpGuideData.HelpGuideTexts[index].Value, helpGuideData.HelpGuideTexts[index].Key));
            helpTextQueue.Enqueue(Tuple.Create(helpGuideData.HelpGuideTexts[(int)index].Value, helpGuideData.HelpGuideTexts[(int)index].Key));

            if (!isProcessingQueue)
            {
                SetHelpUI();
            }
        }

        protected override void OnAwake() {}

        public int TestInt = 0;
        [ContextMenu("Test")]
        public void Test()
        {
            RequestHelpText((HelpType)TestInt);
        }

        private void Start()
        {
            helpWindowText = helpWindow.GetComponentInChildren<TMP_Text>();
            activePanelEffect = transform.GetComponentInChildren<ActivePanelEffect>();
            ActiveState(false);
            isProcessingQueue = false;

            helpGuideData =
                LocalizationSettings.AssetDatabase.GetLocalizedAsset<HelpGuideData>("HelpGuideTable", "HelpGuideKey");
        }

        private async void SetHelpUI()
        {
            isProcessingQueue = true;

            while (helpTextQueue.Count > 0)
            {
                Tuple<string, float> curQueue = helpTextQueue.Peek();
                saveHelpText = curQueue.Item1;
                helpTextDuration = curQueue.Item2;

                helpWindowText.text = saveHelpText;
                ActiveState(true);

                helpTextTask = DelayTask(helpTextDuration);
                await helpTextTask;

                activePanelEffect.Disable();
                await UniTask.WaitUntil(() => activePanelEffect.IsFadeEnd);
                helpTextQueue.Dequeue();
            }

            isProcessingQueue = false;
        }

        private static async UniTask DelayTask(float duration)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
        }

        private void ActiveState(bool state)
        {
            helpWindow.SetActive(state);
        }
    }
}