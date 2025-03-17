using System.Threading.Tasks;
using UnityEngine;

namespace BirdCase
{
    public class DebugLogNode : TaskNode
    {
        public string message;

        private Task task;

        public override void OnCreate()
        {
            description = "디버그용 로그를 출력합니다.";
        }

        protected override void OnStart()
        {
            Debug.Log($"OnStart {message}");
        }

        protected override void OnStop()
        {
            Debug.Log($"OnStop {message}");
        }

        protected override void OnAbort()
        {
            Debug.Log($"OnAbort {message}");
        }

        protected override ENodeState OnUpdate()
        {
            Debug.Log($"OnUpdate {message}");
            return ENodeState.Success;
        }
    }
}