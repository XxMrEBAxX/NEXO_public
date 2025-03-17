using UnityEngine.Networking;
using UnityEngine;
using Unity.Netcode;

namespace BirdCase
{
    public abstract class NetworkSingleton<T> : NetworkBehaviour
            where T : NetworkBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (ReferenceEquals(instance, null))
                {
                    instance = FindObjectOfType<T>();
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = GetComponent<T>();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            OnAwake();
        }

        protected abstract void OnAwake();
    }
    
    public abstract class Singleton<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (ReferenceEquals(instance, null))
                {
                    instance = FindObjectOfType<T>();
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = GetComponent<T>();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            OnAwake();
        }

        protected abstract void OnAwake();
    }
}
