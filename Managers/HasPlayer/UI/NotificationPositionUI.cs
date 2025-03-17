using System;
using Mono.CSharp;
using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class NotificationPositionUI : MonoBehaviour
    {
        private readonly Color WARNING_COLOR = new Color(0.2901961f, 0.3333333f, 0.3803922f, 1);
        private const float IMAGE_ORI_ANGLE = -90;
        
        [SerializeField] 
        private float freeSpace = 50.0f;

        private RectTransform rectTransform;
        private RectTransform positionNotification;
        private UIHologram positionNotificationHologram;
        private RectTransform warningImage;
        private RectTransform defaultImage;
        private bool isOnScreen;

        private Color oriColor;
        
        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            positionNotification = transform.GetChild(0).GetComponent<RectTransform>();
            positionNotificationHologram = positionNotification.GetComponent<UIHologram>();
            warningImage = positionNotification.GetChild(1).GetComponent<RectTransform>();
            defaultImage = positionNotification.GetChild(0).GetComponent<RectTransform>();
            oriColor = positionNotification.GetComponent<Image>().color;
            defaultImage.gameObject.SetActive(false);
            warningImage.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 타겟의 위치가 카메라에서 벗어나면 UI를 활성화합니다.
        /// </summary>
        public void CheckUIActive(Vector3 targetPosition, bool isAlive)
        {
            Vector3 viewPosition = Camera.main.WorldToViewportPoint(targetPosition);
    
            isOnScreen = viewPosition.x >= -0.01f && viewPosition.x <= 1.01f &&
                         viewPosition.y >= -0.3f && viewPosition.y <= 1;
    
            gameObject?.SetActive(!isOnScreen);
            warningImage.gameObject.SetActive(!isAlive);
            defaultImage.gameObject.SetActive(isAlive);
            positionNotificationHologram.TextureColor = isAlive ? oriColor : WARNING_COLOR;
            
            if(!isOnScreen)
                SetNotificationPosition(targetPosition);
        }
    
        /// <summary>
        /// 활성화된 UI의 위치를 플레이어의 위치에 맞게 조정합니다.
        /// </summary>
        private void SetNotificationPosition(Vector3 targetPosition)
        {
            Vector3 playerScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, targetPosition);
    
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            Vector3 edgePosition = Vector3.zero;
    
            edgePosition.x = Mathf.Clamp(playerScreenPosition.x, freeSpace, screenWidth - (freeSpace));
            edgePosition.y = Mathf.Clamp(playerScreenPosition.y, freeSpace, screenHeight - (freeSpace));
            edgePosition /= HUDPresenter.GET_CANVAS_RESOLUTION_ASPECT_RATIO;
            rectTransform.anchoredPosition = edgePosition;
            
            playerScreenPosition /= HUDPresenter.GET_CANVAS_RESOLUTION_ASPECT_RATIO;
            Vector2 direction = (edgePosition - playerScreenPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            positionNotification.rotation = Quaternion.Euler(0, 0, angle + IMAGE_ORI_ANGLE);
            warningImage.localRotation = Quaternion.Euler(0, 0, -(angle + IMAGE_ORI_ANGLE));
        }
    }
}
