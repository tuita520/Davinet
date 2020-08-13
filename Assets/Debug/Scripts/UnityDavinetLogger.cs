namespace Davinet.UnityDebug
{
    public class UnityDavinetLogger : ILogger
    {
        public void Log(string message, LogType logLevel)
        {
            UnityEngine.Debug.Log($"<b>{logLevel}</b>: {message}");
        }

        public void Log(string message, int frame, int objectID, LogType logLevel)
        {
            UnityEngine.Debug.Log($"{frame} ID: <b><color=blue>{objectID}</color></b> <b>{logLevel}</b>: {message}");
        }

        public void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public void Assert(bool condition, string message)
        {
            if (!condition)
                UnityEngine.Debug.LogError(message);
        }
    }
}