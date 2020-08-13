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
        JitterBuffer = 128,
        Event = 256
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

        public static void LogError(string message)
        {
            if (logger != null && Debug.logType != LogType.None)
                logger.LogError(message);
        }

        public static void Assert(bool condition, string message)
        {
            if (logger != null && Debug.logType != LogType.None)
                logger.Assert(condition, message);
        }
    }
}
