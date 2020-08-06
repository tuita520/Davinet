using System;

namespace Davinet
{
    [Flags]
    public enum LogLevel
    {
        None = 0,
        State = 1,
        Property = 2,
        Authority = 4,
        Ownership = 8,
        Spawn = 16
    }

    public static class Debug
    {
        private static ILogger logger;
        private static LogLevel logLevel;

        public static void RegisterLogger(ILogger logger, LogLevel logLevel)
        {
            Debug.logger = logger;
            Debug.logLevel = logLevel;
        }

        public static void Log(string message, int objectID, LogLevel logLevel)
        {
            if (logger != null && (Debug.logLevel & logLevel) != LogLevel.None)
                logger.Log(message, StatefulWorld.Instance.Frame, objectID, logLevel);
        }
    }
}
