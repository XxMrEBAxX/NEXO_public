using UnityEngine;

namespace BirdCase
{
    public class IsNeutralizeNode : DecoratorNode
    {
        public override void OnCreate()
        {
            description = "무력화 상태인지 확인합니다.";
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override void OnAbort()
        {
        }

        protected override ENodeState OnUpdate()
        {
            if (child == null || agent.IsNeutralize)
            {
                return ENodeState.Failure;
            }

            return child.Update();
        }
    }
}