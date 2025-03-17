using UnityEngine;

namespace BirdCase
{
    public class IsPhase2Node : DecoratorNode
    {
        public bool Reverse = false;
        public override void OnCreate()
        {
            description = "2페이즈인지 확인합니다.";
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
            if (child == null || (Reverse ? agent.IsPhase2 : !agent.IsPhase2))
            {
                return ENodeState.Failure;
            }

            return child.Update();
        }
    }
}