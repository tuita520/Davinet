using UnityEngine;

namespace Davinet.Sample
{
    public class RigidbodyDrag : MonoBehaviour
    {
        [SerializeField]
        float speed = 3;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            rb.AddForce(GetComponent<PlayerInputController>().CurrentInput.moveInput * speed);
        }
    }
}
