using System.Collections.Generic;
using LiteNetLib.Utils;

public class StateList<T> : StateField<List<T>> where T : IStateField, new()
{
    public event System.Action<T> OnRemove;
    public event System.Action<T> OnAdd;

    public StateList() : base()
    {
        Value = new List<T>();
    }

    public StateList(System.Action<List<T>, List<T>> eventReceiver) : this()
    {
        OnChanged += eventReceiver;
    }

    public void Add(T element)
    {
        Value.Add(element);        
        OnAdd?.Invoke(element);

        IsDirty = true;
    }

    public void Remove(T element)
    {
        Value.Remove(element);
        OnRemove?.Invoke(element);

        IsDirty = true;
    }

    public bool Contains(T element)
    {
        return Value.Contains(element);
    }

    public void Clear()
    {
        Value.Clear();

        IsDirty = true;
    }

    public override void Pass(NetDataReader reader)
    {
        throw new System.NotImplementedException();
    }

    public override void Read(NetDataReader reader)
    {
        int count = reader.GetInt();

        Value.Clear();
    }

    public override void Write(NetDataWriter writer)
    {
        writer.Put(Value.Count);

        foreach (IStateField field in Value)
        {
            field.Write(writer);
        }
    }
}
