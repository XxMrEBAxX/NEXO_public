using System;
using System.Collections;
using AssetKits.ParticleImage;
using Mono.CSharp;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BirdCase
{
    public class ClashBar : MonoBehaviour
    {
        private WaitForSeconds waitTime;

        public Image ClashBarFill;
        private float maxClashBarValue = 100;
        private float currentClashBarValue = 80;
        private float originValue;
        [HideInInspector] public float subtractValue;
        private float addValue = 3.5f;
        [SerializeField] private ParticleImage clashEffect;

        public event Action FailClashEvent;
        private void Awake()
        {
            originValue = currentClashBarValue;
            ClashBarFill.fillAmount = currentClashBarValue / maxClashBarValue;
            // -81.4 ~ 93.1
        }

        private void OnEnable()
        {
            currentClashBarValue = originValue;
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (currentClashBarValue <= 0)
            {
                FailClashEvent?.Invoke();
                this.gameObject.SetActive(false);
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                currentClashBarValue = Mathf.Clamp(currentClashBarValue + addValue, 0, maxClashBarValue);
                clashEffect.rectTransform.anchoredPosition = new Vector2(clashEffect.rectTransform.anchoredPosition.x, Mathf.Lerp(-81.4f, 93.1f, currentClashBarValue / maxClashBarValue));
                clashEffect.Play();
            }

            currentClashBarValue = Mathf.Clamp(currentClashBarValue - TimeManager.Instance.GetDeltaTime() * subtractValue, 0, maxClashBarValue);
            ClashBarFill.fillAmount = currentClashBarValue / maxClashBarValue;
        }
    }
}
