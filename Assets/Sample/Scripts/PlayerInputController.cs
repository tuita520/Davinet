using Davinet;
using LiteNetLib.Utils;
using UnityEngine;

public class PlayerInputController : MonoBehaviour, IInputController
{
    [SerializeField]
    float maxSpeed = 5;

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
    }

    public void Read(NetDataReader reader)
    {
        moveInput = reader.GetVector3();
    }

    public void Write(NetDataWriter writer)
    {
        writer.Put(moveInput);
    }

    public bool ShouldWrite()
    {
        return true;
    }

    public void Clear(NetDataReader reader)
    {
        throw new System.NotImplementedException();
    }

    public void SetEnabled(bool value)
    {
        enabled = value;
    }
}
