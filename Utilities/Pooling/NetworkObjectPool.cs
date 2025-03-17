using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.Pool;
using System;
using UnityEngine.Analytics;
using UnityEngine.Assertions;

namespace BirdCase
{
    public class NetworkObjectPool : NetworkSingleton<NetworkObjectPool>
    {
        [SerializeField]
        private List<NetworkObjectPoolList> pooledPrefabsList;
        

        private HashSet<GameObject> prefabHashSets = new HashSet<GameObject>();

        private Dictionary<GameObject, ObjectPool<NetworkObject>> pooledObjects = new Dictionary<GameObject, ObjectPool<NetworkObject>>();
        private Dictionary<string, GameObject> pooledObjectByName = new Dictionary<string, GameObject>();

        protected override void OnAwake()
        {
            
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // Registers all objects in pooledPrefabsList to the cache.
            foreach (var pooledPrefabs in pooledPrefabsList)
            {
                if (ReferenceEquals(pooledPrefabs, null))
                    return;
                
                foreach (var configObject in pooledPrefabs.PooledPrefabsList)
                {
                    RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                foreach (var prefab in prefabHashSets)
                {
                    pooledObjects[prefab].Clear();
                }
            }
            
            // Unregisters all objects in pooledPrefabsList from the cache.
            foreach (var prefab in prefabHashSets)
            {
                // Unregister Netcode Spawn 
                try
                {
                    NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            //
            // pooledObjects.Clear();
            // pooledObjectByName.Clear();
            // prefabHashSets.Clear();
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public void OnValidate()
        {
            if (pooledPrefabsList == null)
                return;
            
            foreach (var pooledPrefabs in pooledPrefabsList)
            {
                if(pooledPrefabs == null)
                    return;
                
                for (var i = 0; i < pooledPrefabs.PooledPrefabsList.Count; i++)
                {
                    var prefab = pooledPrefabs.PooledPrefabsList[i].Prefab;
                    if (prefab != null)
                    {
                        Assert.IsNotNull(prefab.GetComponent<NetworkObject>(), $"{nameof(NetworkObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
                    }
                }
            }
        }

        /// <summary>
        /// Gets an instance of the given prefab from the pool. The prefab must be registered to the pool.
        /// </summary>
        /// <remarks>
        /// To spawn a NetworkObject from one of the pools, this must be called on the server, then the instance
        /// returned from it must be spawned on the server. This method will then also be called on the client by the
        /// PooledPrefabInstanceHandler when the client receives a spawn message for a prefab that has been registered
        /// here.
        /// </remarks>
        /// <param name="prefab"></param>
        /// <param name="position">The position to spawn the object at.</param>
        /// <param name="rotation">The rotation to spawn the object with.</param>
        /// <returns></returns>
        public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var networkObject = pooledObjects[prefab].Get();

            var noTransform = networkObject.transform;
            noTransform.position = position;
            noTransform.rotation = rotation;

            return networkObject;
        }
        
        public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, float scale, Quaternion rotation)
        {
            var networkObject = pooledObjects[prefab].Get();

            var noTransform = networkObject.transform;
            noTransform.position = position;
            noTransform.rotation = rotation;
            noTransform.localScale *= scale;

            return networkObject;
        }
        
        public NetworkObject GetNetworkObject(string name, Vector3 position, float scale, Quaternion rotation)
        {
            GameObject prefab = pooledObjectByName[name];
            
            return GetNetworkObject(prefab, position, scale, rotation);
        }

        /// <summary>
        /// Return an object to the pool (reset objects before returning).
        /// </summary>
        public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
        {
            pooledObjects[prefab].Release(networkObject);
        }
        
        public void ReturnNetworkObject(NetworkObject networkObject, string name)
        {
            GameObject prefab = pooledObjectByName[name];
            
            ReturnNetworkObject(networkObject, prefab);
        }

        /// <summary>
        /// Builds up the cache for a prefab.
        /// </summary>
        private void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
        {
            NetworkObject CreateFunc()
            {
                NetworkObject networkObject = Instantiate(prefab).GetComponent<NetworkObject>();
                return networkObject;
            }

            void ActionOnGet(NetworkObject networkObject)
            {
                networkObject.gameObject.SetActive(true);
            }

            void ActionOnRelease(NetworkObject networkObject)
            {
                networkObject.gameObject.SetActive(false);
            }

            void ActionOnDestroy(NetworkObject networkObject)
            {
                Destroy(networkObject.gameObject);
            }

            prefabHashSets.Add(prefab);
            pooledObjectByName[prefab.name] = prefab;

            // Create the pool
            pooledObjects[prefab] = new ObjectPool<NetworkObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);

            // Populate the pool
            var prewarmNetworkObjects = new List<NetworkObject>();
            for (var i = 0; i < prewarmCount; i++)
            {
                prewarmNetworkObjects.Add(pooledObjects[prefab].Get());
            }
            foreach (var networkObject in prewarmNetworkObjects)
            {
                pooledObjects[prefab].Release(networkObject);
            }

            // Register Netcode Spawn handlers
            NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler(prefab, this));
        }
    }

    [Serializable]
    public struct PoolConfigObject
    {
        public GameObject Prefab;
        public int PrewarmCount;
    }

    class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        private GameObject prefab;
        private NetworkObjectPool pool;

        public PooledPrefabInstanceHandler(GameObject prefab, NetworkObjectPool pool)
        {
            this.prefab = prefab;
            this.pool = pool;
        }

        NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return pool.GetNetworkObject(prefab, position, rotation);
        }

        void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
        {
            pool.ReturnNetworkObject(networkObject, prefab);
        }
    }
}
