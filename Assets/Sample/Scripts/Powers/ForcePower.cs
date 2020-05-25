using Davinet;
using UnityEngine;

public class ForcePower : MonoBehaviour
{
    [SerializeField]
    float force = 10;

    [SerializeField]
    float radius = 4;

    [SerializeField]
    float upwardsModifier = 1;

    [SerializeField]
    GameObject art;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        art.SetActive(false);
    }

    private void OnEnable()
    {
        art.SetActive(true);
    }

    private void OnDisable()
    {
        art.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!enabled)
            return;

        Rigidbody targetRb = other.GetComponentInParent<Rigidbody>();

        if (targetRb != null)
        {
            targetRb.AddExplosionForce(force, rb.position, radius, upwardsModifier);
            targetRb.GetComponent<StatefulObject>()?.Ownable.TakeAuthority(GetComponent<StatefulObject>().Ownable.Owner.Value);
        }
    }
}
