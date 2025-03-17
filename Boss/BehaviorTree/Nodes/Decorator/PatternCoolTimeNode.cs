using UnityEngine;

namespace BirdCase
{
    public class PatternCoolTimeNode : DecoratorNode
    {
        public override void OnCreate()
        {
            description = "패턴 쿨타임이 있는지 확인합니다.";
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
            if (child == null || blackboard.PatternCurrentTime + blackboard.PatternCoolTime > Time.time)
            {
                return ENodeState.Failure;
            }

            return child.Update();
        }
    }
}