using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BirdCase
{
    public class ActiveCreditPanel : MonoBehaviour
    {
        [SerializeField]
        private float showDuration = 3f;
        
        private List<TypingLine[]> typingLines = new List<TypingLine[]>();

        private void Awake()
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                TypingLine[] typings = transform.GetChild(i).GetComponentsInChildren<TypingLine>();
                typingLines.Add(typings);
            }

            for (int i = 0; i < typingLines.Count; i++)
            {
                for (int j = 0; j < typingLines[i].Length; j++)
                {
                    typingLines[i][j].gameObject.SetActive(false);
                }
            }
        }

        private void Start()
        {
            TypingLines().Forget();
        }
        
        private async UniTaskVoid TypingLines()
        {
            for(int i = 0;  i < typingLines.Count - 1; i++)
            {
                for(int j = 0; j < typingLines[i].Length; j++)
                {
                    typingLines[i][j].gameObject.SetActive(true);
                    typingLines[i][j].Typing();
                    await UniTask.WaitUntil(() => typingLines[i][j].TypingCompleted, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                }
                await UniTask.Delay(TimeSpan.FromSeconds(showDuration), DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy()); 
                
                for(int j = 0; j < typingLines[i].Length; j++)
                {
                    typingLines[i][j].gameObject.SetActive(false);
                }
            }
        }

        public void SetSmile()
        {
            for(int i = 0; i < typingLines.Count; i++)
            {
                for(int j = 0; j < typingLines[i].Length; j++)
                {
                    typingLines[i][j].gameObject.SetActive(false);
                }
            }
            typingLines[typingLines.Count - 1][0].gameObject.SetActive(true);
            typingLines[typingLines.Count - 1][0].Typing();
        }
    }
}
