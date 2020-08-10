using System;

namespace Davinet
{
    [Flags]
    public enum LogType
    {
        None = 0,
        State = 1,
        Property = 2,
        Authority = 4,
        Ownership = 8,
        Spawn = 16,
        Packet = 32,
        Connection = 64,
        JitterBuffer = 128
    }

    public static class Debug
    {
        private static ILogger logger;
        private static LogType logType;

        public static void RegisterLogger(ILogger logger, LogType logType)
        {
            Debug.logger = logger;
            Debug.logType = logType;
        }

        public static void Log(string message, LogType logType)
        {
            if (logger != null && (Debug.logType & logType) != LogType.None)
                logger.Log(message, logType);
        }

        public static void Log(string message, int objectID, LogType logType)
        {
            if (logger != null && (Debug.logType & logType) != LogType.None)
                logger.Log(message, StatefulWorld.Instance.Frame, objectID, logType);
        }
    }
}
