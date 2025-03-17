using UnityEngine;
using LitJson;
using System.IO;
using System.Text;

namespace BirdCase
{
    public static class JsonManager
    {
        public const string SAVE_PATH = "/Data.json";
        
        public static void Save(PlayData toSaveData)
        {
            JsonMapper.RegisterImporter<int, float>((int value) => (float)value);

            string data = JsonMapper.ToJson(toSaveData);

            StringBuilder sb = new StringBuilder();
            sb.Append(Application.persistentDataPath);
            sb.Append(SAVE_PATH);

            File.WriteAllText(sb.ToString(), data);
        }

        public static PlayData Load()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Application.persistentDataPath);
            sb.Append(SAVE_PATH);

            if (File.Exists(sb.ToString()))
            {
                string json = File.ReadAllText(sb.ToString());
                JsonData data = JsonMapper.ToObject(json);

                PlayData playData = new PlayData();
                playData.GameTime = double.Parse(data["GameTime"].ToString());
                playData.RiaDefaultAttackCount = uint.Parse(data["RiaDefaultAttackCount"].ToString());
                playData.NiaDefaultAttackCount = uint.Parse(data["NiaDefaultAttackCount"].ToString());
                playData.RiaSpecialAttackCount = uint.Parse(data["RiaSpecialAttackCount"].ToString());
                playData.NiaSpecialAttackCount = uint.Parse(data["NiaSpecialAttackCount"].ToString());
                playData.RiaJumpCount = uint.Parse(data["RiaJumpCount"].ToString());
                playData.NiaJumpCount = uint.Parse(data["NiaJumpCount"].ToString());
                playData.RiaMoveTime = double.Parse(data["RiaMoveTime"].ToString());
                playData.NiaMoveTime = double.Parse(data["NiaMoveTime"].ToString());
                playData.RiaStopTime = double.Parse(data["RiaStopTime"].ToString());
                playData.NiaStopTime = double.Parse(data["NiaStopTime"].ToString());
                playData.BossDamagedByRia = int.Parse(data["BossDamagedByRia"].ToString());
                playData.BossDamagedByNia = int.Parse(data["BossDamagedByNia"].ToString());
                playData.BossNeutralizedByRia = int.Parse(data["BossNeutralizedByRia"].ToString());
                playData.BossNeutralizedByNia = int.Parse(data["BossNeutralizedByNia"].ToString());
                playData.IsBossKilled = bool.Parse(data["IsBossKilled"].ToString());
                playData.RiaDeadCount = uint.Parse(data["DeadRiaCount"].ToString());
                playData.NiaDeadCount = uint.Parse(data["DeadNiaCount"].ToString());
                playData.RiaReviveCountByNia = uint.Parse(data["RiaReviveCountByNia"].ToString());
                playData.NiaReviveCountByRia = uint.Parse(data["NiaReviveCountByRia"].ToString());
                playData.RiaReviveCountSelf = uint.Parse(data["RiaReviveCountSelf"].ToString());
                playData.NiaReviveCountSelf = uint.Parse(data["NiaReviveCountSelf"].ToString());
                playData.RiaShieldCount = uint.Parse(data["RiaShieldCount"].ToString());
                playData.NiaShieldCount = uint.Parse(data["NiaShieldCount"].ToString());
                playData.RiaShieldBrokenCount = uint.Parse(data["RiaShieldBrokenCount"].ToString());
                playData.NiaShieldBrokenCount = uint.Parse(data["NiaShieldBrokenCount"].ToString());
                playData.RiaStunTime = double.Parse(data["RiaStunTime"].ToString());
                playData.NiaStunTime = double.Parse(data["NiaStunTime"].ToString());
                
                return playData;
            }

            return default;
        }
    }
}
