using System;
using Cysharp.Threading.Tasks;

namespace BirdCase
{
    public class SummonPatternNode : TaskNode
    {
        public EBossSummonPattern bossPattern;
        private BossOne bossOne;
        private Action completeCallback;

        public override void OnCreate()
        {
            description = "보스의 소환수 패턴 중 하나를 실행합니다.";
        }

        protected override void OnStart()
        {
            bossOne = agent as BossOne;
            blackboard.isSummonPatternProgress = true;
            completeCallback = PatternComplete;
            bossOne.BossSummonPattern(bossPattern, completeCallback);
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
            return ENodeState.Success;
        }

        private void PatternComplete()
        {
            blackboard.isSummonPatternProgress = false;
        }
    }
}
