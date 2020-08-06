using LiteNetLib.Utils;
using UnityEngine;
using Davinet;

public class PlayerInput : IStateField
{
    public Vector3 moveInput;
    public bool transformDown;
    public bool usePowerDown;
    public int setPowerDown;
    public Ray mouseRay;

    public bool IsDirty { get; set; }

    public void Clear()
    {
        moveInput = Vector3.zero;
        transformDown = false;
        usePowerDown = false;
        setPowerDown = -1;
    }

    public void Pass(NetDataReader reader)
    {
        reader.GetVector3();
        //reader.GetBool();
        //reader.GetBool();
        //reader.GetInt();
        //reader.GetVector3();
        //reader.GetVector3();
    }

    public void Read(NetDataReader reader)
    {
        moveInput = reader.GetVector3();
        //transformDown = reader.GetBool();
        //usePowerDown = reader.GetBool();
        //setPowerDown = reader.GetInt();
        //mouseRay.origin = reader.GetVector3();
        //mouseRay.direction = reader.GetVector3();
    }

    public void SetWritable(bool value)
    {
        // Do nothing...for now!
    }

    public void Write(NetDataWriter writer)
    {
        writer.Put(moveInput);
        //writer.Put(transformDown);
        //writer.Put(usePowerDown);
        //writer.Put(setPowerDown);
        //writer.Put(mouseRay.origin);
        //writer.Put(mouseRay.direction);
    }
}
