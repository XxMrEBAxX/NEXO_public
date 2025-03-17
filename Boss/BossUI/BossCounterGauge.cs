using System;
using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class BossCounterGauge : MonoBehaviour
    {
        [SerializeField] private float counterFillTime; // 카운터 게이지가 차는 시간 
        [SerializeField] private Image counterGauge;
        private Time time;
        private float fillSpeed;
        private float curCounterGaugeValue;

        private void Awake()
        {
            counterGauge = counterGauge.GetComponent<Image>();
        }

        private void Start()
        {
            Init();
        }
        
        public void Init()
        {
            counterGauge.fillAmount = 0;
            fillSpeed = 1f / counterFillTime;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AddCounterGauge(10);
            }
            
            SetCounterGauge();
        }

        /// <summary>
        /// 카운터 게이지를 증가 설정합니다
        /// </summary>
        private void SetCounterGauge()
        {
            curCounterGaugeValue += fillSpeed * Time.deltaTime;
            curCounterGaugeValue = Mathf.Clamp(curCounterGaugeValue, 0, 1);
            counterGauge.fillAmount = curCounterGaugeValue;
            
            if (curCounterGaugeValue >= 1)
            {
                // 이곳에 패턴을 실행하는 코드가 필요
                curCounterGaugeValue = 0;
            }
        }
        
        /// <summary>
        /// 카운터 게이지를 추가합니다.
        /// </summary>
        public void AddCounterGauge(float time)
        {
            curCounterGaugeValue += fillSpeed * time;
        }
    }
}
