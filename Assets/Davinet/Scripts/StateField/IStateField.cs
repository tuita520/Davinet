using Davinet;

public interface IStateField : IStreamable
{
    bool IsDirty { get; set; }
}

