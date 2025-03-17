using UnityEngine.Serialization;

namespace BirdCase
{
    public class PatternFailNode : TaskNode
    {
        public ECooperativePattern cooperativePattern;

        public override void OnCreate()
        {
            description = "패턴이 실패 했음을 반환합니다.";
        }

        protected override void OnStart()
        {
            agent.PatternFailed(cooperativePattern);
        }

        protected override void OnStop()
        {
        }

        protected override void OnAbort()
        {
        }

        protected override ENodeState OnUpdate()
        {
            return ENodeState.Success;
        }
    }
}