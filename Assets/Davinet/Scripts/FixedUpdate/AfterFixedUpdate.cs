using UnityEngine;

[DefaultExecutionOrder(100)]
public class AfterFixedUpdate : MonoBehaviour
{
    public event System.Action OnAfterFixedUpdate;

    private void FixedUpdate()
    {
        OnAfterFixedUpdate?.Invoke();
    }
}