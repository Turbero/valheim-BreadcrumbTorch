using BepInEx.Logging;
using UnityEngine;

namespace BreadcrumbTorch
{
    internal class Logger
    {
        public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(BreadcrumbTorch.NAME);
        internal static void Log(object s)
        {
            if (!ConfigurationFile.debug.Value)
            {
                return;
            }

            logger.LogInfo(s?.ToString());
        }

        internal static void LogInfo(object s)
        {
            logger.LogInfo(s?.ToString());
        }

        internal static void LogWarning(object s)
        {
            var toPrint = $"{BreadcrumbTorch.NAME} {BreadcrumbTorch.VERSION}: {(s != null ? s.ToString() : "null")}";

            logger.LogWarning(toPrint);
        }

        internal static void LogError(object s)
        {
            var toPrint = $"{BreadcrumbTorch.NAME} {BreadcrumbTorch.VERSION}: {(s != null ? s.ToString() : "null")}";

            logger.LogError(toPrint);
        }
    }
}
