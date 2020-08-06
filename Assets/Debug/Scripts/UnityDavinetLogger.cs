namespace Davinet
{
    public class UnityDavinetLogger : ILogger
    {
        public void Log(string message, int frame, int objectID, LogLevel logLevel)
        {
            UnityEngine.Debug.Log($"{frame} ID: <b><color=blue>{objectID}</color></b> <b>{logLevel}</b>: {message}");
        }
    }
}