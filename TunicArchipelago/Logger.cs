using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;

namespace TunicArchipelago
{
    internal class Logger
    {
        private static ManualLogSource Log = null;

        public static void SetLogger(ManualLogSource log)
        {
            Log = log;
        }

        public static void LogDebug(object data)
        {
            if (Log != null)
            {
                Log.LogDebug(data);
            }
        }

        public static void LogDebug(BepInExDebugLogInterpolatedStringHandler logHandler)
        {
            if (Log != null)
            {
                Log.LogDebug(logHandler);
            }
        }

        public static void LogInfo(object data)
        {
            if (Log != null)
            {
                Log.LogInfo(data);
            }
        }

        public static void LogInfo(BepInExInfoLogInterpolatedStringHandler logHandler)
        {
            if (Log != null)
            {
                Log.LogInfo(logHandler);
            }
        }

        public static void LogWarning(object data)
        {
            if (Log != null)
            {
                Log.LogWarning(data);
            }
        }

        public static void LogWarning(BepInExWarningLogInterpolatedStringHandler logHandler)
        {
            if (Log != null)
            {
                Log.LogWarning(logHandler);
            }
        }

        public static void LogError(object data)
        {
            if (Log != null)
            {
                Log.LogError(data);
            }
        }

        public static void LogError(BepInExErrorLogInterpolatedStringHandler logHandler)
        {
            if (Log != null)
            {
                Log.LogError(logHandler);
            }
        }

        public static void LogFatal(object data)
        {
            if (Log != null)
            {
                Log.LogFatal(data);
            }
        }

        public static void LogFatal(BepInExFatalLogInterpolatedStringHandler logHandler)
        {
            if (Log != null)
            {
                Log.LogFatal(logHandler);
            }
        }
    }
}
