using UnityEngine;
using UnityEngine.Splines;

namespace BirdCase
{
    public class SplineLerp : MonoBehaviour
    {
        [SerializeField] private SplineContainer spline;
        [SerializeField] private Transform target;
        [SerializeField, Range(0, 1)] private float lerp = 0;

        private void Start()
        {
            
        }

        private void Update()
        {
            if (spline == null) return;
            target.position = spline.EvaluatePosition(lerp);
        }

        private void OnValidate() {
            if (spline == null) return;
            target.position = spline.EvaluatePosition(lerp);
        }
    }
}
