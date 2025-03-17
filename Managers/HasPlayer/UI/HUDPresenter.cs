using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class HUDPresenter : Singleton<HUDPresenter>
    {
        public static readonly Vector2 CANVAS_RESOLUTION_DEFAULT = new Vector2(1920, 1080);
        public static Vector2 GET_CANVAS_RESOLUTION_ASPECT_RATIO => new Vector2(Screen.width / CANVAS_RESOLUTION_DEFAULT.x, Screen.height / CANVAS_RESOLUTION_DEFAULT.y);
        
        public bool IsDebugMode { get; set; } = false;
        public bool IsReady { get; private set; } = false;
        
        private Canvas hudCanvas;

        private UIElement<LaserUI> laserUI;
        private UIElement<LauncherUI> launcherUI;
        
        private UIElement<NotificationPositionUI> notificationPositionPlayerUI;
        
        private UIElement<Image> riaIcon;
        private UIElement<Image> niaIcon;
        
        private UIElement<HPBarUI> playerHP;
        
        private UIElement<SelfReviveUI> selfReviveUI;
        private UIElement<ReviveUI> reviveUI;
        
        private UIElement<TimerUI> timerUI;

        private UIElement<ResultPanelUI> resultUI;
        
        protected override void OnAwake()
        {
            hudCanvas = GetComponent<Canvas>();

            hudCanvas.worldCamera = GameObject.Find("UI Camera").GetComponent<Camera>();
            laserUI = new UIElement<LaserUI>("LaserGun", hudCanvas.gameObject);
            launcherUI = new UIElement<LauncherUI>("Launcher", hudCanvas.gameObject);
            notificationPositionPlayerUI = new UIElement<NotificationPositionUI>("NotificationPos", hudCanvas.gameObject);
            riaIcon = new UIElement<Image>("IconRia", hudCanvas.gameObject);
            niaIcon = new UIElement<Image>("IconNia", hudCanvas.gameObject);
            playerHP = new UIElement<HPBarUI>("PlayerHP", hudCanvas.gameObject);
            selfReviveUI = new UIElement<SelfReviveUI>("SelfReviveUI", hudCanvas.gameObject);
            reviveUI = new UIElement<ReviveUI>("ReviveUI", hudCanvas.gameObject);
            timerUI = new UIElement<TimerUI>("PlayTime", hudCanvas.gameObject);
            resultUI = new UIElement<ResultPanelUI>("ResultPanel", hudCanvas.gameObject);
            
            laserUI.Component.gameObject.SetActive(false);
            launcherUI.Component.gameObject.SetActive(false);
            riaIcon.Component.gameObject.SetActive(false);
            niaIcon.Component.gameObject.SetActive(false);

            PlayManager.PlayerTypeReadyEvent += InitUI;
            PlayManager.PlayerTypeReadyEvent += SetUI;
            PlayManager.PlayerTypeReadyEvent += SetNotificationUI;

            if (IsDebugMode)
            {
                notificationPositionPlayerUI.Component.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (!ReferenceEquals(InGamePlayManager.Instance, null))
            {
                InGamePlayManager.Instance.PlayTimeEvent += timerUI.Component.SetTimerUI;
                InGamePlayManager.Instance.GameEndEvent += resultUI.Component.ShowResultPanel;
            }

            IsReady = true;
        }

        public void InitUI(ulong playerId, PlayerType playerType)
        {
            PlayerBase player = PlayManager.Instance.GetPlayer(playerId);
            
            InitUI(player);
        }
        
        public void InitUI(PlayerBase player)
        {
            PlayManager.PlayerTypeReadyEvent -= InitUI;
            player.ShieldCooldownEvent += playerHP.Component.SetShield;
            player.PlayerHPEvent += InitMainHPSlider;
            player.ReviveEvent += reviveUI.Component.SetReviveGaugeServerRPC;
            player.PlayerHealEvent += playerHP.Component.Heal;
            
            if (player.PlayerType == PlayerType.LASER)
            {
                PlayerOne playerOne = (PlayerOne)player;
                playerOne.ShotEvent += laserUI.Component.Shot;
                playerOne.ChargeEvent += laserUI.Component.Charge;
                playerOne.ChargeCooldownEvent += laserUI.Component.ChargeCooldown;
                playerOne.ReloadEvent += laserUI.Component.Reload;
            }
            else
            {
                PlayerTwo playerTwo = (PlayerTwo)player;
                playerTwo.AmmoInitEvent += launcherUI.Component.Init;
                playerTwo.ShotEvent += launcherUI.Component.Shot;
                playerTwo.SpecialShotCooldownEvent += launcherUI.Component.SpecialShotCooldown;
                playerTwo.ReloadEvent += launcherUI.Component.Reload;
            }
        }

        public void SetUI(ulong playerId, PlayerType playerType)
        {
            PlayManager.PlayerTypeReadyEvent -= SetUI;
            if (playerType == PlayerType.LASER)
            {
                niaIcon.Component.gameObject.SetActive(false);
                launcherUI.Component.gameObject.SetActive(false);
                riaIcon.Component.gameObject.SetActive(true);
                laserUI.Component.gameObject.SetActive(true);
            }
            else
            {
                niaIcon.Component.gameObject.SetActive(true);
                launcherUI.Component.gameObject.SetActive(true);
                riaIcon.Component.gameObject.SetActive(false);
                laserUI.Component.gameObject.SetActive(false);
            }
        }

        private void SetNotificationUI(ulong playerId, PlayerType playerType)
        {
            PlayManager.PlayerTypeReadyEvent -= SetNotificationUI;
            PlayManager.Instance.GetAnotherPlayer(playerId).PlayerPositionEvent += notificationPositionPlayerUI.Component.CheckUIActive;
            PlayManager.Instance.GetPlayer(playerId).SelfReviveEvent += selfReviveUI.Component.SetReviveGauge;
        }

        public void InitMainHPSlider(int currentHP, int maxHP)
        {
            playerHP.Component.InitHP(currentHP, maxHP);
            PlayManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId).PlayerHPEvent -= InitMainHPSlider;
            PlayManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId).PlayerHPEvent += SetMainHPSlider;
        }
        
        public void SetMainHPSlider(int currentHP, int maxHP)
        {
            playerHP.Component.SetHP(currentHP, maxHP);
        }
    }
}
