using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IdentifiableObject))]
[CanEditMultipleObjects]
public class IdentifiableObjectEditor : Editor
{
    private SerializedProperty guidProperty;

    private void OnEnable()
    {
        guidProperty = serializedObject.FindProperty("guid");
    }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate new GUID"))
        {
            var random = new System.Random();
            guidProperty.intValue = random.Next();
        }

        GUILayout.Label($"GUID: {guidProperty.intValue}"); 
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
