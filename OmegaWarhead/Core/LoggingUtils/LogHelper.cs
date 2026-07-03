namespace OmegaWarhead.Core.LoggingUtils
{
    using LabApi.Features.Console;

    /// <summary>
    /// Provides a highly optimized, production-grade centralized logging matrix for the OmegaWarhead ecosystem,
    /// natively bridging configuration diagnostics with the LabAPI console engine.
    /// </summary>
    public static class LogHelper
    {
        #region Debug Diagnostic Vector
        /// <summary>
        /// Logs a debug-level execution trace if diagnostic telemetry is explicitly enabled in the configuration state.
        /// </summary>
        /// <param name="message">The diagnostic execution trace message.</param>
        public static void Debug(string message)
        {
            // Defensive structural guard against early lifecycle execution anomalies
            if (Plugin.Singleton?.Config?.Debug == true)
            {
                Logger.Debug($"[DEBUG] {message}");
            }
        }

        /// <summary>
        /// Logs a debug-level execution trace wrapped inside an explicit contextual subsystem source tag.
        /// </summary>
        /// <param name="source">The operational subsystem domain identifier originating the trace.</param>
        /// <param name="message">The diagnostic execution trace message.</param>
        public static void Debug(string source, string message)
        {
            if (Plugin.Singleton?.Config?.Debug == true)
            {
                Logger.Debug($"[{source}] [DEBUG] {message}");
            }
        }
        #endregion

        #region Informational Broadcasters
        /// <summary>
        /// Logs a standard informational runtime broadcast tracking asset operations.
        /// </summary>
        public static void Info(string message) => Logger.Info($"{message}");

        /// <summary>
        /// Logs an informational runtime broadcast wrapped inside a contextual execution domain tag.
        /// </summary>
        public static void Info(string source, string message) => Logger.Info($"[{source}] {message}");
        #endregion

        #region Warning Alert Core
        /// <summary>
        /// Logs a warning regarding non-fatal system anomalies or automated threshold corrections.
        /// </summary>
        public static void Warn(string message) => Logger.Warn($"{message}");

        /// <summary>
        /// Logs a warning regarding non-fatal system anomalies wrapped inside a contextual execution domain tag.
        /// </summary>
        public static void Warn(string source, string message) => Logger.Warn($"[{source}] {message}");

        /// <summary>
        /// Legacy compatibility alias mapping directly to the primary operational warning vector.
        /// </summary>
        public static void Warning(string message) => Logger.Warn($"{message}");

        /// <summary>
        /// Legacy compatibility alias mapping directly to the contextual warning vector.
        /// </summary>
        public static void Warning(string source, string message) => Logger.Warn($"[{source}] {message}");
        #endregion

        #region Exception and Error Handlers
        /// <summary>
        /// Logs a critical runtime failure or exceptional interruption that impacts system integrity.
        /// </summary>
        public static void Error(string message) => Logger.Error($"{message}");

        /// <summary>
        /// Logs a critical runtime failure wrapped inside a detailed contextual exception vector tracking matrix.
        /// </summary>
        public static void Error(string source, string message) => Logger.Error($"[{source}] {message}");
        #endregion
    }
}