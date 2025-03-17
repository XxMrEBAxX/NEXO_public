using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;

namespace BirdCase
{
    public class TypingText : MonoBehaviour
    {
        [SerializeField] bool skipSpace = false;
        [SerializeField] private bool typingOnStart = false;
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private float typingEachDuration = 0.05f;

        private TMP_TextInfo textInfo;
        
        public bool TypingCompleted { get; private set; } = false;

        private void Awake()
        {
            if (!textComponent)
                textComponent = gameObject.GetComponent<TMP_Text>();
        }

        private void Start()
        {
            if (typingOnStart)
                Typing();
        }

        public void Typing()
        {
            TypingCompleted = false;
            TypingTextStart().Forget();
        }

        public void Clear()
        {
            TypingCompleted = true;
            textComponent.text = "";
            textComponent.ForceMeshUpdate();
        }

        private async UniTaskVoid TypingTextStart()
        {
            TypingCompleted = false;
            textComponent.ForceMeshUpdate();
            textInfo = textComponent.textInfo;
            
            for(int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible)
                    continue;

                TMP_MeshInfo meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];

                for(int j = 0; j < 4; j++)
                {
                    int index = charInfo.vertexIndex + j;
                    Color32 color = meshInfo.colors32[index];
                    meshInfo.colors32[index] = new Color32(color.r, color.g, color.b, 0);
                }
            }

            for(int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                meshInfo.mesh.colors32 = meshInfo.colors32;
                textComponent.UpdateGeometry(meshInfo.mesh, i);
            }
            
            for(int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible)
                    continue;

                if (TypingCompleted)
                    return;

                ShowText(charInfo).Forget();
                
                if(skipSpace && charInfo.character == ' ')
                    continue;
                
                await UniTask.Delay(TimeSpan.FromSeconds(typingEachDuration), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
            TypingCompleted = true;
        }
        
        private async UniTaskVoid ShowText(TMP_CharacterInfo charInfo)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
            
            for (int j = 0; j < 4; j++)
            {
                int index = charInfo.vertexIndex + j;
                Color32 color = meshInfo.colors32[index];
                meshInfo.colors32[index] = new Color32(color.r, color.g, color.b, 255);
            }

            for(int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.colors32 = meshInfo.colors32;
                textComponent.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}

