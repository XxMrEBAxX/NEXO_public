using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace BirdCase
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField]
        private Color warningColor;
        [SerializeField, Range(0, 1)]
        private float warningTime;
        [SerializeField]
        private float warningEffectTime = 1f;
        [SerializeField]
        private AnimationCurve warningTextSizeCurve;
        [SerializeField]
        private Vector3 textWarningSize;
        
        private TMP_Text timerUI;
        private bool isWarning = false;

        private void Awake()
        {
            timerUI = GetComponent<TMP_Text>();
        }

        public void SetTimerUI(float endTime, float curTime)
        {
            if (!isWarning && (endTime - curTime) <= endTime * warningTime)
            {
                isWarning = true;
                WarningTimeEffect().Forget();
            }
            
            float time = Mathf.Max(endTime - curTime, 0);
            timerUI.text = string.Format("{0:00}:{1:00}", (int)(time / 60), (int)(time % 60));
        }

        private async UniTaskVoid WarningTimeEffect()
        {
            Color originColor = timerUI.color;
            Vector3 originalSize = timerUI.transform.localScale;
            float elapsedTime = 0;
            while (elapsedTime < warningEffectTime)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float t = warningTextSizeCurve.Evaluate(elapsedTime / warningEffectTime);
                timerUI.transform.localScale = Vector3.LerpUnclamped(originalSize, textWarningSize, t);
                timerUI.color = Color.LerpUnclamped(originColor, warningColor, elapsedTime / warningEffectTime);
                
                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
            timerUI.transform.localScale = originalSize;
            timerUI.color = warningColor;
        }
    }
}
