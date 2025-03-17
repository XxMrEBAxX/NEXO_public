using System;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace BirdCase
{
    [Serializable]
    public class KeyValuePair<TKey, TValue>
    {
        [OverrideLabel("보여줄 시간"), SerializeField] 
        public TKey Key;
        [OverrideLabel("텍스트"), SerializeField] 
        public TValue Value;
        public KeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
    
    [CreateAssetMenu(fileName = "TutorialFlowData", menuName = "Datas/Localization/TutorialFlowData")]
    public class TutorialFlowData : ScriptableObject
    {
        [OverrideLabel("튜토리얼 시작 텍스트")]
        public KeyValuePair<float, string>[] TutorialStartTexts;
        [OverrideLabel("튜토리얼 시작 텍스트")]
        public KeyValuePair<float, string>[] TutorialEndTexts;
        
        public TutorialData WalkTutorial;

        public TutorialData DefaultAttackTutorial;

        public TutorialData SpecialAttackTutorial;

        public TutorialData CounterTutorial;
        
        public TutorialData ShieldTutorial;
        
        public TutorialData ReviveTutorial;
    }
    
    [Serializable]
    public class TutorialData
    {
        [Header("튜토리얼 텍스트")]
        [OverrideLabel("두 플레이어의 텍스트 내용이 동일한지"), HideInInspector] 
        public bool isTextCommonness = true;
        
        [OverrideLabel("텍스트"), SerializeField] 
        public KeyValuePair<float, string>[] TutorialText;
        
        [OverrideLabel("리아 텍스트"), SerializeField] 
        public KeyValuePair<float, string>[] RiaTutorialText;
        [OverrideLabel("니아 텍스트"), SerializeField] 
        public KeyValuePair<float, string>[] NiaTutorialText;
        
        [Header("퀘스트 설명")]
        [OverrideLabel("두 플레이어의 퀘스트 내용이 동일한지"), HideInInspector] 
        public bool isExplanationCommonness = true;
        
        [OverrideLabel("퀘스트 설명 텍스트"), HideInInspector] 
        public string TutorialExplanationText;
        
        [OverrideLabel("리아 퀘스트 설명 텍스트"), HideInInspector] 
        public string RiaTutorialExplanationText;
        [OverrideLabel("니아 퀘스트 설명 텍스트"), HideInInspector] 
        public string NiaTutorialExplanationText;
        [Header("퀘스트 조건 (갯수 수정 X 중점이 되는 내용 바꾸지 마세요)")] 
        [OverrideLabel("두 플레이어의 퀘스트 조건이 동일한지"), HideInInspector]
        public bool isConditionsCommonness = true;
        
        [OverrideLabel("퀘스트 조건 텍스트"), HideInInspector] 
        public string[] ConditionsText;
        
        [OverrideLabel("리아 퀘스트 조건 텍스트"), HideInInspector]
        public string[] RiaConditionsText;
        [OverrideLabel("니아 퀘스트 조건 텍스트"), HideInInspector]
        public string[] NiaConditionsText;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TutorialFlowData))]
    public class TutorialDataEditor : Editor
    {
        private TutorialFlowData data;
        private void OnEnable()
        {
            data = (TutorialFlowData) target;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("<텍스트 : 그냥 대화창처럼 보여주는 텍스트 (갯수 수정해도됨, 0으로 둬도 됨)>", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("<퀘스트 : 퀘스트 관련 텍스트>", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("퀘스트 설명 : 윗쪽에 나오는 퀘스트 설명입니다", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("퀘스트 조건 : 퀘스트 조건을 나타냅니다 (갯수 수정 X 중점이 되는 내용 바꾸지 마세요)", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("기본적으로 리아, 니아 텍스트가 다르게 나오더라도 갯수는 같게 해주세요)", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Tutorial Start / End", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("튜토리얼 시작 텍스트", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TutorialStartTexts"));
            EditorGUILayout.LabelField("튜토리얼 끝 텍스트", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TutorialEndTexts"));
            EditorGUILayout.Space(50);
            
            Func(data.WalkTutorial, "WalkTutorial");
            Func(data.DefaultAttackTutorial, "DefaultAttackTutorial");
            Func(data.SpecialAttackTutorial, "SpecialAttackTutorial");
            Func(data.CounterTutorial, "CounterTutorial");
            Func(data.ShieldTutorial, "ShieldTutorial");
            Func(data.ReviveTutorial, "ReviveTutorial");
            
            serializedObject.ApplyModifiedProperties();
        }

        private void Func(TutorialData tutorialData, string dataName)
        {
            EditorGUILayout.LabelField(dataName + "---------------------", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".isTextCommonness"));
            
            if (tutorialData.isTextCommonness)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".TutorialText"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".RiaTutorialText"));
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".NiaTutorialText"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".isExplanationCommonness"));
            if (tutorialData.isExplanationCommonness)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".TutorialExplanationText"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".RiaTutorialExplanationText"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".NiaTutorialExplanationText"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".isConditionsCommonness"));
            if (tutorialData.isConditionsCommonness)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".ConditionsText"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".RiaConditionsText"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(dataName + ".NiaConditionsText"));
            }
            EditorGUILayout.Space(30);
        }
    }
#endif
}
