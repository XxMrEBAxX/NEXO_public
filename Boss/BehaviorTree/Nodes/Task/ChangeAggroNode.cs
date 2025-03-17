using System;
using Cysharp.Threading.Tasks;

namespace BirdCase
{
    public class ChangeAggroNode : TaskNode
    {
        private BossOne bossOne;

        public override void OnCreate()
        {
            description = "어그로를 변경합니다.";
        }

        protected override void OnStart()
        {
            bossOne = agent as BossOne;
        }

        protected override void OnStop()
        {
        }

        protected override void OnAbort()
        {
            
        }

        protected override ENodeState OnUpdate()
        {
            bossOne.FindTarget();
            return ENodeState.Success;
        }
    }
}
