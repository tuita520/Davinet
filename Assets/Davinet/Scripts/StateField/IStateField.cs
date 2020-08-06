using Davinet;

public interface IStateField : IStreamable, IAuthorityField
{
    bool IsDirty { get; set; }
}

