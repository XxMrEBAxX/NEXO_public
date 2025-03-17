#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace BirdCase
{

[CustomEditor(typeof(MeshDemolisherTool))]
[CanEditMultipleObjects]
public class MeshDemolisherToolEditor : Editor
{
    private MeshDemolisherTool meshDemolisherTool;

    private SerializedProperty mesh;
    private SerializedProperty parent;
    private SerializedProperty maskObject;
    private SerializedProperty maskEase;
    private SerializedProperty dissolveMaterial;
    private SerializedProperty dustParticle;
    private SerializedProperty fillImage;
    private SerializedProperty sound;

    private void OnEnable()
    {
        meshDemolisherTool = (MeshDemolisherTool)target;

        mesh = serializedObject.FindProperty(nameof(meshDemolisherTool.mesh));
        parent = serializedObject.FindProperty(nameof(meshDemolisherTool.parentTransform));
        maskObject = serializedObject.FindProperty(nameof(meshDemolisherTool.maskObject));
        maskEase = serializedObject.FindProperty(nameof(meshDemolisherTool.maskEase));
        dissolveMaterial = serializedObject.FindProperty(nameof(meshDemolisherTool.dissolveMaterial));
        dustParticle = serializedObject.FindProperty(nameof(meshDemolisherTool.dustParticle));
        fillImage = serializedObject.FindProperty(nameof(meshDemolisherTool.fillImage));
        sound = serializedObject.FindProperty(nameof(meshDemolisherTool.demolishSound));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(mesh, new GUIContent("원본 메쉬"));
        EditorGUILayout.PropertyField(parent, new GUIContent("메쉬들의 부모 오브젝트"));
        EditorGUILayout.PropertyField(maskObject, new GUIContent("마스크 오브젝트"));
        EditorGUILayout.PropertyField(maskEase, new GUIContent("마스크 Ease 그래프"));
        EditorGUILayout.PropertyField(dissolveMaterial, new GUIContent("디졸브 머티리얼"));
        EditorGUILayout.PropertyField(dustParticle, new GUIContent("먼지 파티클"));
        EditorGUILayout.PropertyField(fillImage, new GUIContent("재생성 게이지"));
        EditorGUILayout.PropertyField(sound, new GUIContent("파괴 사운드"));
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("파괴"))
        {
            meshDemolisherTool.Demolish();
        }
        if(GUILayout.Button("초기화"))
        {
            meshDemolisherTool.Reset();
        }
        EditorGUILayout.EndHorizontal();
    }
}

}
#endif
