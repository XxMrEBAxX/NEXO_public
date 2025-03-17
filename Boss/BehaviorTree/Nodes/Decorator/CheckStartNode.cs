namespace BirdCase
{
    public class CheckStartNode : DecoratorNode
    {
        public override void OnCreate()
        {
            description = "보스 전투 상태 진입 여부를 확인합니다.";
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
            
            if (agent.IsBattleState)
            {
                child.Update();
                return ENodeState.Success;
            }

            return ENodeState.Failure;
        }
    }
}