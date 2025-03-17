using System;
using UnityEngine;

namespace BirdCase
{
    public interface IAffectByExplosion
    {
        public void AffectByExplosion(Vector3 explosionCenterPosition, LauncherBaseData.ExplosionData explosionData, int damage, int neutralizeValue, PlayerName playerName);
        public ObjectSize GetObjectSize();
    }

    public interface IGetOffLauncher
    {
        public event Action<IGetOffLauncher> GetOffLauncher;
    }
}
