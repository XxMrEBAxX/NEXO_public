using UnityEngine;

namespace BirdCase
{
    public class FollowTransform : MonoBehaviour
    {
        [SerializeField] private Transform target;
        public bool IsFollow = true;
        private void Start()
        {
            if (ReferenceEquals(target, null))
            {
                Debug.LogError("FollowTransform target is null");
                return;
            }
        }

        private void Update()
        {
            if (!ReferenceEquals(target, null) && IsFollow)
            {
                transform.position = target.position;
                Vector3 offset = target.position - transform.position;
                foreach(Launcher child in GetComponentsInChildren<Launcher>())
                {
                    child.transform.position += offset;
                }
            }
        }
    }
}
