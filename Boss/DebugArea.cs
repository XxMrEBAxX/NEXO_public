using UnityEditor;
using UnityEngine;

namespace BirdCase
{
    public class DebugArea : MonoBehaviour
    {
        public enum EAreaType : byte
        {
            HEIGHT,
            LEFT_WIDTH,
            RIGHT_WIDTH
        }

        public EAreaType AreaType;

        // private void OnDrawGizmosSelected()
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            switch (AreaType)
            {
                case EAreaType.HEIGHT:
                    Gizmos.DrawWireCube(transform.position, new Vector3(70, 0.1f, 20));
                    break;
                case EAreaType.LEFT_WIDTH:
                case EAreaType.RIGHT_WIDTH:
                    Gizmos.DrawWireCube(transform.position, new Vector3(0.1f, 30, 20));
                    break;
            }
        }
    }
}
