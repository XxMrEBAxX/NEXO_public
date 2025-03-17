using System;
using AssetKits.ParticleImage;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BirdCase
{
    public class ActivePanelEffect : MonoBehaviour
    {
        [SerializeField]
        private Vector3 fadeStartScale = new Vector3(0, 0.05f, 1.0f);
        [SerializeField]
        private float fadeDuration = 1f;
        [SerializeField]
        private float fadeDelay = 0.1f;
        [SerializeField]
        private AnimationCurve fadeCurve;

        private Image panelImage;
        private ParticleImage particleImage;
        
        private Vector3 oriSize;
        private bool isFadeIn = false;
        private bool isFadeOut = false;

        public bool IsFadeEnd => !isFadeIn && !isFadeOut;

        private void Awake()
        {
            panelImage = GetComponent<Image>();
            particleImage = GetComponentInChildren<ParticleImage>();
            oriSize = panelImage.transform.localScale;
        }

        private void Start()
        {
            panelImage.transform.localScale = fadeStartScale;
        }

        private void OnEnable()
        {
            particleImage?.Stop();
            isFadeOut = false;
            isFadeIn = true;
            Fade(oriSize).Forget();
        }

        public void Disable()
        {
            particleImage?.Stop();
            isFadeOut = true;
            isFadeIn = false;
            Fade(fadeStartScale, false).Forget();
        }

        private void OnDisable()
        {
            particleImage?.Stop();
            isFadeOut = false;
            isFadeIn = false;
            panelImage.transform.localScale = fadeStartScale;
        }

        private async UniTaskVoid Fade(Vector3 endSize, bool scaleXFirst = true)
        {
            float duration = fadeDuration * 0.5f;
            float elapsedTime = 0;
            Vector3 startSize = panelImage.transform.localScale;
            Vector3 resultSize = new Vector3(
                scaleXFirst ? endSize.x : startSize.x,
                scaleXFirst ? startSize.y : endSize.y, 1);
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime * 0.5f;

                if (!isFadeIn && !isFadeOut)
                {
                    particleImage?.Stop();
                    return;
                }

                float t = fadeCurve.Evaluate(elapsedTime / duration);
                panelImage.transform.localScale = Vector3.LerpUnclamped(startSize, resultSize, t);
                
                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
        
            elapsedTime = 0;
            startSize = resultSize;
            resultSize = endSize;
            particleImage?.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(fadeDelay), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime * 0.5f;

                if (!isFadeIn && !isFadeOut)
                {
                    particleImage?.Stop();
                    return;
                }
                
                panelImage.transform.localScale = Vector3.LerpUnclamped(startSize, resultSize, fadeCurve.Evaluate(elapsedTime / duration));
                
                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
            //particleImage?.Stop();
            if(!scaleXFirst)
                gameObject.SetActive(false);

            isFadeIn = false;
            isFadeOut = false;
        }
    }
}
