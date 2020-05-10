using UnityEngine;

public class ForceTrigger : MonoBehaviour
{
    [SerializeField]
    float force = 10;

    [SerializeField]
    float radius = 4;

    [SerializeField]
    float upwardsModifier = 1;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody targetRb = other.GetComponent<Rigidbody>();

        if (targetRb != null)
            targetRb.AddExplosionForce(force, rb.position, radius, upwardsModifier);
    }
}
