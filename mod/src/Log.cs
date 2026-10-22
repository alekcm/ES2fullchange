using BepInEx.Logging;

namespace ES2.HackingModes
{
    /// <summary>Лог мода. Пишется в консоль BepInEx и в LogOutput.log.</summary>
    internal static class Log
    {
        internal static ManualLogSource Source;

        internal static void Info(string message)
        {
            if (Source != null)
            {
                Source.LogInfo(message);
            }
        }

        internal static void Warn(string message)
        {
            if (Source != null)
            {
                Source.LogWarning(message);
            }
        }

        internal static void Error(string message)
        {
            if (Source != null)
            {
                Source.LogError(message);
            }
        }

        internal static void Debug(string message)
        {
            if (Source != null)
            {
                Source.LogDebug(message);
            }
        }
    }
}
