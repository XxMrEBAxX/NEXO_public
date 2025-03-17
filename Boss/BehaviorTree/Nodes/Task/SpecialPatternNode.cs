using System;
using Cysharp.Threading.Tasks;

namespace BirdCase
{
    public class SpecialPatternNode : TaskNode
    {
        public EBossSpecialPattern bossPattern;
        private BossOne bossOne;
        private Action completeCallback;
        public bool isDisable = false;

        public override void OnCreate()
        {
            description = "보스의 특별 패턴 중 하나를 실행합니다.";
        }

        protected override void OnStart()
        {
            if (isDisable)
                return;
                
            bossOne = agent as BossOne;
            blackboard.isSpecialPatternProgress = true;
            completeCallback = PatternComplete;
            bossOne.BossSpecialPattern(bossPattern, completeCallback);
            blackboard.SetPatternCoolTime();
        }

        protected override void OnStop()
        {
        }

        protected override void OnAbort()
        {

        }

        protected override ENodeState OnUpdate()
        {
            if (isDisable)
            {
                return ENodeState.Failure;
            }
            return ENodeState.Success;
        }

        private void PatternComplete()
        {
            blackboard.isSpecialPatternProgress = false;
        }
    }
}
