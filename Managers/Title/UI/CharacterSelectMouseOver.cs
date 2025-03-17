using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BirdCase
{
    public class CharacterSelectMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool mouse_over = false;
        
        [SerializeField]
        private GameObject obj;

        private void Start()
        {
            obj.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!mouse_over)
            {
                mouse_over = true;
                obj.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (mouse_over)
            {
                mouse_over = false;
                obj.SetActive(false);
            }
        }
    }
}
