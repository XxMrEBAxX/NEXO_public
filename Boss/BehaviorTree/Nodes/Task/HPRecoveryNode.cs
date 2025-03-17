using UnityEngine;

namespace BirdCase
{
    public class HPRecoveryNode : TaskNode
    {
        [Range(0, 100)] public int SetPercent = 75;

        private float preservePercent; // 1% 체력을 저장할 변수

        public override void OnCreate()
        {
            description = "보스 체력을 지정한 퍼센트로 변경합니다.";
        }

        protected override void OnStart()
        {
            if (agent != null && agent.BossData != null)
            {
                agent.HPRatioModified(SetPercent);
            }
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