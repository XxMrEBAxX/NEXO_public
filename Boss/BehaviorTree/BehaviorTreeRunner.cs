using UnityEngine;

namespace BirdCase
{
    public class BehaviorTreeRunner : MonoBehaviour
    {
        public BehaviorTree tree;

        private void Start()
        {
            Debug.Log("BehaviorTreeRunner Start");
            tree = tree.Clone();
            tree.Bind(GetComponent<BossOne>());
        }

        private void Update()
        {
            tree.Update();
        }
    }
}