using LiteNetLib.Utils;

public class StateBool : StateField<bool>
{
    public StateBool() : base() { }
    public StateBool(bool value) : base(value) { }
    public StateBool(bool value, System.Action<bool> eventReceiver) : base(value, eventReceiver) { }

    public override void Read(NetDataReader reader)
    {
        Value = reader.GetBool();
    }

    public override void Write(NetDataWriter writer)
    {
        writer.Put(Value);
    }
}
