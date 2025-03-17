using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BirdCase
{
    public enum PlayerName
    {
        Ria,
        Nia
    }
    
    [Serializable]
    public class PlayData : INetworkSerializable, IEquatable<PlayData>
    {
        private const int MAX_SENTENCE = 2;
        
        // 게임 전반
        private double gameTime = 0;
        public double GameTime
        {
            set { gameTime = value; }
            get { return gameTime; }
        }
        
        private uint riaDefaultAttackCount = 0;
        public uint RiaDefaultAttackCount
        {
            set { riaDefaultAttackCount = value; }
            get { return riaDefaultAttackCount; }
        }

        private uint niaDefaultAttackCount = 0;
        public uint NiaDefaultAttackCount
        {
            set { niaDefaultAttackCount = value; }
            get { return niaDefaultAttackCount; }
        }

        private uint riaSpecialAttackCount = 0;
        public uint RiaSpecialAttackCount
        {
            set { riaSpecialAttackCount = value; }
            get { return riaSpecialAttackCount; }
        }

        private uint niaSpecialAttackCount = 0;
        public uint NiaSpecialAttackCount
        {
            set { niaSpecialAttackCount = value; }
            get { return niaSpecialAttackCount; }
        }

        private uint riaJumpCount = 0;
        public uint RiaJumpCount
        {
            set { riaJumpCount = value; }
            get { return riaJumpCount; }
        }

        private uint niaJumpCount = 0;
        public uint NiaJumpCount
        {  
            set { niaJumpCount = value; }
            get { return niaJumpCount; }
        }

        private double riaMoveTime = 0;
        public double RiaMoveTime
        {
            set { riaMoveTime = value; }
            get { return riaMoveTime; }
        }

        private double niaMoveTime = 0;
        public double NiaMoveTime
        {
            set { niaMoveTime = value; }
            get { return niaMoveTime; }
        }

        private double riaStopTime = 0;
        public double RiaStopTime
        {
            set { riaStopTime = value; }
            get { return riaStopTime; }
        }

        private double niaStopTime = 0;
        public double NiaStopTime
        {
            set { niaStopTime = value; }
            get { return niaStopTime; }
        }

        private int bossDamagedByRia = 0;
        public int BossDamagedByRia
        {
            set { bossDamagedByRia = value; }
            get { return bossDamagedByRia; }
        }

        private int bossDamagedByNia = 0;
        public int BossDamagedByNia
        {
            set { bossDamagedByNia = value; }
            get { return bossDamagedByNia; }
        }

        private int bossNeutralizedByRia = 0;
        public int BossNeutralizedByRia
        {
            set { bossNeutralizedByRia = value; }
            get { return bossNeutralizedByRia; }
        }

        private int bossNeutralizedByNia = 0;
        public int BossNeutralizedByNia
        {
            set { bossNeutralizedByNia = value; }
            get { return bossNeutralizedByNia; }
        }

        private bool isBossKilled = false;
        public bool IsBossKilled // 처리해야함
        {
            set { isBossKilled = value; }
            get { return isBossKilled; }
        }

        private uint riaDeadCount = 0;
        public uint RiaDeadCount
        {
            set { riaDeadCount = value; }
            get { return riaDeadCount; }
        }

        private uint niaDeadCount = 0;
        public uint NiaDeadCount
        {
            set { niaDeadCount = value; }
            get { return niaDeadCount; }
        }

        private uint riaReviveCountByNia = 0;
        public uint RiaReviveCountByNia
        {
            set { riaReviveCountByNia = value; }
            get { return riaReviveCountByNia; }
        }

        private uint niaReviveCountByRia = 0;
        public uint NiaReviveCountByRia
        {
            set { niaReviveCountByRia = value; }
            get { return niaReviveCountByRia; }
        }

        private uint riaReviveCountSelf = 0;
        public uint RiaReviveCountSelf
        {
            set { riaReviveCountSelf = value; }
            get { return riaReviveCountSelf; }
        }

        private uint niaReviveCountSelf = 0;
        public uint NiaReviveCountSelf
        {
            set { niaReviveCountSelf = value; }
            get { return niaReviveCountSelf; }
        }

        private uint riaShieldCount = 0;
        public uint RiaShieldCount
        {
            set { riaShieldCount = value; }
            get { return riaShieldCount; }
        }

        private uint niaShieldCount = 0;
        public uint NiaShieldCount
        {
            set { niaShieldCount = value; }
            get { return niaShieldCount; }
        }

        private uint riaShieldBrokenCount = 0;
        public uint RiaShieldBrokenCount
        {
            set { riaShieldBrokenCount = value; }
            get { return riaShieldBrokenCount; }
        }

        private uint niaShieldBrokenCount = 0;
        public uint NiaShieldBrokenCount
        {
            set { niaShieldBrokenCount = value; }
            get { return niaShieldBrokenCount; }
        }

        private double riaStunTime = 0;
        public double RiaStunTime
        {
            set { riaStunTime = value; }
            get { return riaStunTime; }
        }

        private double niaStunTime = 0;
        public double NiaStunTime
        {
            set { niaStunTime = value; }
            get { return niaStunTime; }
        }

        // 랜덤하게 띄우기
        private List<string> randomData = new List<string>();
        private static readonly string[] RANDOM_DATA =
        {
            "보스가 해당 게임을 시작하기 위해 준비한 시간 : 3달",
            "니아가 우느라 리아가 달래준 시간 : 6시간",
            "리아가 뛰쳐나가서 니아가 잡으러 간 시간 : 30분",
            "리아랑 니아가 상사에게 혼난 시간 : 일주일",
            "보스 머리가 굴러가서 잡으러 간 시간 : 3분",
            "빌드 테스트하는데 컴퓨터가 터진 날짜 : 10.22",
        };
        
        // 업적
        
        
        // 출력
        private string GetAchievementData()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("달성한 업적 ");
            
            return sb.ToString();
        }
        
        public void SetPlayData(PlayData playData)
        {
            gameTime = gameTime == 0 ? playData.gameTime : gameTime;
            riaDefaultAttackCount = riaDefaultAttackCount == 0 ? playData.riaDefaultAttackCount : riaDefaultAttackCount;
            niaDefaultAttackCount = niaDefaultAttackCount == 0 ? playData.niaDefaultAttackCount : niaDefaultAttackCount;
            riaSpecialAttackCount = riaSpecialAttackCount == 0 ? playData.riaSpecialAttackCount : riaSpecialAttackCount;
            niaSpecialAttackCount = niaSpecialAttackCount == 0 ? playData.niaSpecialAttackCount : niaSpecialAttackCount;
            riaJumpCount = riaJumpCount == 0 ? playData.riaJumpCount : riaJumpCount;
            niaJumpCount = niaJumpCount == 0 ? playData.niaJumpCount : niaJumpCount;
            riaMoveTime = riaMoveTime == 0 ? playData.riaMoveTime : riaMoveTime;
            niaMoveTime = niaMoveTime == 0 ? playData.niaMoveTime : niaMoveTime;
            riaStopTime = riaStopTime == 0 ? playData.riaStopTime : riaStopTime;
            niaStopTime = niaStopTime == 0 ? playData.niaStopTime : niaStopTime;
            bossDamagedByRia = bossDamagedByRia == 0 ? playData.bossDamagedByRia : bossDamagedByRia;
            bossDamagedByNia = bossDamagedByNia == 0 ? playData.bossDamagedByNia : bossDamagedByNia;
            bossNeutralizedByRia = bossNeutralizedByRia == 0 ? playData.bossNeutralizedByRia : bossNeutralizedByRia;
            bossNeutralizedByNia = bossNeutralizedByNia == 0 ? playData.bossNeutralizedByNia : bossNeutralizedByNia;
            isBossKilled = isBossKilled || playData.isBossKilled;
            riaDeadCount = riaDeadCount == 0 ? playData.riaDeadCount : riaDeadCount;
            niaDeadCount = niaDeadCount == 0 ? playData.niaDeadCount : niaDeadCount;
            riaReviveCountByNia = riaReviveCountByNia == 0 ? playData.riaReviveCountByNia : riaReviveCountByNia;
            niaReviveCountByRia = niaReviveCountByRia == 0 ? playData.niaReviveCountByRia : niaReviveCountByRia;
            riaReviveCountSelf = riaReviveCountSelf == 0 ? playData.riaReviveCountSelf : riaReviveCountSelf;
            niaReviveCountSelf = niaReviveCountSelf == 0 ? playData.niaReviveCountSelf : niaReviveCountSelf;
            riaShieldCount = riaShieldCount == 0 ? playData.riaShieldCount : riaShieldCount;
            niaShieldCount = niaShieldCount == 0 ? playData.niaShieldCount : niaShieldCount;
            riaShieldBrokenCount = riaShieldBrokenCount == 0 ? playData.riaShieldBrokenCount : riaShieldBrokenCount;
            niaShieldBrokenCount = niaShieldBrokenCount == 0 ? playData.niaShieldBrokenCount : niaShieldBrokenCount;
            riaStunTime = riaStunTime == 0 ? playData.riaStunTime : riaStunTime;
            niaStunTime = niaStunTime == 0 ? playData.niaStunTime : niaStunTime;
        }
        
        public string GetGameData()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("게임 시간 : ");
            sb.Append(string.Format("{0:00}:{1:00}", (int)(gameTime / 60), (int)(gameTime % 60)));
            sb.Append("\n");
            sb.Append("리아 기본 공격 횟수 : ");
            sb.Append(RiaDefaultAttackCount);
            sb.Append("\n");
            sb.Append("니아 기본 공격 횟수 : ");
            sb.Append(NiaDefaultAttackCount);
            sb.Append("\n");
            sb.Append("리아 특수 공격 횟수 : ");
            sb.Append(RiaSpecialAttackCount);
            sb.Append("\n");
            sb.Append("니아 특수 공격 횟수 : ");
            sb.Append(NiaSpecialAttackCount);
            sb.Append("\n");
            sb.Append("리아 점프 횟수 : ");
            sb.Append(RiaJumpCount);
            sb.Append("\n");
            sb.Append("니아 점프 횟수 : ");
            sb.Append(NiaJumpCount);
            sb.Append("\n");
            sb.Append("리아 이동 시간 : ");
            sb.Append(string.Format("{0:0.0}", RiaMoveTime));
            sb.Append("\n");
            sb.Append("니아 이동 시간 : ");
            sb.Append(string.Format("{0:0.0}", NiaMoveTime));
            sb.Append("\n");
            sb.Append("리아 정지 시간 : ");
            sb.Append(string.Format("{0:0.0}", RiaStopTime));
            sb.Append("\n");
            sb.Append("니아 정지 시간 : ");
            sb.Append(string.Format("{0:0.0}", NiaStopTime));
            sb.Append("\n");
            sb.Append("보스에게 준 데미지(리아) : ");
            sb.Append(BossDamagedByRia);
            sb.Append("\n");
            sb.Append("보스에게 준 데미지(니아) : ");
            sb.Append(BossDamagedByNia);
            sb.Append("\n");
            sb.Append("보스에게 준 무력화 수치(리아) : ");
            sb.Append(BossNeutralizedByRia);
            sb.Append("\n");
            sb.Append("보스에게 준 무력화 수치(니아) : ");
            sb.Append(BossNeutralizedByNia);
            sb.Append("\n");
            if (IsBossKilled)
            {
                sb.Append("보스 처치 여부 : 성공!\n");
            }
            else
            {
                sb.Append("보스 처치 여부 : 실패...\n");
            }
            sb.Append("리아 사망 횟수 : ");
            sb.Append(RiaDeadCount);
            sb.Append("\n");
            sb.Append("니아 사망 횟수 : ");
            sb.Append(NiaDeadCount);
            sb.Append("\n");
            sb.Append("리아에 의해 부활된 횟수(니아) : ");
            sb.Append(RiaReviveCountByNia);
            sb.Append("\n");
            sb.Append("니아에 의해 부활된 횟수(리아) : ");
            sb.Append(NiaReviveCountByRia);
            sb.Append("\n");
            sb.Append("리아 자가 부활 횟수 : ");
            sb.Append(RiaReviveCountSelf);
            sb.Append("\n");
            sb.Append("니아 자가 부활 횟수 : ");
            sb.Append(NiaReviveCountSelf);
            sb.Append("\n");
            sb.Append("리아 쉴드 사용 횟수 : ");
            sb.Append(RiaShieldCount);
            sb.Append("\n");
            sb.Append("니아 쉴드 사용 횟수 : ");
            sb.Append(NiaShieldCount);
            sb.Append("\n");
            sb.Append("리아 쉴드 파괴 횟수 : ");
            sb.Append(RiaShieldBrokenCount);
            sb.Append("\n");
            sb.Append("니아 쉴드 파괴 횟수 : ");
            sb.Append(NiaShieldBrokenCount);
            sb.Append("\n");
            sb.Append("리아 스턴 시간 : ");
            sb.Append(string.Format("{0:0.0}", RiaStunTime));
            sb.Append("\n");
            sb.Append("니아 스턴 시간 : ");
            sb.Append(string.Format("{0:0.0}", NiaStunTime));
            sb.Append("\n\n");
            
            //sb.Append(GetAchievementData());

            int sentenceNum = Random.Range(1, MAX_SENTENCE + 1);
            randomData.AddRange(RANDOM_DATA);
            for(int i = 0; i < sentenceNum; i++)
            {
                int randomIndex = Random.Range(0, randomData.Count);
                string data = randomData[randomIndex];
                randomData.RemoveAt(randomIndex);
                sb.Append(data);
                sb.Append("\n");
            }
            
            return sb.ToString();
        }

        public string GetCommonData()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("게임 시간 : ");
            sb.Append(string.Format("{0:00}:{1:00}", (int)(gameTime / 60), (int)(gameTime % 60)));
            sb.Append("\n");
            if (IsBossKilled)
            {
                sb.Append("보스 처치 여부 : 성공!\n");
            }
            else
            {
                sb.Append("보스 처치 여부 : 실패...\n");
            }

            return sb.ToString();
        }

        public string GetRiaData()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("리아 기본 공격 횟수 : ");
            sb.Append(RiaDefaultAttackCount);
            sb.Append("\n");
            sb.Append("리아 특수 공격 횟수 : ");
            sb.Append(RiaSpecialAttackCount);
            sb.Append("\n");
            sb.Append("리아 점프 횟수 : ");
            sb.Append(RiaJumpCount);
            sb.Append("\n");
            sb.Append("리아 이동 시간 : ");
            sb.Append(string.Format("{0:0.0}", RiaMoveTime));
            sb.Append("\n");
            sb.Append("리아 정지 시간 : ");
            sb.Append(string.Format("{0:0.0}", RiaStopTime));
            sb.Append("\n");
            sb.Append("보스에게 준 데미지(리아) : ");
            sb.Append(BossDamagedByRia);
            sb.Append("\n");
            sb.Append("보스에게 준 무력화 수치(리아) : ");
            sb.Append(BossNeutralizedByRia);
            sb.Append("\n");
            sb.Append("리아 사망 횟수 : ");
            sb.Append(RiaDeadCount);
            sb.Append("\n");
            sb.Append("리아에 의해 부활된 횟수(니아) : ");
            sb.Append(RiaReviveCountByNia);
            sb.Append("\n");
            sb.Append("리아 자가 부활 횟수 : ");
            sb.Append(RiaReviveCountSelf);
            sb.Append("\n");
            sb.Append("리아 쉴드 사용 횟수 : ");
            sb.Append(RiaShieldCount);
            sb.Append("\n");
            sb.Append("리아 쉴드 파괴 횟수 : ");
            sb.Append(RiaShieldBrokenCount);
            sb.Append("\n");
            sb.Append("리아 스턴 시간 : ");
            sb.Append(string.Format("{0:0.0}", RiaStunTime));
            sb.Append("\n\n");
            
            randomData.AddRange(RANDOM_DATA);
            int randomIndex = Random.Range(0, randomData.Count);
            string data = randomData[randomIndex];
            randomData.RemoveAt(randomIndex);
            sb.Append(data);
            sb.Append("\n");
            
            return sb.ToString();
        }

        public string GetNiaData()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("니아 기본 공격 횟수 : ");
            sb.Append(NiaDefaultAttackCount);
            sb.Append("\n");
            sb.Append("니아 특수 공격 횟수 : ");
            sb.Append(NiaSpecialAttackCount);
            sb.Append("\n");
            sb.Append("니아 점프 횟수 : ");
            sb.Append(NiaJumpCount);
            sb.Append("\n");
            sb.Append("니아 이동 시간 : ");
            sb.Append(string.Format("{0:0.0}", NiaMoveTime));
            sb.Append("\n");
            sb.Append("니아 정지 시간 : ");
            sb.Append(string.Format("{0:0.0}", NiaStopTime));
            sb.Append("\n");
            sb.Append("보스에게 준 데미지(니아) : ");
            sb.Append(BossDamagedByNia);
            sb.Append("\n");
            sb.Append("보스에게 준 무력화 수치(니아) : ");
            sb.Append(BossNeutralizedByNia);
            sb.Append("\n");
            sb.Append("니아 사망 횟수 : ");
            sb.Append(NiaDeadCount);
            sb.Append("\n");
            sb.Append("니아에 의해 부활된 횟수(리아) : ");
            sb.Append(NiaReviveCountByRia);
            sb.Append("\n");
            sb.Append("니아 자가 부활 횟수 : ");
            sb.Append(NiaReviveCountSelf);
            sb.Append("\n");
            sb.Append("니아 쉴드 사용 횟수 : ");
            sb.Append(NiaShieldCount);
            sb.Append("\n");
            sb.Append("니아 쉴드 파괴 횟수 : ");
            sb.Append(NiaShieldBrokenCount);
            sb.Append("\n");
            sb.Append("니아 스턴 시간 : ");
            sb.Append(string.Format("{0:0.0}", NiaStunTime));
            sb.Append("\n\n");

            randomData.AddRange(RANDOM_DATA);
            int randomIndex = Random.Range(0, randomData.Count);
            string data = randomData[randomIndex];
            randomData.RemoveAt(randomIndex);
            sb.Append(data);
            sb.Append("\n");
            
            return sb.ToString();
        }
    
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref gameTime);
            serializer.SerializeValue(ref riaDefaultAttackCount);
            serializer.SerializeValue(ref niaDefaultAttackCount);
            serializer.SerializeValue(ref riaSpecialAttackCount);
            serializer.SerializeValue(ref niaSpecialAttackCount);
            serializer.SerializeValue(ref riaJumpCount);
            serializer.SerializeValue(ref niaJumpCount);
            serializer.SerializeValue(ref riaMoveTime);
            serializer.SerializeValue(ref niaMoveTime);
            serializer.SerializeValue(ref riaStopTime);
            serializer.SerializeValue(ref niaStopTime);
            serializer.SerializeValue(ref bossDamagedByRia);
            serializer.SerializeValue(ref bossDamagedByNia);
            serializer.SerializeValue(ref bossNeutralizedByRia);
            serializer.SerializeValue(ref bossNeutralizedByNia);
            serializer.SerializeValue(ref isBossKilled);
            serializer.SerializeValue(ref riaDeadCount);
            serializer.SerializeValue(ref niaDeadCount);
            serializer.SerializeValue(ref riaReviveCountByNia);
            serializer.SerializeValue(ref niaReviveCountByRia);
            serializer.SerializeValue(ref riaReviveCountSelf);
            serializer.SerializeValue(ref niaReviveCountSelf);
            serializer.SerializeValue(ref riaShieldCount);
            serializer.SerializeValue(ref niaShieldCount);
            serializer.SerializeValue(ref riaShieldBrokenCount);
            serializer.SerializeValue(ref niaShieldBrokenCount);
            serializer.SerializeValue(ref riaStunTime);
            serializer.SerializeValue(ref niaStunTime);
        }

        public bool Equals(PlayData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return gameTime.Equals(other.gameTime) 
                   && riaDefaultAttackCount == other.riaDefaultAttackCount 
                   && niaDefaultAttackCount == other.niaDefaultAttackCount 
                   && riaSpecialAttackCount == other.riaSpecialAttackCount 
                   && niaSpecialAttackCount == other.niaSpecialAttackCount 
                   && riaJumpCount == other.riaJumpCount 
                   && niaJumpCount == other.niaJumpCount 
                   && riaMoveTime.Equals(other.riaMoveTime)
                   && niaMoveTime.Equals(other.niaMoveTime) 
                   && riaStopTime.Equals(other.riaStopTime) 
                   && niaStopTime.Equals(other.niaStopTime) 
                   && bossDamagedByRia == other.bossDamagedByRia 
                   && bossDamagedByNia == other.bossDamagedByNia 
                   && bossNeutralizedByRia == other.bossNeutralizedByRia 
                   && bossNeutralizedByNia == other.bossNeutralizedByNia 
                   && isBossKilled == other.isBossKilled 
                   && riaDeadCount == other.riaDeadCount 
                   && niaDeadCount == other.niaDeadCount 
                   && riaReviveCountByNia == other.riaReviveCountByNia 
                   && niaReviveCountByRia == other.niaReviveCountByRia 
                   && riaReviveCountSelf == other.riaReviveCountSelf 
                   && niaReviveCountSelf == other.niaReviveCountSelf 
                   && riaShieldCount == other.riaShieldCount 
                   && niaShieldCount == other.niaShieldCount 
                   && riaShieldBrokenCount == other.riaShieldBrokenCount 
                   && niaShieldBrokenCount == other.niaShieldBrokenCount 
                   && riaStunTime.Equals(other.riaStunTime) 
                   && niaStunTime.Equals(other.niaStunTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PlayData)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(gameTime);
            hashCode.Add(riaDefaultAttackCount);
            hashCode.Add(niaDefaultAttackCount);
            hashCode.Add(riaSpecialAttackCount);
            hashCode.Add(niaSpecialAttackCount);
            hashCode.Add(riaJumpCount);
            hashCode.Add(niaJumpCount);
            hashCode.Add(riaMoveTime);
            hashCode.Add(niaMoveTime);
            hashCode.Add(riaStopTime);
            hashCode.Add(niaStopTime);
            hashCode.Add(bossDamagedByRia);
            hashCode.Add(bossDamagedByNia);
            hashCode.Add(bossNeutralizedByRia);
            hashCode.Add(bossNeutralizedByNia);
            hashCode.Add(isBossKilled);
            hashCode.Add(riaDeadCount);
            hashCode.Add(niaDeadCount);
            hashCode.Add(riaReviveCountByNia);
            hashCode.Add(niaReviveCountByRia);
            hashCode.Add(riaReviveCountSelf);
            hashCode.Add(niaReviveCountSelf);
            hashCode.Add(riaShieldCount);
            hashCode.Add(niaShieldCount);
            hashCode.Add(riaShieldBrokenCount);
            hashCode.Add(niaShieldBrokenCount);
            hashCode.Add(riaStunTime);
            hashCode.Add(niaStunTime);
            return hashCode.ToHashCode();
        }
    }
}
