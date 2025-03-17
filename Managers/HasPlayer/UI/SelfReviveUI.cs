using UnityEngine;

namespace BirdCase
{
    public class SelfReviveUI : MonoBehaviour
    {
        [SerializeField]
        private Vector2 offset;

        private Animation[] reviveGauges; 
        private RectTransform rectTransform;
        private Camera camera;
        
        private float gaugeValueDevide;
        private int enabledGaugeCount = 0;
        
        private void Start()
        {
            camera = GameObject.Find("UI Camera").GetComponent<Camera>();
            rectTransform = GetComponent<RectTransform>();
            gameObject.SetActive(false);
            reviveGauges = transform.GetComponentsInChildren<Animation>();
            gaugeValueDevide = 1.0f / (reviveGauges.Length + 1);
        }

        private void OnDisable()
        {
            if (!ReferenceEquals(reviveGauges, null))
            {
                for (int i = 0; i < reviveGauges.Length; i++)
                {
                    reviveGauges[i].Stop();
                    reviveGauges[i].Rewind();

                    AnimationState state = reviveGauges[i]["Reset"];
                    state.enabled = true;
                    state.weight = 1;
                    state.normalizedTime = 0;
                    reviveGauges[i].Sample();
                    state.enabled = false;
                }
            }

            enabledGaugeCount = 0;
        }
        
        public void SetReviveGauge(Vector2 pos, float value)
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
                int gaugeCount = (int)(value / gaugeValueDevide);
                if(enabledGaugeCount < gaugeCount)
                {
                    reviveGauges[enabledGaugeCount + 1 < reviveGauges.Length ? enabledGaugeCount++ : enabledGaugeCount].Play();
                }
            }
        }
    }
}
