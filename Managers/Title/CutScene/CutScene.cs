using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class CutScene : Singleton<CutScene>
    {
        private CancellationTokenSource cts;

        [SerializeField]
        private float cutSceneEndDelay = 1.0f;
        [SerializeField]
        private Button skipButton;
        [SerializeField]
        private TMP_Text afterSkipText;

        private GameObject cutSceneObject;

        private bool isSkip = false;

        protected override void OnAwake()
        {
            cutSceneObject = transform.GetChild(0).gameObject;
        }

        private void Start()
        {
            DontDestroyOnLoad(this);
            cutSceneObject.SetActive(false);

            skipButton.onClick.AddListener(() => CheckCutSceneEnd(true).Forget());
        }

        private void Update()
        {
            if (!cutSceneObject.activeInHierarchy)
            {
                StopCutScene01();
                StopCutScene02();
                StopCutScene03();
            }
        }

        public void PlayCutScene()
        {
            cutSceneObject.SetActive(true);
            skipButton.gameObject.SetActive(true);
            afterSkipText.gameObject.SetActive(false);
            OnDestroy();
            cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            OnDestroy();
        }

        private void OnDestroy()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        public void CheckCutSceneEnd()
        {
            CheckCutSceneEnd(false).Forget();
        }

        private async UniTaskVoid CheckCutSceneEnd(bool isPressSkipBtn = false)
        {
            if (!isSkip)
            {
                isSkip = true;

                if (!isPressSkipBtn)
                    await UniTask.Delay(TimeSpan.FromSeconds(cutSceneEndDelay), DelayType.UnscaledDeltaTime, cancellationToken: cts.Token);

                InGamePlayManager.CutSceneEndEvent += DisableCutScene;
                InGamePlayManager.IsCutSceneEnd = true;
                skipButton.gameObject.SetActive(false);
                afterSkipText.gameObject.SetActive(true);
            }
        }

        private void DisableCutScene()
        {
            cutSceneObject.SetActive(false);
            isSkip = false;
            InGamePlayManager.CutSceneEndEvent -= DisableCutScene;
        }

        [SerializeField] private EventReference cutScene01;
        private EventInstance cutScene01Instance;

        [SerializeField] private EventReference cutScene02;
        private EventInstance cutScene02Instance;

        [SerializeField] private EventReference cutScene03;
        private EventInstance cutScene03Instance;

        public void PlayCutScene01()
        {
            if (!cutSceneObject.activeInHierarchy)
                return;

            cutScene01Instance = SoundManager.Instance.Play(cutScene01, SoundManager.Banks.SFX, 1);
        }

        public void PlayCutScene02()
        {
            if (!cutSceneObject.activeInHierarchy)
                return;

            cutScene02Instance = SoundManager.Instance.Play(cutScene02, SoundManager.Banks.SFX, 1);
        }

        public void PlayCutScene03()
        {
            if (!cutSceneObject.activeInHierarchy)
                return;

            cutScene03Instance = SoundManager.Instance.Play(cutScene03, SoundManager.Banks.SFX, 1);
        }

        public void StopCutScene01()
        {
            SoundManager.Instance.Stop(cutScene01Instance);
        }

        public void StopCutScene02()
        {
            SoundManager.Instance.Stop(cutScene02Instance);
        }

        public void StopCutScene03()
        {
            SoundManager.Instance.Stop(cutScene03Instance);
        }
    }
}
