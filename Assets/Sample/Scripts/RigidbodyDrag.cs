using UnityEngine;

namespace Davigo.Networking
{
    public class RigidbodyDrag : MonoBehaviour
    {
        [SerializeField]
        float speed = 5;

        [SerializeField]
        float maxSpeed = 5;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                Plane plane = new Plane(Vector3.up, Vector3.zero);

                float d;
                plane.Raycast(ray, out d);

                if (d > 0)
                {
                    Vector3 target = ray.origin + ray.direction * d;

                    Vector3 to = Vector3.ClampMagnitude(target - transform.position, maxSpeed);

                    rb.AddForce(to * speed);
                }
            }
        }
    }
}