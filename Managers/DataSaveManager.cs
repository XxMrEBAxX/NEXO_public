using Unity.Netcode;
using System;

namespace BirdCase
{
    public class DataSaveManager : NetworkSingleton<DataSaveManager>
    {
        public PlayData CurPlayData { get; set; }

        private bool isGameStart = false;
        public bool IsGameStart
        {
            get
            {
                return isGameStart;
            }
            set
            {
                if(value)
                    CurPlayData = new PlayData();
                isGameStart = value;
            }
        }
        
        public event Action OnDataSaved;

        protected override void OnAwake()
        {
            CurPlayData = new PlayData();
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            CurPlayData = null;
        }
        
        public void SaveData()
        {
            JsonManager.Save(CurPlayData);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetPlayDataServerRpc(PlayData playData)
        {
            CurPlayData.SetPlayData(playData);
            SaveData();
            OnDataSaved?.Invoke();
            SetPlayDataClientRpc(CurPlayData);
        }
        
        [ClientRpc]
        private void SetPlayDataClientRpc(PlayData playData)
        {
            if (NetworkManager.Singleton.IsServer)
                return;
            
            CurPlayData.SetPlayData(playData);
            SaveData();
            OnDataSaved?.Invoke();
        }
    }
}
