using Davinet;
using LiteNetLib.Utils;

public abstract class StateField<T> : IStateField
{
    /// <summary>
    /// Parameters are the new value and previous value.
    /// </summary>
    public event System.Action<T, T> OnChanged;

    /// <summary>
    /// Initializes with empty data and <see cref="IsDirty"/> set false.
    /// StateFields are writable by default.
    /// </summary>
    public StateField()
    {
        writable = true;
    }

    /// <summary>
    /// Initializes this field with a default value and sets it dirty.
    /// </summary>
    /// <param name="value"></param>
    public StateField(T value) : this()
    {
        IsDirty = true;

        this.value = value;
    }

    /// <summary>
    /// Initializes this field with a default value, registers an event receiver to 
    /// <see cref="OnChanged"/>, sets it dirty. Event receiver will immediately fire.
    /// </summary>
    /// <param name="value"></param>
    public StateField(T value, System.Action<T, T> eventReceiver) : this()
    {
        IsDirty = true;

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
            if (!writable)
                return;

            Set(value);
        }
    }

    protected void Set(T value)
    {
        T previous = this.value;
        this.value = value;
        IsDirty = true;

        OnChanged?.Invoke(this.value, previous);
    }

    public bool IsDirty { get; set; }

    private T value;
    private bool writable;

    public abstract void Write(NetDataWriter writer);
    public abstract void Read(NetDataReader reader);
    public abstract void Pass(NetDataReader reader);

    public int LastReadFrame { get; set; } = -1;

    public void SetWritable(bool value)
    {
        writable = value;
    }
}
