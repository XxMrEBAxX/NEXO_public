using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine;

namespace BirdCase
{
    public class TestManager : MonoBehaviour
    {
        private PlayerSpawner playerSpawner;

        private GameObject spawnedPlayer;
        
        private GameObject player01Prefab;
        private GameObject player02Prefab;
        private CinemachineCamera cinemachine;
        
        private bool isPlayerSpawn = false;
        
        private void Awake()
        {
            cinemachine = Camera.main.transform.parent.GetComponent<CinemachineCamera>();
            playerSpawner = FindObjectOfType<PlayerSpawner>();
            player01Prefab = playerSpawner.RiaPrefab;
            player02Prefab = playerSpawner.NiaPrefab;
        }

        private void Start()
        {
            if (!ReferenceEquals(FindAnyObjectByType<GameManager>(), null))
            {
                Destroy(this);
                return;
            }

            if(!ReferenceEquals(PlayManager.Instance, null))
                PlayManager.Instance.IsDebugMode = true;
            HUDPresenter.Instance.IsDebugMode = true;

            NetworkManager.Singleton.StartHost();
        }

        private void Update()
        {
            if (Keyboard.current[Key.Digit1].wasPressedThisFrame)
            {
                SpawnPlayer(player01Prefab);
            }
            else if (Keyboard.current[Key.Digit2].wasPressedThisFrame)
            {
                SpawnPlayer(player02Prefab);
            }
        }

        private void SpawnPlayer(GameObject playerPrefab)
        {
            if (isPlayerSpawn)
            {
                playerSpawner.DespawnPlayerLocaly(spawnedPlayer);
                PlayerBase.GetPlayerAction -= (id, isSamePlayer) => isSamePlayer ? spawnedPlayer : null;
            }
            isPlayerSpawn = true;

            spawnedPlayer = playerSpawner.SpawnPlayerLocaly(playerPrefab);
            PlayerBase playerBase = spawnedPlayer.GetComponent<PlayerBase>();
            PlayManager.Instance.SetPlayerInDebugMode(playerBase);
            PlayerBase.GetPlayerAction += (id, isSamePlayer) => isSamePlayer ? spawnedPlayer : null;
            cinemachine.Follow = spawnedPlayer.transform.GetComponentInChildren<CameraFollowObject>().transform; 
            spawnedPlayer.transform.GetComponentInChildren<CameraFollowObject>().SetCinemachine();
            playerBase.Set();
        }
    }
}
