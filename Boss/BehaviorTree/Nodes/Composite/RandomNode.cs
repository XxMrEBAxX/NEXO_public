using UnityEngine;

namespace BirdCase
{
    public class RandomNode : CompositeNode
    {
        private int randomIndex;
        
        public override void OnCreate()
        {
            description = "자신의 자식들 중 하나를 실행합니다.";
        }

        protected override void OnStart()
        {
            // 자식 노드 중 랜덤하게 하나 생성
            randomIndex = Random.Range(0, children.Count);
        }

        protected override void OnStop()
        {
        }

        protected override void OnAbort()
        {
        }

        protected override ENodeState OnUpdate()
        {
            if (children.Count == 0)
            {
                return ENodeState.Success;
            }
            
            var child = children[randomIndex];
            var result = child.Update();

            switch (result)
            {
                case ENodeState.InProgress:
                    return ENodeState.InProgress;
                case ENodeState.Success:
                    return ENodeState.Success;
                case ENodeState.Failure:
                    return ENodeState.Failure;
            }

            return ENodeState.Failure;
        }
    }
}