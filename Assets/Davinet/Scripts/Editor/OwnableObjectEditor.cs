using Davinet;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OwnableObject))]
[CanEditMultipleObjects]
public class OwnableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        OwnableObject ownable = (OwnableObject)serializedObject.targetObject;
        GUILayout.Label($"Owner: {ownable.Owner}");
        GUILayout.Label($"Authority: {ownable.Authority}");
    }
}
