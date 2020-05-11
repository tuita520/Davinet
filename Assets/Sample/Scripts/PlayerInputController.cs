using UnityEngine;
using Davinet;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField]
    float maxSpeed = 5;

    private Vector3 moveInput;

    public Vector3 GetInput()
    {
        return moveInput;
    }

    public void InjectInput(Vector3 input)
    {
        moveInput = input;

        GetComponent<RigidbodyDrag>().MoveInput = moveInput;
    }

    public void SetInputMode(bool injectOnly)
    {
        enabled = injectOnly;
    }

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
    }
}
