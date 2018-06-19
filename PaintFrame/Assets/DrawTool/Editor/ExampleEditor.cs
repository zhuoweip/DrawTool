using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Example))]
public class ExampleEditor : Editor
{

    private SerializedProperty text;

    private void OnEnable()
    {
        text = serializedObject.FindProperty("Text");
    }

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls();

        EditorGUILayout.LabelField("Text");

        EditorGUILayout.HelpBox("请直接删除该对象", MessageType.Info);
        var newValue = EditorGUILayout.TextArea(text.stringValue, GUILayout.ExpandWidth(true), GUILayout.Height(300));
        if (newValue != text.stringValue) text.stringValue = newValue;

        serializedObject.ApplyModifiedProperties();
    }

}
