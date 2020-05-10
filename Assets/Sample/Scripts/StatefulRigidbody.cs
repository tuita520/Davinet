using Davinet;
using LiteNetLib.Utils;
using UnityEngine;

public class StatefulRigidbody : MonoBehaviour, IStateful
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Read(NetDataReader reader)
    {
        rb.position = reader.GetVector3();
        rb.rotation = reader.GetQuaternion();
        rb.velocity = reader.GetVector3();
        rb.angularVelocity = reader.GetVector3();
    }

    public void Write(NetDataWriter writer)
    {
        writer.Put(rb.position);
        writer.Put(rb.rotation);
        writer.Put(rb.velocity);
        writer.Put(rb.angularVelocity);
    }

    public bool ShouldWrite()
    {
        return !rb.IsSleeping();
    }
}
