using Davinet;
using LiteNetLib.Utils;
using UnityEngine;

public class StatefulRigidbody : MonoBehaviour, IStreamable
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Read(NetDataReader reader)
    {
        Vector3 position = reader.GetVector3();
        Quaternion rotation = reader.GetQuaternion();
        Vector3 velocity = reader.GetVector3();
        Vector3 angularVelocity = reader.GetVector3();

        // Check if the new updates are very similiar to the current position.
        // If they are, they will not be applied to allow this object to fall asleep.
        // TODO: A better way of handling this.
        float epsilon = 0.1f;

        if (!rb.position.PerComponentIsEqual(position, epsilon))
            rb.position = position;

        if (!rb.rotation.PerComponentIsEqual(rotation, epsilon))
            rb.rotation = rotation;

        if (!rb.velocity.PerComponentIsEqual(velocity, epsilon))
            rb.velocity = velocity;

        if (!rb.angularVelocity.PerComponentIsEqual(angularVelocity, epsilon))
            rb.angularVelocity = angularVelocity;
    }

    public void Write(NetDataWriter writer)
    {
        writer.Put(rb.position);
        writer.Put(rb.rotation);
        writer.Put(rb.velocity);
        writer.Put(rb.angularVelocity);
    }

    public void Pass(NetDataReader reader)
    {
        reader.GetVector3();
        reader.GetQuaternion();
        reader.GetVector3();
        reader.GetVector3();
    }

    public bool ShouldWrite()
    {
        return true;
    }
}
