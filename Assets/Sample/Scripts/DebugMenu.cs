using Davinet;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    [SerializeField]
    Text frameText;

    private void FixedUpdate()
    {
        frameText.text = $"Frame: {StatefulWorld.Instance.Frame}";
    }
}
