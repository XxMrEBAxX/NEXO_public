using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using FMODUnity;

namespace BirdCase
{
    public class CreditUI : MonoBehaviour
    {
        private TypingCreditText typingCreditText;
        private ActiveCreditPanel activePanelEffect;
        private CreditPanelUI creditPanelUI;
        private bool finishCredit;
        [SerializeField] private GameObject fadeImage;

        [SerializeField] private EventReference creditFadeSound;
        [SerializeField] private EventReference creditBGMSound;

        private void Awake()
        {
            typingCreditText = FindFirstObjectByType<TypingCreditText>();
            activePanelEffect = FindFirstObjectByType<ActiveCreditPanel>();
            creditPanelUI = FindFirstObjectByType<CreditPanelUI>();
            fadeImage.SetActive(true);
            typingCreditText.SetActive(false);
        }

        private void Start()
        {
            finishCredit = false;
            TimeManager.Instance.TimeScale = 0;
            SoundManager.Instance.StopAllSounds();
            EndCredit().Forget();
        }

        private void Update()
        {
            if (finishCredit || Time.timeScale == 0)
            {
                return;
            }

            if (Keyboard.current.spaceKey.isPressed)
            {
                TimeManager.Instance.TimeScale = 4;
                SoundManager.Instance.pitch = 2;
            }
            else
            {
                TimeManager.Instance.TimeScale = 1;
                SoundManager.Instance.pitch = 1;
            }
        }

        private async UniTaskVoid EndCredit()
        {
            await Fade();
            await UniTask.WaitUntil(() => typingCreditText.EraseStart, PlayerLoopTiming.Update,
                this.GetCancellationTokenOnDestroy());
            activePanelEffect.SetSmile();
            creditPanelUI.IsPlaying = false;
            await UniTask.WaitUntil(() => typingCreditText.TypingCompleted, PlayerLoopTiming.Update,
                this.GetCancellationTokenOnDestroy());

            finishCredit = true;
            TimeManager.Instance.TimeScale = 1;
            SoundManager.Instance.pitch = 1;
            await FadeOut();
            SceneManager.LoadScene(1);
            SoundManager.Instance.PlayMainMenuBGM();
        }

        private async UniTask Fade()
        {
            SoundManager.Instance.Play(creditFadeSound, SoundManager.Banks.SFX);
            await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.UnscaledDeltaTime);
            fadeImage.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f), DelayType.UnscaledDeltaTime);
            fadeImage.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f), DelayType.UnscaledDeltaTime);
            fadeImage.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f), DelayType.UnscaledDeltaTime);
            fadeImage.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.UnscaledDeltaTime);
            fadeImage.SetActive(false);
            TimeManager.Instance.TimeScale = 1;
            SoundManager.Instance.pitch = 1;
            typingCreditText.SetActive(true);
            SoundManager.Instance.Play(creditBGMSound, SoundManager.Banks.BGM);
        }

        private async UniTask FadeOut()
        {
            fadeImage.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f), DelayType.UnscaledDeltaTime);
            fadeImage.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f), DelayType.UnscaledDeltaTime);
            fadeImage.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f), DelayType.UnscaledDeltaTime);
            fadeImage.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.UnscaledDeltaTime);
            fadeImage.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), DelayType.UnscaledDeltaTime);
        }
    }
}