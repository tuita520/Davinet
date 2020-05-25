using Davinet;
using LiteNetLib.Utils;

public class StateObjectReference : StateField<StatefulObject>
{
    public StateObjectReference() : base() { }
    public StateObjectReference(StatefulObject value) : base(value) { }
    public StateObjectReference(StatefulObject value, System.Action<StatefulObject, StatefulObject> eventReceiver) : base(value, eventReceiver) { }

    public override void Pass(NetDataReader reader)
    {
        reader.GetInt();
    }

    public override void Read(NetDataReader reader)
    {
        int id = reader.GetInt();

        if (id == 0)
            Value = null;
        else
            Value = StatefulWorld.Instance.GetStatefulObject(id);
    }

    public override void Write(NetDataWriter writer)
    {
        if (Value == null)
            writer.Put(0);
        else
            writer.Put(Value.ID);
    }
}
