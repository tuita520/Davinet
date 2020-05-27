namespace Davinet
{
    /// <summary>
    /// Object implementing this interface will have <see cref="SetEnabled(bool)"/> called
    /// with true passed in if the local client is controlling this object.
    /// </summary>
    public interface IInputController
    {
        void SetEnabled(bool value);
    }
}