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
        GUILayout.Label($"GUID: {guidProperty.intValue}");
        
        if (GUILayout.Button("Generate GUID"))
        {
            var random = new System.Random();
            guidProperty.intValue = random.Next();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
