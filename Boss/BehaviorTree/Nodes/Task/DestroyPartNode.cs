namespace BirdCase
{
    public class DestroyPartNode : TaskNode
    {
        private BossOne bossOne;
        
        public override void OnCreate()
        {
            description = "패턴이 실패 했음을 반환합니다.";
        }

        protected override void OnStart()
        {
            bossOne = (BossOne) agent;
        }

        protected override void OnStop()
        {
        }

        protected override void OnAbort()
        {
        }

        protected override ENodeState OnUpdate()
        {
            // 이 패턴이 실패했기에 Success를 반환하지 않고 Failure를 반환합니다.
            return ENodeState.Failure;
        }
    }
}