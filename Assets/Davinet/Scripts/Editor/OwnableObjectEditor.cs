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

        // If the Owner property is null, then the object has not been initialized.
        if (ownable.Owner == null)
            return;

        GUILayout.Label($"Owner: {ownable.Owner.Value}");
        GUILayout.Label($"Local Authority: {ownable.Authority.Value}");
        GUILayout.Label($"Unacknowledged Authority: {ownable.EffectiveAuthority}");
    }
}
