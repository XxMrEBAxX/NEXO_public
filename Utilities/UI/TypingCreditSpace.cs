using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using FMOD;
using UnityEngine;
using TMPro;
using Debug = UnityEngine.Debug;

namespace BirdCase
{
    public class TypingCreditSpace : MonoBehaviour
    {
        private const int MAX_PERIOD_COUNT = 3;
        
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private float typingEachDuration = 0.5f;
        
        private StringBuilder typingStack = new StringBuilder();

        private string oriText;
        private int periodCount = 0;
        
        private void Awake()
        {
            if (!textComponent)
                textComponent = gameObject.GetComponent<TMP_Text>();
        }

        private void Start()
        {
            oriText = textComponent.text;
            textComponent.text = "";
            TypingPeriod().Forget();
        }

        private async UniTaskVoid TypingPeriod()
        {
            typingStack.Clear();
            typingStack.Append(oriText);
            while (gameObject.activeInHierarchy)
            {
                typingStack.Append('.');
                periodCount++;
                textComponent.text = typingStack.ToString();
                
                if(periodCount >= MAX_PERIOD_COUNT)
                {
                    periodCount = 0;
                    typingStack.Remove(typingStack.Length - MAX_PERIOD_COUNT, MAX_PERIOD_COUNT);
                }
                
                await UniTask.Delay(TimeSpan.FromSeconds(typingEachDuration), DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
        }
    }
}

