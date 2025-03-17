using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace BirdCase
{
    public class StartMenuUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField ipInputField;
        [SerializeField] private Button  localJoinButton;
        
        [SerializeField] private TMP_InputField hostCodeInputField;
        [SerializeField] private Button inputCodeJoinButton;

        private void Start()
        {
            hostCodeInputField.onEndEdit.AddListener(InputEnterHostCode);
            ipInputField.onEndEdit.AddListener(InputEnterIP);
        }
        
        public void OnClickLocalButton()
        {
            ipInputField.Select();
            ipInputField.ActivateInputField();
        }
        
        public void OnClickJoinButton()
        {
            hostCodeInputField.Select();
            hostCodeInputField.ActivateInputField();
        }
        
        private void InputEnterHostCode(string code)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                inputCodeJoinButton.onClick.Invoke();
            }
            // if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            // {
            //     inputCodeJoinButton.onClick.Invoke();
            // } 
        }
        
        private void InputEnterIP(string ip)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                localJoinButton.onClick.Invoke();
            }
        }
    }
}
