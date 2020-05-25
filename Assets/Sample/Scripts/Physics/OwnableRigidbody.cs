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
        // TODO: This will get called every frame the rigidbody is asleep.
        if (rb.IsSleeping())
        {
            GetComponent<OwnableObject>().RelinquishAuthority();
        }
    }
}
