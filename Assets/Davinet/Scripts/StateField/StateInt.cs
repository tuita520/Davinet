using LiteNetLib.Utils;

public class StateInt : StateField<int>
{
    public StateInt() : base() { }
    public StateInt(int value) : base(value) { }
    public StateInt(int value, System.Action<int, int> eventReceiver) : base(value, eventReceiver) { }

    public override void Read(NetDataReader reader)
    {
        Value = reader.GetInt();
    }

    public override void Pass(NetDataReader reader)
    {
        reader.GetInt();
    }

    public override void Write(NetDataWriter writer)
    {
        writer.Put(Value);
    }
}
