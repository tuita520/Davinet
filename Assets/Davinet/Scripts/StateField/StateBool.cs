using LiteNetLib.Utils;

public class StateBool : StateField<bool>
{
    public StateBool() : base() { }
    public StateBool(bool value) : base(value) { }
    public StateBool(bool value, System.Action<bool, bool> eventReceiver) : base(value, eventReceiver) { }

    public override void Read(NetDataReader reader)
    {
        Set(reader.GetBool());
    }

    public override void Pass(NetDataReader reader)
    {
        reader.GetBool();
    }

    public override void Write(NetDataWriter writer)
    {
        writer.Put(Value);
    }
}
