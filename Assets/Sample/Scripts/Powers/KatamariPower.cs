using UnityEngine;
using System.Collections.Generic;
using Davinet;

public class KatamariPower : MonoBehaviour
{
    /*
    [SerializeField]
    float launchForce = 10;

    private StateList<StateObjectReference> stuckRigidbodies;

    private void Awake()
    {
        stuckRigidbodies = new StateList<StateObjectReference>();

        stuckRigidbodies.OnAdd += StuckRigidbodies_OnAdd;
    }

    private void StuckRigidbodies_OnAdd(StateObjectReference obj)
    {
        Rigidbody rb = obj.Value.GetComponent<Rigidbody>();

        rb.transform.parent = transform;
        rb.isKinematic = true;

        foreach (Collider collider in GetComponentsInChildren<Collider>(true))
        {
            Physics.IgnoreCollision(collider, rb.GetComponentInChildren<Collider>());
        }
    }

    private void OnDisable()
    {
        stuckRigidbodies.Clear();

        foreach (Rigidbody rb in stuckRigidbodies)
        {
            rb.transform.parent = null;
            rb.isKinematic = false;

            Vector3 to = (rb.position - GetComponent<Rigidbody>().position).normalized;

            rb.AddForce(to * launchForce, ForceMode.Impulse);

            foreach (Collider collider in GetComponentsInChildren<Collider>(true))
            {
                Physics.IgnoreCollision(collider, rb.GetComponentInChildren<Collider>(), false);
            }
        }

        stuckRigidbodies.Clear();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!enabled)
            return;

        Rigidbody rb = collision.gameObject.GetComponentInParent<Rigidbody>();

        if (rb != null && !stuckRigidbodies.Contains(rb.GetComponent<StatefulObject>()))
        {
            stuckRigidbodies.Add(new StateObjectReference(rb.GetComponent<StatefulObject>()));
        }
    }
    */
}
