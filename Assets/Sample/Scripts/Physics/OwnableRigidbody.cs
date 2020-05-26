using Davinet;
using UnityEngine;

public class OwnableRigidbody : MonoBehaviour
{
    private Rigidbody rb;
    private OwnableObject ownable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ownable = GetComponent<OwnableObject>();
    }    

    private void FixedUpdate()
    {
        // TODO: This should only get called on the frame when the rigidbody is asleep, rather than every frame.
        if (rb.IsSleeping())
        {
            ownable.RelinquishAuthority();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out OwnableObject targetOwnable))
        {
            targetOwnable.TakeAuthority(ownable.Authority.Value);
        }
    }
}
