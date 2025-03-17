using System;
using System.Text;
using UnityEngine;
using TMPro;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;

namespace BirdCase
{
    public class Title : Singleton<Title>
    {
        [Header("Local")]
        [SerializeField] private GameObject localPanel;
        [SerializeField] private TextMeshProUGUI localIpText;
        [SerializeField] private TextMeshProUGUI portText; 
        [SerializeField] private TMP_InputField ipInputField;
        [SerializeField] private TextMeshProUGUI hostingText;
        
        [Header("Host")]
        [SerializeField] private GameObject hostPanel;
        [SerializeField] private TextMeshProUGUI joinCodeText;
        
        [Header("Client")]
        [SerializeField] private GameObject clientPanel;
        [SerializeField] private TMP_InputField joinCodeInputField;
        
        [Header("Waiting")]
        [SerializeField] private GameObject waitingPanel;
        
        [Header("Setting")]
        [SerializeField] private LanguageSetting languageSetting;

        private StringBuilder stringBuilder;
        private bool isLocalPanelShow = false;
        
        protected override void OnAwake()
        {
        }

        private void Start()
        {
            RelayManager.Instance.OnRelayJoinCodeCreatedEvent += (string s) => joinCodeText.text = s;
            ConnectionManager.Instance.OnNetworkEvent += (NetworkEvent networkEvent) =>
            {
                switch (networkEvent)
                {
                    case NetworkEvent.Nothing:
                        ShowWaitingPanel();
                        break;
                    case NetworkEvent.TransportFailure:
                        if (isLocalPanelShow)
                        {
                            ShowLocalPanel();
                        }
                        else
                        {
                            ShowClientPanel();
                        }

                        break;
                }
            };

            GameManager.Instance.OnGameStateChanged += GameStateChangedCallback;
                        
            stringBuilder = new StringBuilder();
            SoundManager.Instance.PlayMainMenuBGM();
        }

        private static void GameStateChangedCallback(GameManager.GameState gameState)
        {
            if (gameState == GameManager.GameState.TITLE)
            {
                ConnectionManager.Instance.CancelNetworking();
                ChangeScene("Scenes/MenuScene/MenuScene", false);
                return;
            }
            
            if (gameState == GameManager.GameState.RESULT)
            {
                ConnectionManager.Instance.CancelNetworking();
                return;
            }

            if(!NetworkManager.Singleton.IsServer && ConnectionManager.Instance.CurrentNetworkEvent == NetworkEvent.Connect)
                return;
            
            switch (gameState)
            {
                // case GameManager.GameState.TITLE:
                //     ChangeScene("Scenes/MenuScene/MenuScene", ConnectionManager.Instance.CurrentNetworkEvent == NetworkEvent.Connect);
                //     break;
                case GameManager.GameState.CHARACTER_SELECTION:
                    ChangeScene("Scenes/CharacterSelection", ConnectionManager.Instance.CurrentNetworkEvent == NetworkEvent.Connect);
                    break;
                case GameManager.GameState.GAME:
                    ChangeScene("Scenes/PlayScene/PlayScene", ConnectionManager.Instance.CurrentNetworkEvent == NetworkEvent.Connect);
                    break;
                default:
                    break;
            }
        }

        private static void ChangeScene(string sceneName, bool isNetworkConnect)
        {
            SceneLoaderWrapper.Instance.LoadScene(sceneName, isNetworkConnect);
        }
        
        private void ShowLocalPanel()
        {
            isLocalPanelShow = true;
            localPanel.SetActive(true);
            hostPanel.SetActive(false);
            clientPanel.SetActive(false);
            waitingPanel.SetActive(false);
        }

        public void ShowHostPanel()
        {
            localPanel.SetActive(false);
            hostPanel.SetActive(true);
            clientPanel.SetActive(false);
            waitingPanel.SetActive(false);
        }
        
        public void ShowClientPanel()
        {
            isLocalPanelShow = false;
            localPanel.SetActive(false);
            hostPanel.SetActive(false);
            clientPanel.SetActive(true);
            waitingPanel.SetActive(false);
        }
        
        public void ShowWaitingPanel()
        {
            localPanel.SetActive(false);
            hostPanel.SetActive(false);
            clientPanel.SetActive(false);
            waitingPanel.SetActive(true);
        }
        
        public void LocalButtonCallback()
        {   
            ShowLocalPanel();
            string curLanguage = languageSetting.CurrentLanguage();
            
            stringBuilder.Append(curLanguage == "한국어" ? "내 아이피: " : "My IP: ");
            stringBuilder.Append(IpManager.Instance.GetIp());
            localIpText.text = stringBuilder.ToString();
            stringBuilder.Clear();
            
            // IpManager.Instance.SplitIpAndPort(ipInputField.text, out string ip, out ushort port);
            // hostingText.text = curLanguage == "한국어" ? $"포트: {port}" : $"Port: {port}";
        }

        public void IpHostButtonCallback()
        {
            IpManager.Instance.SplitIpAndPort(ipInputField.text, out string ip, out ushort port);
            IpManager.Instance.HostWithIp(port);
            //portText.text = "Port: " + port;
            portText.text = languageSetting.CurrentLanguage() == "한국어" ? $"포트: {port}" : $"Port: {port}";
        }

        public void IpJoinButtonCallback()
        {
            IpManager.Instance.SplitIpAndPort(ipInputField.text, out string ip, out ushort port);
            IpManager.Instance.JoinWithIp(ip, port);
        }

        public void HostButtonCallback()
        {
            RelayManager.Instance.StartCoroutine(
                RelayManager.Instance.ConfigureTransportAndStartNgoAsHost());
            
            ShowHostPanel();
        }

        public void ClientButtonCallback()
        {
            ShowClientPanel();
        }

        public void RelayJoinButtonCallback()
        {
            RelayManager.Instance.StartCoroutine(
                RelayManager.Instance.ConfigureTransportAndStartNgoAsConnectingPlayer(joinCodeInputField.text));
        }

        public void CopyToClipBoard()
        {
            GUIUtility.systemCopyBuffer = joinCodeText.text;
        }

        public void CopyToClipBoardIP()
        {
            GUIUtility.systemCopyBuffer = IpManager.Instance.GetIpAndPort();
        }

        public void PlaySelectSound()
        {
            SoundManager.Instance.PlayUISelectSound();
        }
    }
}
