namespace BetterOmegaWarhead.Core.LoggingUtils
{
    using Exiled.API.Features;

    /// <summary>
    /// Provides centralized and prefixed logging utilities for the BetterOmegaWarhead plugin.
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// Logs a debug-level message prefixed with <c>[BetterOmegaWarhead]</c>, only if debugging is enabled in the plugin config.
        /// </summary>
        /// <param name="message">The message to log for debugging purposes.</param>
        public static void Debug(string message)
        {
            if (Plugin.Singleton.Config.Debug)
                Log.Debug($"[BetterOmegaWarhead] {message}");
        }

        /// <summary>
        /// Logs an informational message prefixed with <c>[BetterOmegaWarhead]</c>.
        /// </summary>
        /// <param name="message">The message to log as informational output.</param>
        public static void Info(string message)
        {
            Log.Info($"[BetterOmegaWarhead] {message}");
        }

        /// <summary>
        /// Logs a warning message prefixed with <c>[BetterOmegaWarhead]</c>.
        /// </summary>
        /// <param name="message">The message to log as a warning.</param>
        public static void Warning(string message)
        {
            Log.Warn($"[BetterOmegaWarhead] {message}");
        }

        /// <summary>
        /// Logs an error message prefixed with <c>[BetterOmegaWarhead]</c>.
        /// </summary>
        /// <param name="message">The message to log as an error.</param>
        public static void Error(string message)
        {
            Log.Error($"[BetterOmegaWarhead] {message}");
        }
    }
}
