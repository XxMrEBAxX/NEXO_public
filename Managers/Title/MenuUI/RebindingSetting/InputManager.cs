using UnityEngine;
using UnityEngine.InputSystem;

namespace BirdCase
{
    public class InputManager : Singleton<InputManager>
    {
        public Vector2 MoveInput { get; private set; }
        //public bool 

        private PlayerInput playerInput;
        
        protected override void OnAwake()
        {
        }
    }
}
