using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;

namespace BirdCase
{
    public class JumpingText : MonoBehaviour
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private AnimationCurve jumpCurve;
        [SerializeField] private float jumpHeight = 10f;
        [SerializeField] private float eachJumpDuration = 1f;

        private TMP_TextInfo textInfo;

        private void Awake()
        {
            if (!textComponent)
                textComponent = gameObject.GetComponent<TMP_Text>();
        }

        private void Start()
        {
            JumpTextStart().Forget();
        }

        private async UniTaskVoid JumpTextStart()
        {
            textInfo = textComponent.textInfo;

            while (gameObject.activeInHierarchy)
            {
                textComponent.ForceMeshUpdate();

                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                    if (!charInfo.isVisible)
                        continue;

                    JumpText(charInfo).Forget();
                    await UniTask.Delay(TimeSpan.FromSeconds(eachJumpDuration), DelayType.UnscaledDeltaTime,
                        PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                }

                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
        }

        private async UniTaskVoid JumpText(TMP_CharacterInfo charInfo)
        {
            float elapsedTime = 0;
            Vector3[] verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            TMP_MeshInfo meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
            Vector3[] origVerts = new Vector3[4];
            for (int j = 0; j < 4; j++)
            {
                origVerts[j] = verts[charInfo.vertexIndex + j];
            }

            while (elapsedTime < eachJumpDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                for (int j = 0; j < 4; j++)
                {
                    int index = charInfo.vertexIndex + j;
                    meshInfo.vertices[index] = new Vector3(
                        origVerts[j].x,
                        origVerts[j].y + jumpCurve.Evaluate((elapsedTime / eachJumpDuration) * 2) * jumpHeight,
                        origVerts[j].z);
                }

                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    meshInfo.mesh.vertices = meshInfo.vertices;
                    textComponent.UpdateGeometry(meshInfo.mesh, i);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
            
            for (int j = 0; j < 4; j++)
            {
                int index = charInfo.vertexIndex + j;
                meshInfo.vertices[index] = origVerts[j];
            }
            
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                meshInfo.mesh.vertices = meshInfo.vertices;
                textComponent.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}

