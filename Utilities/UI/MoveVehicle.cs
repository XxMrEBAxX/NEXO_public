using UnityEngine;

namespace BirdCase
{
    public class MoveVehicle : MonoBehaviour
    {
        //[SerializeField] private GameObject[] vehicles;
        [SerializeField] private float moveSpeed = 10.0f;
        [SerializeField] private float arriveDistance = 300.0f;
        
        private Vector3 oriPosition;
        
        private void Start()
        {
            oriPosition = transform.position;
        }
        
        private void Update()
        {
            transform.Translate(Vector3.right * (moveSpeed * Time.deltaTime));
            
            if(Vector3.Distance(oriPosition, transform.position) >= arriveDistance)
            {
                transform.position = oriPosition;
            }
        } 
    }
}
