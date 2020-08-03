using Davinet;
using TMPro;
using UnityEngine;

public class StatefulObjectDebugView : MonoBehaviour
{
    [SerializeField]
    TextMeshPro text;

    private StatefulObject statefulObject;

    public void Initialize(StatefulObject statefulObject)
    {
        this.statefulObject = statefulObject;
    }

    private void LateUpdate()
    {
        transform.position = statefulObject.transform.position;
    }

    private void FixedUpdate()
    {
        text.text = $"ID: {statefulObject.ID}\n" +
            $"Owner: { statefulObject.Ownable.Owner.Value}\n" +
            $"Authority: {statefulObject.Ownable.Authority.Value}";
    }
}
