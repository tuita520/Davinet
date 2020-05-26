using Davinet;
using UnityEngine;

public class OwnableRigidbody : MonoBehaviour
{
    private Rigidbody rb;
    private OwnableObject ownable;

    private bool isSleeping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ownable = GetComponent<OwnableObject>();
    }    

    private void FixedUpdate()
    {
        if (!isSleeping)
        {
            if (rb.IsSleeping())
            {
                ownable.RelinquishAuthority();
                isSleeping = true;
            }
        }
        else
        {
            if (!rb.IsSleeping())
            {
                isSleeping = false;
            }
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
