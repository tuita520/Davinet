using UnityEngine;

public class RigidbodyDrag : MonoBehaviour
{
    [SerializeField]
    float speed = 3;

    private Rigidbody rb;

    public Vector3 MoveInput { private get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.AddForce(MoveInput * speed);
    }
}
