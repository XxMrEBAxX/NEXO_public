#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace BirdCase
{
    public class CalculateArea : MonoBehaviour
    {
        public enum EAreaNumber : byte
        {
            ONE = 1,
            TWO,
            THREE,
            FOUR,
            FIVE,
            SIX
        }
        [SerializeField] private GameObject leftArea;
        [SerializeField] private GameObject rightArea;
        [SerializeField] private GameObject heightArea;

        public EAreaNumber CalculateAreaNumber(Vector3 v)
        {
            if (v.y > heightArea.transform.position.y)
            {
                if (v.x < leftArea.transform.position.x)
                {
                    return EAreaNumber.ONE;
                }
                else if (v.x > rightArea.transform.position.x)
                {
                    return EAreaNumber.FIVE;
                }
                else
                {
                    return EAreaNumber.THREE;
                }
            }
            else
            {
                if (v.x < leftArea.transform.position.x)
                {
                    return EAreaNumber.TWO;
                }
                else if (v.x > rightArea.transform.position.x)
                {
                    return EAreaNumber.SIX;
                }
                else
                {
                    return EAreaNumber.FOUR;
                }
            }
        }

#if UNITY_EDITOR
        //private void OnDrawGizmosSelected()
        private void OnDrawGizmos()
        {
            Vector3 leftPosition = leftArea.transform.position;
            leftPosition.y = heightArea.transform.position.y;
            leftArea.transform.position = leftPosition;

            Vector3 rightPosition = rightArea.transform.position;
            rightPosition.y = heightArea.transform.position.y;
            rightArea.transform.position = rightPosition;

            float leftDifferentX = heightArea.transform.position.x - leftArea.transform.position.x;
            float rightDifferentX = rightArea.transform.position.x - heightArea.transform.position.x;
            Handles.Label(leftArea.transform.position + Vector3.up * 5 + Vector3.left * leftDifferentX, "Area 1", EditorStyles.centeredGreyMiniLabel);
            Handles.Label(leftArea.transform.position + Vector3.down * 5 + Vector3.left * leftDifferentX, "Area 2", EditorStyles.centeredGreyMiniLabel);
            Handles.Label(heightArea.transform.position + Vector3.up * 5, "Area 3", EditorStyles.centeredGreyMiniLabel);
            Handles.Label(heightArea.transform.position + Vector3.down * 5, "Area 4", EditorStyles.centeredGreyMiniLabel);
            Handles.Label(rightArea.transform.position + Vector3.up * 5 + Vector3.right * rightDifferentX, "Area 5", EditorStyles.centeredGreyMiniLabel);
            Handles.Label(rightArea.transform.position + Vector3.down * 5 + Vector3.right * rightDifferentX, "Area 6", EditorStyles.centeredGreyMiniLabel);
        }
#endif
    }
}
