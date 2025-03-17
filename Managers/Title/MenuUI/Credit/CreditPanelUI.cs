using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace BirdCase
{
    public class CreditPanelUI : MonoBehaviour
    {
        [SerializeField] 
        private GameObject backgroundPanel;
        [SerializeField] 
        private GameObject artParentObject;
        [FormerlySerializedAs("fadeCurve")] [SerializeField]
        private AnimationCurve fadeOutCurve;
        [SerializeField]
        private float showDuration = 5f;
        [SerializeField]
        private float fadeInDuration = 0.3f;
        [SerializeField]
        private float fadeOutDuration = 1f;
        [SerializeField]
        private float fadeDelay = 3f;
        
        private CanvasGroup canvasGroup;
        private GameObject[] artObjects;
        public bool IsPlaying { get; set; } = false;

        private void Awake()
        {
            canvasGroup = artParentObject.GetComponent<CanvasGroup>();
            artObjects = new GameObject[artParentObject.transform.childCount];
            for (int i = 0; i < artParentObject.transform.childCount; i++)
            {
                artObjects[i] = artParentObject.transform.GetChild(i).gameObject;
            }
        }
        
        private void Start()
        {
            foreach (var artObject in artObjects)
            {
                artObject.SetActive(false);
            }
            
            ChangeArt().Forget();
        }

        private async UniTaskVoid ChangeArt()
        {
            IsPlaying = true;
            int currentArtIndex = 0;
            while (IsPlaying)
            {
                await ShowPanelArt(currentArtIndex);
                backgroundPanel.SetActive(false);
                await UniTask.Delay(TimeSpan.FromSeconds(showDuration), DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                await HidePanelArt(currentArtIndex);
                backgroundPanel.SetActive(true);
                await UniTask.Delay(TimeSpan.FromSeconds(fadeDelay), DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                currentArtIndex = (currentArtIndex + 1) >= artObjects.Length ? 0 : currentArtIndex + 1;
            }
            
            backgroundPanel.SetActive(true);
            foreach (GameObject artObject in artObjects)
            {
                artObject.SetActive(false);
            }
        }

        private async UniTask ShowPanelArt(int index)
        {
            float elapsedTime = 0;
            artObjects[index].SetActive(true);
            canvasGroup.alpha = 0;
            while(elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                    
                canvasGroup.alpha = elapsedTime / fadeInDuration;
                
                await UniTask.Yield();
            }
            canvasGroup.alpha = 1;
        }
        
        private async UniTask HidePanelArt(int index)
        {
            float elapsedTime = 0;
            canvasGroup.alpha = 1;
            while(elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = fadeOutCurve.Evaluate(elapsedTime / fadeOutDuration);
                canvasGroup.alpha = t;
                
                await UniTask.Yield();
            }
            artObjects[index].SetActive(false);
            canvasGroup.alpha = 0;
        }
    }
}
