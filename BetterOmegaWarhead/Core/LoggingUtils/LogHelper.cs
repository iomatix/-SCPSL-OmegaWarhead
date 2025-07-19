namespace BetterOmegaWarhead.Core.LoggingUtils
{
    using Exiled.API.Features;

    /// <summary>
    /// Centralized logging helper.
    /// </summary>
    public static class LogHelper
    {
        public static void Debug(string message)
        {
            if (Plugin.Singleton.Config.Debug) Log.Debug($"[BetterOmegaWarhead] {message}");
        }

        public static void Info(string message)
        {
            Log.Info($"[BetterOmegaWarhead] {message}");
        }

        public static void Warning(string message)
        {
            Log.Warn($"[BetterOmegaWarhead] {message}");
        }

        public static void Error(string message)
        {
            Log.Error($"[BetterOmegaWarhead] {message}");
        }
    }
}
