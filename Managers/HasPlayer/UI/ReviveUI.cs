using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class ReviveUI : NetworkBehaviour
    {
        [SerializeField]
        private Vector2 offset;

        private Image[] reviveGauges;
        private RectTransform rectTransform;
        private Camera camera;

        private float gaugeValueDevide;
        
        private int enabledGaugeCount = 0;
        
        private void Start()
        {
            camera = GameObject.Find("UI Camera").GetComponent<Camera>();
            rectTransform = GetComponent<RectTransform>();
            reviveGauges = transform.GetChild(0).GetComponentsInChildren<Image>();
            gameObject.SetActive(false);
            gaugeValueDevide = 1.0f / (reviveGauges.Length + 1);
        }

        private void OnDisable()
        {
            enabledGaugeCount = 0;
            if (reviveGauges != null)
            {
                for (int i = 0; i < reviveGauges.Length; i++)
                {
                    if(reviveGauges[i] != null)
                        reviveGauges[i].enabled = false;
                }
            }
        }

        [ServerRpc (RequireOwnership = false)]
        public void SetReviveGaugeServerRPC(Vector2 pos, float value)
        {
            SetReviveGaugeClientRPC(pos, value);
        }
        
        [ClientRpc]
        private void SetReviveGaugeClientRPC(Vector2 pos, float value)
        {
            Vector2 playerScreenPosition = RectTransformUtility.WorldToScreenPoint(camera, pos);
            playerScreenPosition /= HUDPresenter.GET_CANVAS_RESOLUTION_ASPECT_RATIO;
            playerScreenPosition += offset;
            rectTransform.anchoredPosition = playerScreenPosition;
            
            if (value == 0)
            {
                gameObject.SetActive(true);
            }
            else if (value == 1)
            {
                gameObject.SetActive(false);
            }
            else
            {
                int setGaugeCount = Mathf.Min((int)(value / gaugeValueDevide), reviveGauges.Length);
                for(int i = enabledGaugeCount; i < setGaugeCount; i++)
                {
                    reviveGauges[i].enabled = true;
                }
                enabledGaugeCount = setGaugeCount;
            }
        }
    }
}
