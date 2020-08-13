namespace Davinet
{
    public interface ILogger
    {
        void Log(string message, LogType logType);
        void Log(string message, int frame, int objectID, LogType logType);
        void LogError(string message);
        void Assert(bool condition, string errorMessage);
    }
}
