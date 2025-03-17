namespace BirdCase
{
    public class CheckIsPatternNode : DecoratorNode
    {
        public override void OnCreate()
        {
            description = "패턴 중이면 자식 노드를 실행하지 않습니다.";
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
            if (child == null)
            {
                return ENodeState.Aborted;
            }

            if (!blackboard.isPatternProgress)
            {
                return child.Update();
            }

            return ENodeState.Failure;
        }
    }
}