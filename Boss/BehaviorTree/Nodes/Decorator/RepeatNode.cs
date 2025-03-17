namespace BirdCase
{
    public class RepeatNode : DecoratorNode
    {
        public override void OnCreate()
        {
            description = "자신의 자식을 반복해서 실행합니다.";
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
            
            child.Update();
            return ENodeState.InProgress;
        }
    }
}