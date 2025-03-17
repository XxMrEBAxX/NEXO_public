using System;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using NetworkEvent = Unity.Netcode.NetworkEvent;
using Cysharp.Threading.Tasks;
using System.Threading;
using Mono.CSharp;
using Unity.Netcode;

namespace BirdCase
{
    [RequireComponent(typeof(IpManager), typeof(RelayManager))]
    public class ConnectionManager : Singleton<ConnectionManager>
    {
        private const float TIME_OUT = 3f; // 3 seconds
        
        private static Dictionary<ulong, ClientRpcParams> clientRpcParamsDictionary = new Dictionary<ulong, ClientRpcParams>();
        public event Action<NetworkEvent> OnNetworkEvent;
        public NetworkEvent CurrentNetworkEvent { get; private set; } = NetworkEvent.Nothing;

        private UnityTransport transport;
        private bool isAwaitCanceled = false;
        
        protected override void OnAwake()
        {
        }

        private void Start()
        {
            transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.OnTransportEvent += OnTransportEvent;
            transport.Initialize();
        }

        private void OnDestroy()
        {
            transport.OnTransportEvent -= OnTransportEvent;
        }

        private void OnTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
        {
            if (eventType == NetworkEvent.Connect)
            {
                CurrentNetworkEvent = NetworkEvent.Connect;
            }
            else if (eventType == NetworkEvent.Disconnect)
            {
                CurrentNetworkEvent = NetworkEvent.Disconnect;
            }
        }

        public void SetConnectionData()
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(Application.version);
        }

        public void CancelNetworking()
        {
            CurrentNetworkEvent = NetworkEvent.Nothing;
            RelayManager.Instance.StopAllocating();
            
            NetworkManager networkManager = NetworkManager.Singleton;
            networkManager.Shutdown(true);
        }
        
        public void CancelWaiting()
        {
            isAwaitCanceled = true;
        }
        
        public void WaitForConnect()
        {
            WaitForAwaitTime().Forget();
        }
        
        private async UniTaskVoid WaitForAwaitTime()
        {
            isAwaitCanceled = false;
            OnNetworkEvent?.Invoke(CurrentNetworkEvent);
            float elapsedTime = 0;
            while (true)
            {
                elapsedTime += Time.unscaledDeltaTime;
                
                if ((CurrentNetworkEvent == NetworkEvent.Connect && NetworkManager.Singleton.IsApproved) ||
                    GameManager.Instance.CurGameState != GameManager.GameState.TITLE)
                {
                    return;
                }

                if (elapsedTime >= TIME_OUT || isAwaitCanceled || CurrentNetworkEvent == NetworkEvent.Disconnect)
                {
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken:this.GetCancellationTokenOnDestroy(), true);
            }

            if (CurrentNetworkEvent != NetworkEvent.Connect)
            {
                CurrentNetworkEvent = NetworkEvent.Nothing;
                NetworkManager.Singleton.Shutdown();
            }

            if (!isAwaitCanceled)
            {
                OnNetworkEvent?.Invoke(NetworkEvent.TransportFailure);
            }
        }
        
    
        public static ClientRpcParams GetClientRpcParams(ulong clientId)
        {
            ClientRpcParams clientRpcParams;
            if (clientRpcParamsDictionary.TryGetValue(clientId, out clientRpcParams))
            {
                return clientRpcParams;
            }
            else
            {
                clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };
                clientRpcParamsDictionary.Add(clientId, clientRpcParams);
                return clientRpcParams;
            }
        }
    }
}
