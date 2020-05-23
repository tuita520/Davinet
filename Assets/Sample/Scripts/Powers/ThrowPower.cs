using Davinet;
using UnityEngine;

public class ThrowPower : MonoBehaviour
{
    [SerializeField]
    float heightOffset = 2;

    [SerializeField]
    float throwVelocity = 10;

    [SerializeField]
    GameObject grabSphere;

    private StateObjectReference heldObject { get; set; }

    private PlayerInputController playerInputController;

    private void Awake()
    {
        heldObject = new StateObjectReference();
        heldObject.OnChanged += HeldObject_OnChanged;

        grabSphere.transform.parent = null;

        grabSphere.SetActive(false);

        playerInputController = GetComponent<PlayerInputController>();
    }

    private void OnEnable()
    {
        grabSphere.transform.position = transform.position + Vector3.up * heightOffset;
        grabSphere.SetActive(true);
    }

    private void OnDisable()
    {
        grabSphere.SetActive(false);
    }

    private void HeldObject_OnChanged(StatefulObject current, StatefulObject previous)
    {
        if (current != null)
        {
            current.GetComponent<Rigidbody>().isKinematic = true;
            StatefulWorld.Instance.SetOwnership(current.Ownable, GetComponent<OwnableObject>().Owner);
        }

        if (previous != null)
        {
            previous.GetComponent<Rigidbody>().isKinematic = false;
            StatefulWorld.Instance.RelinquishOwnership(previous.Ownable);
        }
    }

    private void FixedUpdate()
    {
        Vector3 grabPosition = transform.position + Vector3.up * heightOffset;
        grabSphere.transform.position = grabPosition;

        if (heldObject.Value != null)
        {
            heldObject.Value.GetComponent<Rigidbody>().position = grabPosition;
        }

        if (playerInputController.CurrentInput.usePowerDown)
            Use(playerInputController.CurrentInput.mouseRay);
    }

    private void Use(Ray ray)
    {
        if (heldObject.Value != null)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            float d;
            plane.Raycast(ray, out d);

            if (d > 0)
            {
                Vector3 target = ray.origin + ray.direction * d;
                Vector3 to = target - transform.position;

                heldObject.Value.GetComponent<Rigidbody>().velocity = to * throwVelocity;
                heldObject.Value = null;
            }
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                StatefulObject stateful = hit.collider.GetComponentInParent<StatefulObject>();

                if (stateful != null && stateful != GetComponent<StatefulObject>() && stateful.Ownable.CanClaim(GetComponent<OwnableObject>().Owner)) 
                    heldObject.Value = stateful;
            }
        }
    }
}
