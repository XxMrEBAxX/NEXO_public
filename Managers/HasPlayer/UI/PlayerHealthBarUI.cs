using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class PlayerHealthBarUI : MonoBehaviour
    {
        public Image HealthBarFill;
        private float healthBarFillWidth;
        private float startOffset;
        private float endOffset;

        private void Start()
        {
            healthBarFillWidth = HealthBarFill.rectTransform.rect.width;
            startOffset = HealthBarFill.rectTransform.anchoredPosition.x - healthBarFillWidth;
            endOffset = HealthBarFill.rectTransform.anchoredPosition.x;
        }
        
        // 현재 체력의 양에 따라 체력바를 설정합니다.
        public void SetHealthBar(int currentHealth, int maxHealth)
        {
            float fillAmount = (float)currentHealth / maxHealth;
            HealthBarFill.rectTransform.anchoredPosition = new Vector3(Mathf.Lerp(startOffset, endOffset, fillAmount), HealthBarFill.rectTransform.anchoredPosition.y, 0);
        }
    }
}
