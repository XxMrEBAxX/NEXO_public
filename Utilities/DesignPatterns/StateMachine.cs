using System;

namespace BirdCase
{
    public abstract class StateMachine<T>
    {
        public Action<T> action;

        public StateMachine()
        {
            action = Enter;
        }
        
        public virtual void Init()
        {
            action = Enter;
        }
        
        public virtual void Enter(T t)
        {
            action = Update;
        }

        public virtual void Update(T t) {}
        public virtual void FixedUpdate(T t) {}
    
        public virtual void Exit(T t) {}
    }
}
