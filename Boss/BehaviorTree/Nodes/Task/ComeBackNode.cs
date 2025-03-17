using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace BirdCase
{
    public class ComeBackNode : TaskNode
    {
        public float MoveSpeed = 1.0f;
        public float AttackCompleteDelay = 1.0f;

        private Vector3 partPos;
        private BossOne bossOne;
        private BossParts[] bossParts;
        private BossParts curBossParts;
        private UniTask task;
        private bool isTaskRunning;

        public override void OnCreate()
        {
            description = "공격이 끝난 후 원래 위치로 돌아옵니다.";
            task = UniTask.CompletedTask;
        }

        protected override void OnStart()
        {
            bossOne = agent.GetComponentInChildren<BossOne>();
            bossParts = bossOne.GetComponentsInChildren<BossParts>();

            foreach (BossParts part in bossParts)
            {
                if (part.BossPart == EBossParts.RIGHT_ARM)
                {
                    curBossParts = part;
                    
                    if (isTaskRunning) // 작업이 실행 중이면 종료
                    {
                        return;
                    }
                    
                    task = ComeBackTask(AttackCompleteDelay);
                    task.Forget();

                    return;
                }
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
            if(task.Status == UniTaskStatus.Succeeded)
            {
                return ENodeState.Success;
            }
            
            return ENodeState.InProgress;
        }

        /// <summary>
        /// 공격이 종료된 후 원래 위치로 돌아옵니다.
        /// </summary>
        private void ComeBackHand(BossParts part)
        {
        }

        private async UniTask ComeBackTask(float delay)
        {
            isTaskRunning = true;
            Debug.Log("SwingAttackTask 실행 중");
            try
            {
                ComeBackHand(curBossParts);
                await UniTask.Delay(TimeSpan.FromSeconds(delay));

            }
            catch (Exception e) when (e is OperationCanceledException)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                isTaskRunning = false; // 작업이 끝나면 false로 변경
                task = UniTask.CompletedTask;
            }
        }
    }
}
