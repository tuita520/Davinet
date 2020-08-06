namespace Davinet
{
    public interface ILogger
    {
        void Log(string message, int frame, int objectID, LogLevel logLevel);
    }
}
