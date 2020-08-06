using LiteNetLib.Utils;
using UnityEngine;

public class StateColor : StateField<Color>
{
    public StateColor() : base() { }
    public StateColor(Color value) : base(value) { }
    public StateColor(Color value, System.Action<Color, Color> eventReceiver) : base(value, eventReceiver) { }

    public override void Pass(NetDataReader reader)
    {
        reader.GetFloat();
        reader.GetFloat();
        reader.GetFloat();
        reader.GetFloat();
    }

    public override void Read(NetDataReader reader)
    {
        Color color = new Color();

        color.r = reader.GetFloat();
        color.g = reader.GetFloat();
        color.b = reader.GetFloat();
        color.a = reader.GetFloat();

        Set(color);
    }

    public override void Write(NetDataWriter writer)
    {
        writer.Put(Value.r);
        writer.Put(Value.g);
        writer.Put(Value.b);
        writer.Put(Value.a);
    }
}
