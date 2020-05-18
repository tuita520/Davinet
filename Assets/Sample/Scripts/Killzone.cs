using UnityEngine;

public class Killzone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        StatefulGameObject statefulGameObject = other.GetComponentInParent<StatefulGameObject>();

        // TODO: Ideally check for a IDestructible interface or something.
        if (statefulGameObject != null && !other.GetComponentInParent<PlayerInputController>())
        {
            statefulGameObject.Alive.Value = false;
        }
    }
}
