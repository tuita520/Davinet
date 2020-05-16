using LiteNetLib.Utils;

public abstract class StateField<T> : IStateField
{
    public event System.Action<T> OnChanged;

    public StateField()
    {

    }

    public StateField(T value)
    {
        this.value = value;
    }

    public StateField(T value, System.Action<T> eventReceiver)
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
            this.value = value;
            IsDirty = true;

            OnChanged(this.value);
        }
    }

    public bool IsDirty { get; set; }

    private T value;

    public abstract void Write(NetDataWriter writer);
    public abstract void Read(NetDataReader reader);
}
