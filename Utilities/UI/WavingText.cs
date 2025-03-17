using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BirdCase
{
    public class WavingText : MonoBehaviour
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private Gradient gradient;

        private void Awake()
        {
            if (!textComponent)
                textComponent = gameObject.GetComponent<TMP_Text>();
        }

        private void Update()
        {
            textComponent.ForceMeshUpdate();
            TMP_TextInfo textInfo = textComponent.textInfo;

            for(int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible)
                    continue;

                Vector3[] verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                TMP_MeshInfo meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];

                for(int j = 0; j < 4; j++)
                {
                    int index = charInfo.vertexIndex + j;
                    Vector3 orig = verts[charInfo.vertexIndex + j];
                    meshInfo.vertices[index] = orig + new Vector3(0, Mathf.Sin(Time.time * 2f + orig.x * 0.01f) * 10f, 0);

                    Color32 color = gradient.Evaluate(Mathf.Abs(Mathf.Sin((Time.time + orig.x * 0.01f) * 0.3f)));
                    meshInfo.colors32[index] = color;
                }

            }

            for(int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                meshInfo.mesh.colors32 = meshInfo.colors32;
                textComponent.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}
