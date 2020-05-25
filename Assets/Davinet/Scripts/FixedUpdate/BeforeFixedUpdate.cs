using UnityEngine;

[DefaultExecutionOrder(-100)]
public class BeforeFixedUpdate : MonoBehaviour
{
    public event System.Action OnBeforeFixedUpdate;

    private void FixedUpdate()
    {
        OnBeforeFixedUpdate?.Invoke();
    }
}
