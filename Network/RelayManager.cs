using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Http;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;

namespace BirdCase
{
    public class RelayManager : Singleton<RelayManager>
    {
        private const int MAX_CONNECTIONS = 1;

        public string RelayJoinCode { get; private set; }
        
        public event Action<string> OnRelayJoinCodeCreatedEvent;

        private UnityTransport transport;

        private bool isAllocating;

        protected override void OnAwake()
        {   
        }

        private void Start()
        {
            transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            
            AuthenticatingPlayer();
        }
        
        private async void AuthenticatingPlayer()
        {
            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                //string playerID = AuthenticationService.Instance.PlayerId;
            }
            catch (AuthenticationException e)
            {
                Debug.LogException(e);
            }
            catch (RequestFailedException e)
            {
                Debug.LogException(e);
            }
        }

        public void StopAllocating()
        {
            isAllocating = false;
        }

        public async Task<RelayServerData> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
        {
            Allocation allocation;
            try
            {
                allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay create allocation request failed {e.Message}");
                throw;
            }
            
            Debug.Log($"server : {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
            Debug.Log($"server : {allocation.AllocationId}");

            try
            {
                RelayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            }
            catch
            {
                Debug.LogError("Relay create join code request failed");
                throw;
            }

            return new RelayServerData(allocation, "dtls");
        }

        public IEnumerator ConfigureTransportAndStartNgoAsHost()
        {
            isAllocating = true;
            Task<RelayServerData> serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(MAX_CONNECTIONS);
            while (!serverRelayUtilityTask.IsCompleted)
            {
                if (!isAllocating)
                {
                    yield break;
                }

                yield return null;
            }

            if (serverRelayUtilityTask.IsFaulted)
            {
                Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
                yield break;
            }

            RelayServerData relayServerData = serverRelayUtilityTask.Result;
            
            transport.SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            OnRelayJoinCodeCreatedEvent?.Invoke(RelayJoinCode);
            isAllocating = false;
            yield return null;
        }

        public async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
        {
            JoinAllocation allocation;
            try
            {
                allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            }
            catch
            {
                Debug.LogError("Relay create join code request failed");
                throw;
            }

            Debug.Log($"client : {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
            Debug.Log($"host : {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
            Debug.Log($"client : {allocation.AllocationId}");
            
            return new RelayServerData(allocation, "dtls");
        }

        public IEnumerator ConfigureTransportAndStartNgoAsConnectingPlayer(string joinCode)
        {
            Task<RelayServerData> clientRelayUtilityTask = JoinRelayServerFromJoinCode(joinCode);

            while (!clientRelayUtilityTask.IsCompleted)
            {
                yield return null;
            }

            if (clientRelayUtilityTask.IsFaulted)
            {
                Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception : " + clientRelayUtilityTask.Exception.Message);
                yield break;
            }

            RelayServerData relayServerData = clientRelayUtilityTask.Result;
            
            transport.SetRelayServerData(relayServerData);

            ConnectionManager.Instance.SetConnectionData();
            NetworkManager.Singleton.StartClient();

            ConnectionManager.Instance.WaitForConnect();
            
            yield return null;
        }
    }
}
