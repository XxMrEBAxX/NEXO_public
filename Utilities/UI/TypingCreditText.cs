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
    public class TypingCreditText : MonoBehaviour
    {
        private const string FINISH_TYPING = "Our Special Player | \n \t\t\t\t You. \n\n\n Thanks for playing! \n";
        
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private float typingEachDuration = 0.05f;
        [SerializeField] private float typingEnterDuration = 0.5f;
        [SerializeField] private float typingUnderlineDuration = 0.5f;
        
        [SerializeField] private float onAfterTypingEndDelay = 3f;
        [SerializeField] private float deleteEachDuration = 0.001f;

        private RectTransform maskRect;
        private TMP_TextInfo textInfo;
        private StringBuilder typingStack = new StringBuilder();
        
        private string oriText;
        private bool underScore = false;
        private float originPosY = 0;

        public bool TypingCompleted { get; private set; } = false;
        public bool EraseStart { get; private set; } = false;

        private void Awake()
        {
            if (!textComponent)
                textComponent = gameObject.GetComponent<TMP_Text>();
            
            maskRect = textComponent.transform.parent.GetComponent<RectTransform>();

            oriText = textComponent.text;
            originPosY = textComponent.rectTransform.anchoredPosition.y;
        }

        private void Start()
        {
            TypingText().Forget();
        }
        
        public void SetActive(bool active)
        {
            if(active)
            {
                textComponent.rectTransform.anchoredPosition = new Vector2(textComponent.rectTransform.anchoredPosition.x, originPosY);
            }
            else
            {
                textComponent.rectTransform.anchoredPosition = new Vector2(textComponent.rectTransform.anchoredPosition.x, 10000);
            }
            textComponent.text = "";
        }

        private async UniTaskVoid TypingText()
        {
            TypingCompleted = false;
            textInfo = textComponent.textInfo;
            UnderScore().Forget();
            typingStack.Clear(); 
            for(int i = 0; i < oriText.Length; i++)
            {
                typingStack.Append(oriText[i]);
                if (oriText[i] == '\n')
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(typingEnterDuration), DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(typingEachDuration), DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                }
                textComponent.text = typingStack.ToString();
                
                textComponent.ForceMeshUpdate();
                
                TMP_CharacterInfo charInfo = textInfo.characterInfo[textInfo.characterCount - 1];
                if ((charInfo.bottomRight.y * -1) - textComponent.rectTransform.anchoredPosition.y > maskRect.rect.height)
                {
                    textComponent.rectTransform.anchoredPosition += new Vector2(0, charInfo.topRight.y - charInfo.bottomRight.y);
                }
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(onAfterTypingEndDelay), DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            
            for(int i = oriText.Length - 1; i >= 0; i--)
            {
                typingStack.Remove(i, 1);
                textComponent.text = typingStack.ToString();
                
                await UniTask.Delay(TimeSpan.FromSeconds(deleteEachDuration), DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                
                TMP_CharacterInfo charInfo = textInfo.characterInfo[Mathf.Max(0, textInfo.characterCount - 1)];
                if ((charInfo.bottomRight.y * -1) - textComponent.rectTransform.anchoredPosition.y < maskRect.rect.height)
                {
                    textComponent.rectTransform.anchoredPosition = Vector2.Max(new Vector2(0, 0), textComponent.rectTransform.anchoredPosition - new Vector2(0, charInfo.topRight.y - charInfo.bottomRight.y));
                }
            }
            
            EraseStart = true;
            for(int i = 0; i < FINISH_TYPING.Length; i++)
            {
                typingStack.Append(FINISH_TYPING[i]);
                if (FINISH_TYPING[i] == '\n')
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(typingEnterDuration), DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(typingEachDuration), DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                }
                textComponent.text = typingStack.ToString();
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(onAfterTypingEndDelay), DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            TypingCompleted = true;
        }

        private async UniTask UnderScore()
        {
            float elapsedTime = 0;
            string text;
            while (!TypingCompleted)
            {
                elapsedTime = 0;
                underScore = !underScore;
                while(elapsedTime < typingUnderlineDuration)
                {
                    elapsedTime += Time.deltaTime;

                    if (textComponent.text.Length > 0)
                    {
                        if (underScore && textComponent.text[textComponent.text.Length - 1] != '_')
                        {
                            text = textComponent.text + "_";
                            textComponent.text = text;
                        }
                        else if (!underScore && textComponent.text[textComponent.text.Length - 1] == '_')
                        {
                            text = textComponent.text.Remove(textComponent.text.Length - 1);
                            textComponent.text = text;
                        }
                    }

                    await UniTask.Yield();   
                }
                await UniTask.Yield();
            }
        }
    }
}

