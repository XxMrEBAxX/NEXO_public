using System;
using UnityEngine;

namespace BirdCase
{
    public class WeightPatternNode : TaskNode
    {
        private BossOne bossOne;
        private Action completeCallback;

        public override void OnCreate()
        {
            description = "가중치에 따른 패턴을 실행합니다.";
        }

        protected override void OnStart()
        {
            bossOne = agent as BossOne;
            blackboard.isPatternProgress = true;
            blackboard.PatternCount++;
            completeCallback = PatternComplete;
            bossOne.WeightPattern(completeCallback);
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
