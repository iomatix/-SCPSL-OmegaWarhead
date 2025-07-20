namespace OmegaWarhead.Core.LoggingUtils
{
    using Exiled.API.Features;

    /// <summary>
    /// Provides centralized and prefixed logging utilities for the OmegaWarhead plugin.
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// Logs a debug-level message prefixed with <c>[OmegaWarhead]</c>, only if debugging is enabled in the plugin config.
        /// </summary>
        /// <param name="message">The message to log for debugging purposes.</param>
        public static void Debug(string message)
        {
            if (Plugin.Singleton.Config.Debug)
                Log.Debug($"{message}");
        }

        /// <summary>
        /// Logs an informational message prefixed with <c>[OmegaWarhead]</c>.
        /// </summary>
        /// <param name="message">The message to log as informational output.</param>
        public static void Info(string message)
        {
            Log.Info($"{message}");
        }

        /// <summary>
        /// Logs a warning message prefixed with <c>[OmegaWarhead]</c>.
        /// </summary>
        /// <param name="message">The message to log as a warning.</param>
        public static void Warning(string message)
        {
            Log.Warn($"{message}");
        }

        /// <summary>
        /// Logs an error message prefixed with <c>[OmegaWarhead]</c>.
        /// </summary>
        /// <param name="message">The message to log as an error.</param>
        public static void Error(string message)
        {
            Log.Error($"{message}");
        }
    }
}
