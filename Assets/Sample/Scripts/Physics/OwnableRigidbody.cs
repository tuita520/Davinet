using UnityEngine;

namespace Davinet.Sample
{
    public class OwnableRigidbody : MonoBehaviour
    {
        private Rigidbody rb;
        private OwnableObject ownable;

        public bool IsSleeping;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            ownable = GetComponent<OwnableObject>();

            rb.sleepThreshold = 0.01f;
        }

        private void FixedUpdate()
        {
            if (!IsSleeping)
            {
                if (rb.IsSleeping())
                {
                    ownable.RelinquishAuthority();
                    IsSleeping = true;
                }
            }
            else
            {
                if (!rb.IsSleeping())
                {
                    IsSleeping = false;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out OwnableObject targetOwnable))
            {
                targetOwnable.TryTakeAuthority(ownable.Authority.Value);
            }
        }
    }
}
