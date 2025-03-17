using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    public class InGamePlayManager : PlayManager
    {
        // 무조건 컷씬, 연출 끝나고 본 게임 시작을 할 수 있나 체크
        public override bool IsAllPlayersReady => base.IsAllPlayersReady && IsCutSceneEndBoth && IsBossAppear;
        public static event Action CutSceneEndEvent;
        
        [SerializeField, Tooltip("게임 종료 시간, 단위는 초입니다"), Min(0)]
        private float gameEndPlayTime = 1200f;
        public float GameEndPlayTime => gameEndPlayTime;

        private bool isGameStart = false;
        public bool IsGameEnd { get; private set; } = false;

        private NetworkVariable<float> playTime = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<int> cutSceneEndPlayer = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public static bool IsCutSceneEnd;
        
        // 컷씬을 다 봤는지 체크 (연출 시작용)
        public bool IsCutSceneEndBoth => cutSceneEndPlayer.Value >= 2;
        public float PlayTime => playTime.Value;
        public bool ActiveTimer { get; set; } = false;
        public bool IsBossAppear { get; set; } = false;

        protected override void Start()
        {
            base.Start();
            playerSpawner.ServerSceneInit(GameManager.Instance.RiaClientId.Value, GameManager.Instance.NiaClientId.Value, false);
            DataSaveManager.Instance.OnDataSaved += () => InvokeGameEndEvent(DataSaveManager.Instance.CurPlayData);
            
            cutSceneEndPlayer.OnValueChanged += (oldValue, newValue) =>
            {
                if (newValue >= 2)
                {
                    CutSceneEndEvent?.Invoke();
                    playerBases.ForEach(x => x.PlayBossAppear(true));
                    SoundManager.Instance.PlayPhase1BGM();
                }
            };
        }

        bool isFirstDebug = false;
        protected override void Update()
        {
            base.Update();
            CheckPlayTime();

            if (IsDebugMode && !isFirstDebug)
            {
                isFirstDebug = true;
                DataSaveManager.Instance.IsGameStart = true;
                cutSceneEndPlayer.Value = 3;
                SoundManager.Instance.PlayPhase1BGM();
                return;
            }

            if (IsCutSceneEnd)
            {
                IsCutSceneEnd = false;
                if (IsServer)
                {
                    cutSceneEndPlayer.Value += 1;
                }
                else
                {
                    CutSceneEndServerRpc();
                }
            }

            if (!isGameStart && IsAllPlayersReady && !IsDebugMode)
            {
                isGameStart = true;
                playerBases.ForEach(x => x.PlayBossAppear(false));
                playerBases.ForEach(x => x.PlayerControlAllClientRPC(true));
                DataSaveManager.Instance.IsGameStart = true;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            IsCutSceneEnd = false;
            CutSceneEndEvent = null;
        }

        [ServerRpc(RequireOwnership = false)]
        public void CutSceneEndServerRpc()
        {
            cutSceneEndPlayer.Value += 1;
        }

        private void CheckPlayTime()
        {
            if (IsGameEnd)
                return;

            if (IsAllPlayersReady && IsServer && ActiveTimer)
            {
                playTime.Value += Time.unscaledDeltaTime;
            }


            InvokePlayTimeEvent(gameEndPlayTime, playTime.Value);
        }

        public void BossDefeat()
        {
            playerBases.ForEach(x => x.PlayBossDefeat());
            playerBases.ForEach(x => x.PlayerControlAllClientRPC(false));
            PlayerInputController.CanInput = false;
        }

        public void GameEnd()
        {
            IsGameEnd = true;
            if (IsServer)
            {
                TimeManager.Instance.TimeScale = 1;
            }

            DataSaveManager.Instance.CurPlayData.GameTime = playTime.Value;

            if (!IsDebugMode)
            {
                if (!NetworkManager.Singleton.IsServer)
                    DataSaveManager.Instance.SetPlayDataServerRpc(DataSaveManager.Instance.CurPlayData);

                GameManager.Instance.ChangeStateToResult();
            }
            else
            {
                InvokeGameEndEvent(DataSaveManager.Instance.CurPlayData);
            }
        }
    }
}
