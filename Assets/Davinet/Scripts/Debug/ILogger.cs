namespace Davinet
{
    public interface ILogger
    {
        void Log(string message, LogType logType);
        void Log(string message, int frame, int objectID, LogType logType);
    }
}
