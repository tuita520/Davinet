using UnityEngine;

public class PlayerInputController : MonoBehaviour, IInputController
{
    [SerializeField]
    float maxSpeed = 5;

    public bool TransformPressed { get; private set; }
    private Vector3 moveInput;

    private void Update()
    {
        moveInput = Vector3.zero;

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Plane plane = new Plane(Vector3.up, Vector3.zero);

            float d;
            plane.Raycast(ray, out d);

            if (d > 0)
            {
                Vector3 target = ray.origin + ray.direction * d;

                moveInput = Vector3.ClampMagnitude(target - transform.position, maxSpeed);
            }
        }

        GetComponent<RigidbodyDrag>().MoveInput = moveInput;

        if (Input.GetKeyDown(KeyCode.T))
        {
            GetComponent<CubeSphereTransformer>().Transform();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GetComponent<PowerController>().SetPowerActive(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GetComponent<PowerController>().SetPowerActive(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GetComponent<PowerController>().SetPowerActive(2);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            GetComponent<ThrowPower>().Use(ray);
        }
    }

    public void SetEnabled(bool value)
    {
        enabled = value;

        if (enabled)
        {
            FindObjectOfType<SmoothFollowCamera>().target = transform;
            FindObjectOfType<SmoothFollowCamera>().enabled = true;
        }
    }
}
