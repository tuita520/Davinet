using LiteNetLib.Utils;

public abstract class StateField<T> : IStateField
{
    /// <summary>
    /// Parameters are the new value and previous value.
    /// </summary>
    public event System.Action<T, T> OnChanged;

    public StateField()
    {
        IsDirty = true;
    }

    public StateField(T value) : this()
    {
        this.value = value;
    }

    public StateField(T value, System.Action<T, T> eventReceiver) : this()
    {
        OnChanged += eventReceiver;

        Value = value;
    }

    public T Value
    {
        get
        {
            return value;
        }

        set
        {
            OnChanged?.Invoke(value, this.value);

            this.value = value;
            IsDirty = true;            
        }
    }

    public bool IsDirty { get; set; }

    private T value;

    public abstract void Write(NetDataWriter writer);
    public abstract void Read(NetDataReader reader);
    public abstract void Pass(NetDataReader reader);
}
