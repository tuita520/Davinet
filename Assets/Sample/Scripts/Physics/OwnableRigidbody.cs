using Davinet;
using UnityEngine;

public class OwnableRigidbody : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }    

    private void FixedUpdate()
    {
        // TODO: This should only get called on the frame when the rigidbody is asleep, rather than every frame.
        if (rb.IsSleeping())
        {
            GetComponent<OwnableObject>().RelinquishAuthority();
        }
    }
}
