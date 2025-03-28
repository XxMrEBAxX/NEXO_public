using UnityEngine;

namespace BirdCase
{
    public abstract class Node : ScriptableObject
    {
        public enum ENodeState
        {
            InProgress,
            Failure,
            Success,
            Aborted,
        }

        [HideInInspector] public ENodeState state = ENodeState.InProgress;
        [HideInInspector] public bool started = false;
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        [HideInInspector] public Blackboard blackboard;
        [HideInInspector] public BossBase agent;
        [TextArea] public string description;
        
        public ENodeState Update()
        {
            if (!started)
            {
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state == ENodeState.Failure || state == ENodeState.Success)
            {
                OnStop();
                started = false;
            }
            else if (state == ENodeState.Aborted)
            {
                OnAbort();
                started = false;
            }

            return state;
        }

        public virtual Node Clone()
        {
            Node node = Instantiate(this);
            node.name = node.name.Replace("(Clone)", "");
            return node;
        }

        public abstract void OnCreate();
        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract void OnAbort();
        protected abstract ENodeState OnUpdate();
    }
}