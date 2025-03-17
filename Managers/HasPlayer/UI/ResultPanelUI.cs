using System;
using Cysharp.Threading.Tasks;
using FMODUnity;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BirdCase
{
    public class ResultPanelUI : MonoBehaviour
    {
        [SerializeField]
        private float fadeTime = 0.5f;
        [SerializeField]
        private AnimationCurve fadeSizeCurve;
        [SerializeField]
        private TMP_Text commonResultText;
        [SerializeField]
        private TMP_Text riaResultText;
        [SerializeField]
        private TMP_Text niaResultText;

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private PlayData playData;

        [SerializeField] EventReference winSound;
        [SerializeField] EventReference defeatSound;

        private GameObject[] child;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.enabled = true;
            canvasGroup.alpha = 0;
            rectTransform = commonResultText.transform.parent.GetComponent<RectTransform>();

            child = new GameObject[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                child[i] = transform.GetChild(i).gameObject;
                child[i].SetActive(false);
            }
        }

        public void ShowResultPanel(PlayData playData)
        {
            foreach (var obj in child)
            {
                obj.SetActive(true);
            }
            
            SetData(playData);
            rectTransform.anchoredPosition = Vector3.zero;
            FadeIn().Forget();

            if (playData.IsBossKilled)
            {
                SoundManager.Instance.Play(winSound, SoundManager.Banks.SFX);
            }
            else
            {
                SoundManager.Instance.Play(defeatSound, SoundManager.Banks.SFX);
            }
        }

        private void SetData(PlayData playData)
        {
            commonResultText.text = playData.GetCommonData();
            riaResultText.text = playData.GetRiaData();
            niaResultText.text = playData.GetNiaData();
            this.playData = playData;
        }

        private async UniTaskVoid FadeIn()
        {
            float elapsedTime = 0;
            Vector3 originSize = canvasGroup.transform.localScale;
            while (elapsedTime < fadeTime)
            {
                elapsedTime += TimeManager.Instance.GetUnscaledDeltaTime();

                canvasGroup.alpha = elapsedTime / fadeTime;
                canvasGroup.transform.localScale =
                    Vector3.LerpUnclamped(Vector3.zero, originSize, fadeSizeCurve.Evaluate(elapsedTime / fadeTime));

                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
            canvasGroup.alpha = 1;
            canvasGroup.transform.localScale = originSize;
        }

        public void GoToTitleBtn()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                TimeManager.Instance.TimeScale = TimeManager.Instance.OriginTimeScale;
                TimeManager.Instance.PlayerTimeScale = 1;
            }

            Time.timeScale = TimeManager.Instance.OriginTimeScale;

            GameManager.Instance.GameEnd();

            if (playData.IsBossKilled)
            {
                SceneManager.LoadScene("EndingScene");
            }
            else
            {
                GameManager.Instance.GoToTitle();
            }
        }
    }
}
