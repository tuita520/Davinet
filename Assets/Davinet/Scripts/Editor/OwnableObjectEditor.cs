using Davinet;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OwnableObject))]
[CanEditMultipleObjects]
public class OwnableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            OwnableObject ownable = (OwnableObject)serializedObject.targetObject;
            GUILayout.Label($"Owner: {ownable.Owner.Value}");
            GUILayout.Label($"Local Authority: {ownable.Authority.Value}");
            GUILayout.Label($"Unacknowledged Authority: {ownable.EffectiveAuthority}");
        }
    }
}
