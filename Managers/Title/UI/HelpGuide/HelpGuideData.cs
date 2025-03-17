using System;
using UnityEngine;
using UnityEditor;
using MyBox;

namespace BirdCase
{
    [Serializable]
    public class HelpGuideSetting<TKey, TValue>
    {
        [OverrideLabel("보여줄 시간"), SerializeField]
        public TKey Key;
        [OverrideLabel("텍스트"), SerializeField]
        public TValue Value;
        
        public HelpGuideSetting(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
    
    [CreateAssetMenu(fileName = "HelpGuideData", menuName = "Datas/Localization/HelpGuideData")]
    public class HelpGuideData : ScriptableObject
    {
        [OverrideLabel("도움창 텍스트"), SerializeField]
        public HelpGuideSetting<float, string>[] HelpGuideTexts;
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(HelpGuideData))]
    public class HelpGuideDataEditor : Editor
    {
        private HelpGuideData helpGuideData;
        private SerializedProperty _helpGuideTexts;
        
        private void OnEnable()
        {
            helpGuideData = target as HelpGuideData;
            _helpGuideTexts = serializedObject.FindProperty("HelpGuideTexts");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_helpGuideTexts, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}
