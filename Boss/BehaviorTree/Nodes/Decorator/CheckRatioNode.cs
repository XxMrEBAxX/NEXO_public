using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BirdCase
{
    public class CheckRatioNode : DecoratorNode
    {
        [Range(0,100)] public int SetPercent = 70;
        public ECooperativePattern cooperativePattern;
        public bool IsOnlyPhase2 = false;
        
        private float preservePercent;  // 1% 체력을 저장할 변수
        private bool isCheck = false;   // 한번만 실행 시키기 위해서 변수 사용
        
        
        public override void OnCreate()
        {
            description = "지정된 비율과 체력을 확인합니다.";
        }

        protected override void OnStart()
        {
            if (agent != null && agent.BossData != null)
            {
                preservePercent = agent.BossData.BossHealth * 0.01f;
            }
            isCheck = agent.IsPatternExecuted(cooperativePattern);
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

            if (agent != null)
            {
                bool isNotPhase2 = IsOnlyPhase2 && !agent.IsPhase2;
                if (isCheck || isNotPhase2)
                {
                    return ENodeState.Failure;
                }

                if (agent.CurrentHealth.Value <= preservePercent * SetPercent)
                {
                    agent.PatternExecuted(cooperativePattern);
                    child.Update();
                    return ENodeState.Success;
                }

                return ENodeState.Failure;
            }
            
            return ENodeState.Failure;
        }
    }
}
