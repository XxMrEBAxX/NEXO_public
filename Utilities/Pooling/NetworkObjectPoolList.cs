using System.Collections.Generic;
using UnityEngine;

namespace BirdCase
{
    [CreateAssetMenu(menuName = "Utilities/Pooling/NetworkObjectPoolList")]
    public class NetworkObjectPoolList : ScriptableObject
    {
        [SerializeField] 
        private List<PoolConfigObject> pooledPrefabsList;
        public List<PoolConfigObject> PooledPrefabsList => pooledPrefabsList;
    }
}
