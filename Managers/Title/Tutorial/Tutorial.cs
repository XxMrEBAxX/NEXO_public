using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BirdCase
{
    public class Tutorial : NetworkBehaviour
    {
        #region Serialized Fields
        
        [SerializeField]
        private Camera leftCamera;
        [SerializeField]
        private Camera rightCamera;
        [SerializeField]
        private Camera uiCamera;
        [SerializeField]
        private CinemachineCamera leftCinemachineCamera;
        [SerializeField]
        private CinemachineCamera rightCinemachineCamera;
        [SerializeField]
        private CinemachineCamera mainCinemachineCamera;
        
        [SerializeField]
        private Image riaCameraRenderTexture;
        [SerializeField]
        private Image niaCameraRenderTexture;
        
        [SerializeField]
        private float cameraLerpTime = 1.0f;
        
        [SerializeField] 
        private GameObject[] disableCharacterSelection;
        [SerializeField]
        private GameObject characterSelectionPanel;
        [SerializeField]
        private CanvasGroup hudPanel;
        [SerializeField]
        private GameObject skipBtnPanel;
        [SerializeField]
        private Image loadingPanel;

        [SerializeField]
        private GameObject[] playerDummy;
        [SerializeField]
        private WallFall[] wallFallToLeft;
        [SerializeField]
        private WallFall[] wallFallToRight;
        [SerializeField]
        private WallFall[] wallFallToBack;
        [SerializeField]
        private GameObject[] wallDisable;
        [SerializeField]
        private WallFall[] wallGoDown;
        #endregion
        
        #region Variables
        private CompleteText tutorialTextPanel;
        
        private ulong riaClientId;
        private ulong niaClientId;
        
        private PlayerType playerType;

        private PlayerSpawner playerSpawner;
        private PlayerInputController playerInputController;

        private CinemachineBrain mainCinemachineBrain;
        private Camera mainCamera;
        
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        
        private bool isOtherPlayerComplete = false;

        private PlayerBase localPlayer;
        private PlayerBase otherPlayer;
        
        private PlayerReviveDummy playerReviveDummy;
        private CounterMonsterDummy counterMonsterDummy;
        private ParticleSystem dummyEffect;
        
        private bool isSkipLocalCheck = false;
        private Button skipBtn;
        private Toggle[] skipPlayerToggle;
        
        private TutorialFlowData tutorialFlowData;
        
        #endregion
        
        private void Awake()
        {
            playerSpawner = FindObjectOfType<PlayerSpawner>();
            playerInputController = FindObjectOfType<PlayerInputController>();
            tutorialTextPanel = GetComponentInChildren<CompleteText>();
            playerReviveDummy = FindFirstObjectByType<PlayerReviveDummy>();
            counterMonsterDummy = FindFirstObjectByType<CounterMonsterDummy>();
            skipBtn = skipBtnPanel.GetComponentInChildren<Button>();
            skipPlayerToggle = skipBtnPanel.GetComponentsInChildren<Toggle>();
            dummyEffect = playerReviveDummy.transform.parent.GetComponentInChildren<ParticleSystem>();
        }

        private void Start()
        {
            leftCamera.rect = new Rect(0, 0, 0.5f, 1);
            rightCamera.rect = new Rect(0.5f, 0, 1f, 1);
            tutorialTextPanel.gameObject.SetActive(false);
            hudPanel.alpha = 0;
            playerInputController.gameObject.SetActive(false);
            playerReviveDummy.gameObject.SetActive(false);
            counterMonsterDummy.gameObject.SetActive(false);
            
            mainCamera = Camera.main;
            mainCinemachineCamera = mainCamera.transform.parent.GetComponent<CinemachineCamera>();
            mainCinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
            mainCinemachineBrain.enabled = false;
            
            loadingPanel.color = Color.black;
            loadingPanel.gameObject.SetActive(false);
            skipBtn.onClick.AddListener(() => SkipTutorial(NetworkManager.Singleton.LocalClientId));
            skipBtnPanel.SetActive(false);
            tutorialFlowData = LocalizationSettings.AssetDatabase.GetLocalizedAsset<TutorialFlowData>("Tutorial", "TutorialFlow");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            cancellationToken.Cancel();
            cancellationToken.Dispose();
        }

        #region Tutorial Start 
        
        [ClientRpc]
        public void StartTutorialClientRPC(ulong riaClientId, ulong niaClientId)
        {
            if (IsServer)
            {
                playerSpawner.ServerSceneInit(riaClientId, niaClientId, false);
            }

            this.riaClientId = riaClientId;
            this.niaClientId = niaClientId;
            playerType = NetworkManager.Singleton.LocalClientId == riaClientId ? PlayerType.LASER : PlayerType.LAUNCHER;
            TutorialCameraLerp(NetworkManager.Singleton.LocalClientId == this.riaClientId).Forget();
        }

        private async UniTaskVoid TutorialCameraLerp(bool chooseLeft)
        {
            
            float elapsedTime = 0;
            Rect rect;
            float oriX;
            if (chooseLeft)
            {
                rightCinemachineCamera.enabled = false;
                leftCinemachineCamera.Priority = 1;
                rect = leftCamera.rect;
                oriX = leftCamera.rect.x;
                riaCameraRenderTexture.transform.SetAsLastSibling();
            }
            else
            {
                leftCinemachineCamera.enabled = false;
                rightCinemachineCamera.Priority = 1;
                rect = rightCamera.rect;
                oriX = rightCamera.rect.x;
                niaCameraRenderTexture.transform.SetAsLastSibling();
            }

            foreach (GameObject obj in disableCharacterSelection)
            {
                obj.SetActive(false);
            }
            while (cameraLerpTime > elapsedTime)
            {
                elapsedTime += TimeManager.Instance.GetUnscaledDeltaTime();
                float t = elapsedTime / cameraLerpTime;

                rect.width = Mathf.Lerp(0.5f, 1, t);
                rect.x = Mathf.Lerp(oriX, 0, t);

                if (chooseLeft)
                {
                    leftCamera.rect = rect;
                    riaCameraRenderTexture.fillAmount = Mathf.Lerp(0.5f, 1, t);
                    niaCameraRenderTexture.fillAmount = Mathf.Lerp(0.5f, 0, t);
                }
                else
                {
                    rightCamera.rect = rect;
                    niaCameraRenderTexture.fillAmount = Mathf.Lerp(0.5f, 1, t);
                    riaCameraRenderTexture.fillAmount = Mathf.Lerp(0.5f, 0, t);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
            
            riaCameraRenderTexture.gameObject.SetActive(false);
            niaCameraRenderTexture.gameObject.SetActive(false);
            rightCamera.gameObject.SetActive(false);
            leftCamera.gameObject.SetActive(false);
            characterSelectionPanel.SetActive(false);

            mainCinemachineBrain.enabled = true;
            
            await UniTask.Delay(TimeSpan.FromSeconds(2.0f), cancellationToken: this.GetCancellationTokenOnDestroy());
            
            PlayerInit().Forget(); 
        }

        private async UniTaskVoid PlayerInit()
        {
            while (true)
            {
                if (PlayManager.Instance.IsAllPlayersReady)
                {
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
            
            rightCinemachineCamera.Priority = 0;
            leftCinemachineCamera.Priority = 0;
            mainCinemachineCamera.Priority = 1;
            
            await UniTask.WaitWhile(() => mainCinemachineBrain.IsBlending, cancellationToken: this.GetCancellationTokenOnDestroy());
            
            mainCamera.cullingMask |= LayerMask.GetMask("PlayerOne");
            mainCamera.cullingMask |= LayerMask.GetMask("PlayerTwo");
            mainCamera.cullingMask &= ~(LayerMask.GetMask("PlayerCollider"));
            foreach (GameObject player in playerDummy)
            {
                player.SetActive(false);
            }
            hudPanel.alpha = 1;
            playerInputController.gameObject.SetActive(true);
            UniversalAdditionalCameraData cameraData = mainCamera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Clear();
            cameraData.cameraStack.Add(uiCamera);
            
            foreach (WallFall wall in wallFallToRight)
            {
                wall.Fall(new Vector3(0, 0, -1), 90);
            }
            foreach (WallFall wall in wallFallToLeft)
            {
                wall.Fall(new Vector3(0, 0, 1), 90);
            }
            foreach (WallFall wall in wallFallToBack)
            {
                wall.Fall(new Vector3(1, 0, 0), 90);
            }
            foreach (GameObject wall in wallDisable)
            {
                wall.SetActive(false);
            }
            
            await UniTask.WaitWhile(() => wallFallToRight[0].IsMove, cancellationToken: this.GetCancellationTokenOnDestroy());
            
            foreach (WallFall wall in wallFallToRight)
            {
                wall.Go(new Vector3(0, -0.3f, 0));
            }
            foreach (WallFall wall in wallFallToLeft)
            {
                wall.Go(new Vector3(0, -0.3f, 0));
            }
            foreach (WallFall wall in wallFallToBack)
            {
                wall.Go(new Vector3(0, -0.3f, 0));
            }
            foreach (WallFall wall in wallGoDown)
            {
                wall.Go(new Vector3(0, -0.3f, 0));
            }
            
            PlayerInputController.CanInput = true;
            TutorialFlow().Forget();
        }

        #endregion

        #region Tutorial Flow

        private async UniTaskVoid TutorialFlow()
        {
            localPlayer = PlayManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId);
            otherPlayer = PlayManager.Instance.GetAnotherPlayer(NetworkManager.Singleton.LocalClientId);
            localPlayer.PlayerControlAllClientRPC(true);
            otherPlayer.PlayerControlAllClientRPC(true);
            
            skipBtnPanel.SetActive(true);
            tutorialTextPanel.gameObject.SetActive(true);
            await TextShow(tutorialFlowData.TutorialStartTexts);
            await WalkTutorial();
            SetTutorial();
            
            await AttackTutorial();
            SetTutorial();
            
            await SpecialAttackTutorial();
            SetTutorial();
            
            await CounterTutorial();
            SetTutorial();
            
            await ShieldTutorial();
            SetTutorial();
            
            await ReviveTutorial();
            SetTutorial();
            
            await TextShow(tutorialFlowData.TutorialEndTexts);
            EndTutorial(IsServer);
        }

        private void SetTutorial()
        {
            tutorialTextPanel.gameObject.SetActive(false);
            tutorialTextPanel.gameObject.SetActive(true);
            isOtherPlayerComplete = false;
        }

        private async UniTask TextShow(params KeyValuePair<float, string>[] str)
        {
            for(int i = 0; i < str.Length; i++)
            {
                tutorialTextPanel.SetText(str[i].Value);
                await UniTask.Delay(TimeSpan.FromSeconds(str[i].Key), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, cancellationToken.Token);
            }
        }
        
        [ServerRpc (RequireOwnership = false)]
        private void OtherPlayerCompleteServerRPC(ulong clientId)
        {
            OtherPlayerCompleteClientRPC(clientId);
        }
        
        [ClientRpc]
        private void OtherPlayerCompleteClientRPC(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
                return;
            
            isOtherPlayerComplete = true;
        }

        private KeyValuePair<float, string>[] ChooseText(TutorialData data)
        {
            if (data.isTextCommonness)
            {
                return data.TutorialText;
            }
            else
            {
                return playerType == PlayerType.LASER ? data.RiaTutorialText : data.NiaTutorialText;
            }
        }
        
        private string ChooseExplanationText(TutorialData data)
        {
            if (data.isExplanationCommonness)
            {
                return data.TutorialExplanationText;
            }
            else
            {
                return playerType == PlayerType.LASER ? data.RiaTutorialExplanationText : data.NiaTutorialExplanationText;
            }
        }

        private string[] ChooseConditionText(TutorialData data)
        {
            if (data.isConditionsCommonness)
            {
                return data.ConditionsText;
            }
            else
            {
                return playerType == PlayerType.LASER ? data.RiaConditionsText : data.NiaConditionsText;
            }
        }

        #region WalkTutorial
        
        private async UniTask WalkTutorial()
        {
            await TextShow(ChooseText(tutorialFlowData.WalkTutorial));
            tutorialTextPanel.SetText(ChooseExplanationText(tutorialFlowData.WalkTutorial), ChooseConditionText(tutorialFlowData.WalkTutorial));
            await UniTask.WaitUntil(() =>  !tutorialTextPanel.IsToggleOn, PlayerLoopTiming.Update, cancellationToken.Token);
            while (!tutorialTextPanel.IsCompleteAll)
            {
                if (!tutorialTextPanel.IsComplete(0) && Vector3.Distance(localPlayer.transform.position, otherPlayer.transform.position) < 10f)
                {
                    tutorialTextPanel.Complete(0);
                }

                if (!tutorialTextPanel.IsComplete(1) && localPlayer.IsJumping)
                {
                    tutorialTextPanel.Complete(1);
                    OtherPlayerCompleteServerRPC(NetworkManager.Singleton.LocalClientId);
                }

                if (!tutorialTextPanel.IsComplete(2) && isOtherPlayerComplete)
                {
                    tutorialTextPanel.Complete(2);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken.Token, true);
            }
        }

        #endregion

        #region AttackTutorial

        private bool isShot = false;
        private async UniTask AttackTutorial()
        {
            await TextShow(ChooseText(tutorialFlowData.DefaultAttackTutorial));
            tutorialTextPanel.SetText(ChooseExplanationText(tutorialFlowData.DefaultAttackTutorial), ChooseConditionText(tutorialFlowData.DefaultAttackTutorial));
            await UniTask.WaitUntil(() =>  !tutorialTextPanel.IsToggleOn, PlayerLoopTiming.Update, cancellationToken.Token);
            localPlayer.ShotEvent += IsShot;
            while (!tutorialTextPanel.IsCompleteAll)
            {
                if (!tutorialTextPanel.IsComplete(0) && isShot)
                {
                    tutorialTextPanel.Complete(0);
                    
                    if (tutorialTextPanel.IsComplete(1))
                    {
                        OtherPlayerCompleteServerRPC(NetworkManager.Singleton.LocalClientId);
                    }
                }

                if (!tutorialTextPanel.IsComplete(1) && localPlayer.IsReloading)
                {
                    tutorialTextPanel.Complete(1);

                    if (tutorialTextPanel.IsComplete(0))
                    {
                        OtherPlayerCompleteServerRPC(NetworkManager.Singleton.LocalClientId);
                    }
                }

                if (!tutorialTextPanel.IsComplete(2) && isOtherPlayerComplete)
                {
                    tutorialTextPanel.Complete(2);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken.Token, true);
            }
            localPlayer.ShotEvent -= IsShot;
            isShot = false;
        }
        
        private void IsShot(int max, int cur)
        {
            isShot = true;
        }
        
        #endregion

        #region SpecialAttackTutorial

        private bool attackToNiaSpecialAttack = false;
        private bool isSpecialShot = false;
        
        private async UniTask SpecialAttackTutorial()
        {
            await TextShow(ChooseText(tutorialFlowData.SpecialAttackTutorial));
            tutorialTextPanel.SetText(ChooseExplanationText(tutorialFlowData.SpecialAttackTutorial), ChooseConditionText(tutorialFlowData.SpecialAttackTutorial));
            await UniTask.WaitUntil(() =>  !tutorialTextPanel.IsToggleOn, PlayerLoopTiming.Update, cancellationToken.Token);
            await (playerType == PlayerType.LASER ? SpecialAttackTutorialRia() : SpecialAttackTutorialNia());
        }
        
        private async UniTask SpecialAttackTutorialRia()
        {
            LaserGun.IsAttackSpecialLauncherEvent += AttackSpecial;
            PlayerOne player = (PlayerOne)localPlayer;
            player.ChargeAttackEvent += CheckRiaSpecialAttack;
            while (!tutorialTextPanel.IsCompleteAll)
            {
                player.ReduceChargeAttackCooldown(1);
                
                if (!tutorialTextPanel.IsComplete(0) && isSpecialShot)
                {
                    tutorialTextPanel.Complete(0);
                    
                    if (tutorialTextPanel.IsComplete(1))
                    {
                        OtherPlayerCompleteServerRPC(NetworkManager.Singleton.LocalClientId);
                    }
                }

                if (!tutorialTextPanel.IsComplete(1) && attackToNiaSpecialAttack)
                {
                    tutorialTextPanel.Complete(1);

                    if (tutorialTextPanel.IsComplete(0))
                    {
                        OtherPlayerCompleteServerRPC(NetworkManager.Singleton.LocalClientId);
                    }
                }

                if (!tutorialTextPanel.IsComplete(2) && isOtherPlayerComplete)
                {
                    tutorialTextPanel.Complete(2);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken.Token, true);
            }
            LaserGun.IsAttackSpecialLauncherEvent -= AttackSpecial;
            player.ChargeAttackEvent -= CheckRiaSpecialAttack;
        }
        
        private async UniTask SpecialAttackTutorialNia()
        {
            Launcher.IsAttackSpecialLauncherEvent += AttackSpecial;
            PlayerTwo player = (PlayerTwo)localPlayer;
            player.SpecialShotCooldownEvent += CheckNiaSpecialAttack;
            while (!tutorialTextPanel.IsCompleteAll)
            {
                player.ReduceChargeAttackCooldown(1);
                
                if (!tutorialTextPanel.IsComplete(0) && isSpecialShot)
                {
                    tutorialTextPanel.Complete(0);
                    
                    if (tutorialTextPanel.IsComplete(1))
                    {
                        OtherPlayerCompleteServerRPC(NetworkManager.Singleton.LocalClientId);
                    }
                }

                if (!tutorialTextPanel.IsComplete(1) && attackToNiaSpecialAttack)
                {
                    tutorialTextPanel.Complete(1);

                    if (tutorialTextPanel.IsComplete(0))
                    {
                        OtherPlayerCompleteServerRPC(NetworkManager.Singleton.LocalClientId);
                    }
                }

                if (!tutorialTextPanel.IsComplete(2) && isOtherPlayerComplete)
                {
                    tutorialTextPanel.Complete(2);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken.Token, true);
            }
            Launcher.IsAttackSpecialLauncherEvent -= AttackSpecial;
            player.SpecialShotCooldownEvent -= CheckNiaSpecialAttack;
        }
        
        private void AttackSpecial()
        {
            attackToNiaSpecialAttack = true;
        }

        private void CheckNiaSpecialAttack(float gauge)
        {
            isSpecialShot = gauge == 0;
        }
        
        private void CheckRiaSpecialAttack(int chargeStep)
        {
            isSpecialShot = chargeStep >= 1;
        }

        #endregion

        #region CounterTutorial

        private async UniTask CounterTutorial()
        {
            await TextShow(ChooseText(tutorialFlowData.CounterTutorial));
            tutorialTextPanel.SetText(ChooseExplanationText(tutorialFlowData.CounterTutorial), ChooseConditionText(tutorialFlowData.CounterTutorial));
            await UniTask.WaitUntil(() =>  !tutorialTextPanel.IsToggleOn, PlayerLoopTiming.Update, cancellationToken.Token);
            dummyEffect.Play();
            counterMonsterDummy.gameObject.SetActive(true);
            counterMonsterDummy.CounterSuccessEvent += SuccessCounter;
            counterMonsterDummy.StartCounter();
            while (!tutorialTextPanel.IsCompleteAll)
            {
                if (!tutorialTextPanel.IsComplete(0) && isOtherPlayerComplete)
                {
                    tutorialTextPanel.Complete(0);
                }
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken.Token, true);
            }
            counterMonsterDummy.CounterSuccessEvent -= SuccessCounter;
        }
        
        private void SuccessCounter()
        {
            OtherPlayerCompleteServerRPC(riaClientId);
            OtherPlayerCompleteServerRPC(niaClientId);
        }

        #endregion

        #region ShieldTutorial

        private bool isShieldActivate = false;
        private async UniTask ShieldTutorial()
        {
            await TextShow(ChooseText(tutorialFlowData.ShieldTutorial));
            tutorialTextPanel.SetText(ChooseExplanationText(tutorialFlowData.ShieldTutorial), ChooseConditionText(tutorialFlowData.ShieldTutorial));
            await UniTask.WaitUntil(() =>  !tutorialTextPanel.IsToggleOn, PlayerLoopTiming.Update, cancellationToken.Token);
            localPlayer.ShieldCooldownEvent += CheckActiveShield;
            while (!tutorialTextPanel.IsCompleteAll)
            {
                if (!tutorialTextPanel.IsComplete(0) && isShieldActivate)
                {
                    tutorialTextPanel.Complete(0);
                    OtherPlayerCompleteServerRPC(NetworkManager.Singleton.LocalClientId);
                }
                
                if (!tutorialTextPanel.IsComplete(1) && isOtherPlayerComplete)
                {
                    tutorialTextPanel.Complete(1);
                }
                
                if (IsServer)
                {
                    localPlayer.ReduceShieldCooldownClientRPC(1);
                    otherPlayer.ReduceShieldCooldownClientRPC(1);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken.Token, true);
            }
            localPlayer.ShieldCooldownEvent -= CheckActiveShield;
        }

        private void CheckActiveShield(float shieldCooldown)
        {
            isShieldActivate = shieldCooldown > 0;
        }

        #endregion
        
        #region ReviveTutorial
        
        private bool isRevive = false;
        private async UniTask ReviveTutorial()
        {
            await TextShow(ChooseText(tutorialFlowData.ReviveTutorial));
            tutorialTextPanel.SetText(ChooseExplanationText(tutorialFlowData.ReviveTutorial), ChooseConditionText(tutorialFlowData.ReviveTutorial));
            await UniTask.WaitUntil(() =>  !tutorialTextPanel.IsToggleOn, PlayerLoopTiming.Update, cancellationToken.Token);
            playerReviveDummy.ReviveEvent += Revive;
            dummyEffect.Play();
            playerReviveDummy.gameObject.SetActive(true);
            if (playerType == PlayerType.LASER)
            {
                while (!tutorialTextPanel.IsCompleteAll)
                {
                    if (!tutorialTextPanel.IsComplete(0) && isRevive)
                    {
                        tutorialTextPanel.Complete(0);
                        OtherPlayerCompleteServerRPC(NetworkManager.Singleton.LocalClientId);
                    }

                    if (!tutorialTextPanel.IsComplete(1) && isOtherPlayerComplete)
                    {
                        tutorialTextPanel.Complete(1);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken.Token, true);
                }
            }
            else
            {
                while (!tutorialTextPanel.IsCompleteAll)
                {
                    if (!tutorialTextPanel.IsComplete(0) && isOtherPlayerComplete)
                    {
                        tutorialTextPanel.Complete(0);
                    }

                    if (!tutorialTextPanel.IsComplete(1) && isRevive)
                    {
                        tutorialTextPanel.Complete(1);
                        OtherPlayerCompleteServerRPC(NetworkManager.Singleton.LocalClientId);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken.Token, true);
                }
            }

            playerReviveDummy.ReviveEvent -= Revive;
        }
        
        private void Revive()
        {
            isRevive = true;
        }
        #endregion

        #endregion
        
        private void SkipTutorial(ulong playerId)
        {
            isSkipLocalCheck = !isSkipLocalCheck;
            SkipTutorialServerRPC(playerId, isSkipLocalCheck);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SkipTutorialServerRPC(ulong playerId, bool isSkip)
        {
            SkipTutorialClientRPC(playerId == NetworkManager.ServerClientId, isSkip);
        }
        
        [ClientRpc]
        private void SkipTutorialClientRPC(bool isServer, bool isSkip)
        {
            if (isServer)
            {
                skipPlayerToggle[0].isOn = isSkip;
            }
            else
            {
                skipPlayerToggle[1].isOn = isSkip;
            }

            if (skipPlayerToggle[0].isOn && skipPlayerToggle[1].isOn)
            {
                EndTutorial(IsServer);
            }
        }

        private void EndTutorial(bool isServer)
        {
            SoundManager.Instance.StopCharacterSelectionBGM();
            loadingPanel.gameObject.SetActive(true);
            CutScene.Instance.PlayCutScene();
            cancellationToken.Cancel();
            if (isServer)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
