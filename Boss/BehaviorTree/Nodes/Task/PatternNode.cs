using System;
using Cysharp.Threading.Tasks;

namespace BirdCase
{
    public class PatternNode : TaskNode
    {
        public EBossPattern bossPattern;
        private BossOne bossOne;
        private Action completeCallback;

        public override void OnCreate()
        {
            description = "보스의 패턴 중 하나를 실행합니다.";
        }

        protected override void OnStart()
        {
            bossOne = agent as BossOne;
            blackboard.isPatternProgress = true;
            blackboard.PatternCount++;
            completeCallback = PatternComplete;
            bossOne.BossPattern(bossPattern, completeCallback);
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
            blackboard.isPatternProgress = false;
            blackboard.SetPatternCoolTime();
        }
    }
}
