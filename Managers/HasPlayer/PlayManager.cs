using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    public class PlayManager : NetworkSingleton<PlayManager>
    {
        protected List<ulong> playerIds = new List<ulong>();
        protected List<PlayerBase> playerBases = new List<PlayerBase>();
        protected Dictionary<ulong, PlayerBase> playerDict = new Dictionary<ulong, PlayerBase>();
        public PlayerBase[] GetAllPlayer() => playerBases.ToArray();
        public PlayerBase GetPlayer(ulong id) => playerDict[id];
        public GameObject GetPlayerObject(ulong id) => playerDict[id].gameObject;
        public PlayerBase GetAnotherPlayer(ulong id) => playerDict[playerIds.Find(x => x != id)];
        public virtual bool IsAllPlayersReady => clientPlayersReady.Value == 2 || IsDebugMode;

        public PlayerType CurPlayerType { get; private set;}
        public bool IsDebugMode { get; set; } = false;
        
        public static event Action<ulong, PlayerType> PlayerTypeReadyEvent;
        public event Action<float, float> PlayTimeEvent;
        public event Action<PlayData> GameEndEvent;
        
        protected CinemachineCamera cinemachine;
        protected PlayerSpawner playerSpawner;
        
        protected int currentPlayerNum = 0;
        
        protected NetworkVariable<int> clientPlayersReady = new NetworkVariable<int>(0);
        
        protected override void OnAwake()
        {
            playerSpawner = FindFirstObjectByType<PlayerSpawner>();
            cinemachine = Camera.main.transform.parent.GetComponent<CinemachineCamera>();
        }
        
        public override void OnDestroy()
        {
            PlayerBase.GetPlayerAction -= FindPlayer;
            PlayerTypeReadyEvent = null;
            base.OnDestroy();
        }
        
        protected virtual void Start()
        {
            clientPlayersReady.OnValueChanged += (oldValue, newValue) =>
            {
                if (newValue == 2)
                {
                    if (IsServer)
                    {
                        TimeManager.Instance.TimeScale = TimeManager.Instance.OriginTimeScale;
                        TimeManager.Instance.PlayerTimeScale = 1;
                    }
                    Time.timeScale = TimeManager.Instance.OriginTimeScale;
                }
            };
        }

        protected virtual void Update()
        {
            if (currentPlayerNum < 2 && playerSpawner.transform.childCount == 2 && HUDPresenter.Instance.IsReady)
            {
                SetPlayer();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        protected void ClientIsReadyServerRPC()
        {
            clientPlayersReady.Value++;
        }
        
        public void SetPlayer()
        {
            if (playerIds.Count > 0)
                return;
            
            PlayerBase player1 = playerSpawner.transform.GetChild(0).GetComponent<PlayerBase>();
            playerIds.Add(player1.OwnerClientId);
            playerBases.Add(player1);
            playerDict.Add(player1.OwnerClientId, player1);
            currentPlayerNum++;
            
            PlayerBase player2 = playerSpawner.transform.GetChild(1).GetComponent<PlayerBase>();
            playerIds.Add(player2.OwnerClientId);
            playerBases.Add(player2);
            playerDict.Add(player2.OwnerClientId, player2);
            currentPlayerNum++;
            
            PlayerBase.GetPlayerAction += FindPlayer;

            CurPlayerType = GetPlayer(NetworkManager.Singleton.LocalClientId).PlayerType;
            
            CameraFollowObject cameraFollowObject =
                GetPlayer(NetworkManager.Singleton.LocalClientId).GetComponentInChildren<CameraFollowObject>();
            cameraFollowObject.SetCinemachine();
            cinemachine.Follow = cameraFollowObject.transform;
            
            ClientIsReadyServerRPC();
            PlayerTypeReadyEvent?.Invoke(NetworkManager.Singleton.LocalClientId, CurPlayerType);
            player1.Set();
            player2.Set();
        }

        public void SetPlayerInDebugMode(PlayerBase player)
        {
            playerDict.Clear();
            CurPlayerType = player.PlayerType;
            playerDict.Add(NetworkManager.Singleton.LocalClientId, player);
            
            PlayerTypeReadyEvent?.Invoke(NetworkManager.Singleton.LocalClientId, CurPlayerType);
        }

        private GameObject FindPlayer(ulong id, bool isFindThisPlayer)
        {
            return isFindThisPlayer ? GetPlayerObject(id) : GetAnotherPlayer(id).gameObject;
        }

        protected void InvokePlayTimeEvent(float gameEndPlayTime, float playTime)
        {
            PlayTimeEvent?.Invoke(gameEndPlayTime, playTime);
        }
        
        protected void InvokeGameEndEvent(PlayData result)
        {
            GameEndEvent?.Invoke(result);
        }
    }
}
