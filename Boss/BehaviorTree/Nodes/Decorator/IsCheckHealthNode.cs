using UnityEngine;

namespace BirdCase
{
    public class IsCheckHealthNode : DecoratorNode
    {
        public override void OnCreate()
        {
            description = "체력이 남아있는지 확인합니다.";
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
            if (child == null || agent.CurrentHealth.Value <= 0)
            {
                return ENodeState.Failure;
            }

            return child.Update();
        }
    }
}