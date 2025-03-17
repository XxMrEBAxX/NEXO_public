using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BirdCase
{
    public class FontManager : MonoBehaviour
    {
         public TMP_FontAsset FontAsset;
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(FontManager))]
    public class TMP_FontChangerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Change Font!"))
            {
                TMP_FontAsset fontAsset = ((FontManager)target).FontAsset;

                foreach(TextMeshPro textMeshPro3D in GameObject.FindObjectsOfType<TextMeshPro>(true)) 
                { 
                    textMeshPro3D.font = fontAsset;
                }
                foreach(TextMeshProUGUI textMeshProUi in GameObject.FindObjectsOfType<TextMeshProUGUI>(true)) 
                { 
                    textMeshProUi.font = fontAsset;
                }
            }
        }
    }
#endif
}

