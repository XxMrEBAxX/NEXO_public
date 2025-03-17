using System;
using Mono.CSharp;
using UnityEngine;
using Unity.Netcode;
using UnityEditor;

namespace BirdCase
{
    public class GameManager : NetworkSingleton<GameManager>
    {
        public enum GameState
        {
            TITLE,
            CHARACTER_SELECTION,
            GAME,
            RESULT,
        }

        private GameState gameState;
        public GameState CurGameState => gameState;
        private int connectedPlayers;

        [HideInInspector]
        public NetworkVariable<ulong> RiaClientId = new NetworkVariable<ulong>(ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [HideInInspector]
        public NetworkVariable<ulong> NiaClientId = new NetworkVariable<ulong>(ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        public event Action<GameState> OnGameStateChanged;

        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            gameState = GameState.TITLE;
            
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;
            
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (!ReferenceEquals(NetworkManager.Singleton, null))
            {
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
                NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
            }
        }

        private void OnServerStarted()
        { 
            if (!IsServer)
                return;

            connectedPlayers = 0;
            
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
        }

        private void OnServerStopped(bool isHost)
        {
            if (!isHost)
                return;
            
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
        }
        
        private void OnClientConnectedCallback(ulong clientId)
        {
            if (!IsServer)
            {
                return;
            }
            
            connectedPlayers = Mathf.Min(2, connectedPlayers + 1);
            
            if (connectedPlayers >= 2)
            {
                ChangeToCharacterSelection();
            }
        }
        
        private void OnClientDisconnectedCallback(ulong clientId)
        {
            if (!IsServer && !NetworkManager.Singleton.IsApproved)
            {
                return;
            }
            
            connectedPlayers = Mathf.Max(1, connectedPlayers - 1);
            if (connectedPlayers < 2 && gameState != GameState.TITLE && gameState != GameState.RESULT)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    ChangeGameStateClientRpc(GameState.TITLE);   
                }
                else
                {
                    gameState = GameState.TITLE;
                    OnGameStateChanged?.Invoke(GameState.TITLE);
                }
            }
        }
        
        private void ChangeToCharacterSelection()
        {
            ChangeGameStateClientRpc(GameState.CHARACTER_SELECTION);
        }

        public void SetCharacterId(ulong riaPlayer, ulong niaPlayer)
        {
            RiaClientId.Value = riaPlayer;
            NiaClientId.Value = niaPlayer;
        }
        
        public void StartGame()
        {
            ChangeGameStateClientRpc(GameState.GAME);
        }
        
        public void ChangeStateToResult()
        {
            gameState = GameState.RESULT;
        }
        
        public void GameEnd()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            OnGameStateChanged?.Invoke(gameState);
        }

        public void GoToTitle()
        {
            gameState = GameState.TITLE;
            OnGameStateChanged?.Invoke(gameState);
        }
        
        [ClientRpc]
        private void ChangeGameStateClientRpc(GameState state)
        {
            gameState = state;
            OnGameStateChanged?.Invoke(gameState);
        }
        
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (request.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
            {
                response.Approved = true;
                return;
            }

            var versionCheck = System.Text.Encoding.ASCII.GetString(request.Payload);
            if(versionCheck != Application.version)
            {
                response.Approved = false;
                response.Reason = "Version mismatch";
            }
            else
            {
                response.Approved = true;
                response.Reason = "";
            }
        }
    }
}
