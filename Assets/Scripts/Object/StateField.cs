public class StateField<T>
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

            OnChanged(this.value);
        }
    }

    private T value;
}
