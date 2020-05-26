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
        GUILayout.Label($"Authority: {ownable.Authority.Value}");
    }

    private void OnSceneGUI()
    {
        OwnableObject ownable = (OwnableObject)target;

        if (ownable.Owner == null)
            return;

        Handles.BeginGUI();
        Vector2 pos2D = HandleUtility.WorldToGUIPoint(ownable.transform.position);
        GUILayout.BeginArea(new Rect(pos2D, Vector2.one * 150));
        GUI.backgroundColor = new Color(1, 1, 1, 0.5f);
        GUILayout.Box($"Owner: { ownable.Owner.Value}\n" +
            $"Authority: {ownable.Authority.Value}");
        GUILayout.EndArea();
        Handles.EndGUI();        
    }
}
