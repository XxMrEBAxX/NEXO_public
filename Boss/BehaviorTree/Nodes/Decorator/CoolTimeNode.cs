using UnityEngine;

namespace BirdCase
{
    public class CoolTimeNode : DecoratorNode
    {
        public float CoolTime = 0f;
        private float currentTime = float.MinValue;
        public bool isStartCoolTime = false;
        private bool isStartCoolTimeRunning = false;
        
        public override void OnCreate()
        {
            description = "지정한 시간마다 노드를 실행합니다.";
        }

        protected override void OnStart()
        {
            if(isStartCoolTime && !isStartCoolTimeRunning)
            {
                isStartCoolTimeRunning = true;
                currentTime = Time.time;
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
            if (child == null || currentTime + CoolTime > Time.time)
            {
                return ENodeState.Aborted;
            }

            currentTime = Time.time;

            return child.Update();
        }
    }
}