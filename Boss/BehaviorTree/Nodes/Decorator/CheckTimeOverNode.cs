using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BirdCase
{
    public class CheckTimeOverNode : DecoratorNode
    {
        private bool isCheck = false;   // 한번만 실행 시키기 위해서 변수 사용


        public override void OnCreate()
        {
            description = "게임 시간이 지났는지 확인합니다.";
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

            InGamePlayManager inGamePlayManager = PlayManager.Instance as InGamePlayManager;
            if (isCheck)
            {
                return ENodeState.Failure;
            }

            if (inGamePlayManager.PlayTime >= inGamePlayManager.GameEndPlayTime)
            {
                return child.Update();
            }

            return ENodeState.Failure;
        }
    }
}
