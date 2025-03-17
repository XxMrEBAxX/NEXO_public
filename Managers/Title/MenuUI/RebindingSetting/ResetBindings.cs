using UnityEngine;
using UnityEngine.InputSystem;

namespace BirdCase
{
    public class ResetBindings : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputAction;
        [SerializeField] private string targetControlScheme;

        public void ResetAllBindings()
        {
            foreach (InputActionMap map in inputAction.actionMaps)
            {
                map.RemoveAllBindingOverrides();
            }
        }
        
        public void ResetControlSchemeBindings()
        {
            foreach (InputActionMap map in inputAction.actionMaps)
            {
                foreach (InputAction action in map.actions)
                {
                    action.RemoveBindingOverride(InputBinding.MaskByGroup(targetControlScheme));
                }
            }
        }
    }
}
