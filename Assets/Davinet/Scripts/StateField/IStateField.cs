using Davinet;

public interface IStateField : IStreamable, IAuthorityControllable
{
    bool IsDirty { get; set; }
}

