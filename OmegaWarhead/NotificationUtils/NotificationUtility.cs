namespace OmegaWarhead.NotificationUtils
{
    using Cassie;
    using LabApi.Features.Wrappers;
    using System.Collections.Generic;

    /// <summary>
    /// Provides extension methods for dictionary operations.
    /// </summary>
    #region DictionaryExtensions Class
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Deconstructs a key-value pair into separate key and value components.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="kvp">The key-value pair to deconstruct.</param>
        /// <param name="key">The output key.</param>
        /// <param name="value">The output value.</param>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
    #endregion

    /// <summary>
    /// Provides utility methods for sending notifications and calculating message durations in the Omega Warhead system.
    /// </summary>
    #region NotificationUtility Class
    public static class NotificationUtility
    {
        #region Cassie Message Methods

        /// <summary>
        /// Sends a standard Cassie message with specified settings, optionally clearing previous messages.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="customSubtitles">Optional custom subtitles for the message.</param>
        public static void SendCassieMessage(string message, string customSubtitles = "")
        {
            ProcessAndDispatchMessage(
                message,
                customSubtitles,
                Plugin.Singleton.Config.CassieMessageClearBeforeWarheadMessage,
                "pitch_0.95",
                Plugin.Singleton.Config.CassieMessagePriority
            );
        }

        /// <summary>
        /// Sends an important Cassie message with adjusted pitch and higher priority, optionally clearing previous messages.
        /// </summary>
        /// <param name="message">The important message to send.</param>
        /// <param name="customSubtitles">Optional custom subtitles for the message.</param>
        public static void SendImportantCassieMessage(string message, string customSubtitles = "")
        {
            ProcessAndDispatchMessage(
                message,
                customSubtitles,
                Plugin.Singleton.Config.CassieMessageClearBeforeImportant,
                "pitch_1.05",
                Plugin.Singleton.Config.CassieMessageImportantPriority
            );
        }

        /// <summary>
        /// Core method responsible for validating, formatting, and dispatching Cassie messages.
        /// </summary>
        /// <param name="message">The base message to be processed.</param>
        /// <param name="customSubtitles">The subtitles to display alongside the message.</param>
        /// <param name="shouldClear">Indicates whether the Cassie queue should be cleared before sending.</param>
        /// <param name="pitchModifier">The pitch modification string (e.g., "pitch_0.95").</param>
        /// <param name="priority">The priority level of the message in the queue.</param>
        private static void ProcessAndDispatchMessage(string message, string customSubtitles, bool shouldClear, string pitchModifier, float priority)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (shouldClear) Announcer.Clear();

            string processedSubtitles = string.Empty;
            if (!string.IsNullOrEmpty(customSubtitles) && !Plugin.Singleton.Config.DisableCassieMessages)
            {
                processedSubtitles = customSubtitles;
            }

            string fullMessage = $"{pitchModifier} {message}";

            Announcer.Message(fullMessage, customSubtitles: processedSubtitles, priority: priority, playBackground: false);
        }

        #endregion

        #region Message Duration Calculations

        /// <summary>
        /// Calculates the duration of a Cassie message based on its content and speed.
        /// Uses the updated LabAPI CassiePlaybackModifiers.
        /// </summary>
        /// <param name="message">The message to calculate duration for.</param>
        /// <param name="speed">The playback speed multiplier (default is 0.95).</param>
        /// <returns>The precise duration of the message in seconds.</returns>
        public static double CalculateCassieMessageDuration(string message, double speed = 0.95)
        {
            CassiePlaybackModifiers modifiers = default;
            modifiers.Pitch = (float)speed;

            return Announcer.CalculateDuration(message, modifiers);
        }

        /// <summary>
        /// Calculates the total duration of a dictionary of messages with their respective speeds.
        /// </summary>
        /// <param name="messageSpeedDictionary">A dictionary mapping messages to their playback speeds.</param>
        /// <returns>The total precise duration of all messages in seconds.</returns>
        public static double CalculateTotalMessagesDurations(IDictionary<string, float> messageSpeedDictionary)
        {
            double totalDuration = 0d;
            foreach (var kvp in messageSpeedDictionary)
            {
                totalDuration += CalculateCassieMessageDuration(kvp.Key, kvp.Value);
            }
            return totalDuration;
        }

        /// <summary>
        /// Calculates the total duration of a collection of messages with a default speed.
        /// Supports both List<string> and array collections.
        /// </summary>
        /// <param name="messages">The collection of messages to calculate duration for.</param>
        /// <param name="defaultSpeed">The default speed for all messages (default is 1).</param>
        /// <returns>The total precise duration of all messages in seconds.</returns>
        public static double CalculateTotalMessagesDurations(IEnumerable<string> messages, float defaultSpeed = 1f)
        {
            double totalDuration = 0d;
            foreach (string message in messages)
            {
                totalDuration += CalculateCassieMessageDuration(message, defaultSpeed);
            }
            return totalDuration;
        }

        /// <summary>
        /// Calculates the total duration of parameters array of messages with a default speed.
        /// </summary>
        /// <param name="defaultSpeed">The default speed for all messages.</param>
        /// <param name="messages">The array of messages to calculate duration for.</param>
        /// <returns>The total precise duration of all messages in seconds.</returns>
        public static double CalculateTotalMessagesDurations(float defaultSpeed = 1f, params string[] messages)
        {
            return CalculateTotalMessagesDurations((IEnumerable<string>)messages, defaultSpeed);
        }

        #endregion

        #region Notification Message Generation
        /// <summary>
        /// Generates a Cassie notification message based on the remaining time until Omega Warhead detonation.
        /// </summary>
        /// <param name="notifyTime">The remaining time in seconds until detonation.</param>
        /// <returns>A formatted Cassie message for the countdown.</returns>
        public static string GetCassieCounterNotifyMessage(int notifyTime)
        {
            if (notifyTime < 5) return $".G3 {notifyTime} .G5";
            if (notifyTime >= 5 && notifyTime <= 20) return $".G3 {notifyTime.ToString()} Seconds .G5";
            return $".G3 {notifyTime.ToString()} Seconds until Omega Warhead Detonation .G5";
        }
        #endregion

        #region Broadcast Methods
        /// <summary>
        /// Broadcasts the Omega Warhead activation message to all ready players.
        /// </summary>
        public static void BroadcastOmegaActivation()
        {
            foreach (Player player in Player.ReadyList)
                player.SendHint(Plugin.Singleton.Config.ActivatedMessage, 6f);
        }

        /// <summary>
        /// Broadcasts the helicopter incoming Cassie message to all players.
        /// </summary>
        public static void BroadcastHelicopterCountdown()
        {
            SendImportantCassieMessage(Plugin.Singleton.Config.HeliIncomingCassie, Plugin.Singleton.Config.HelicopterIncomingMessage);
        }

        /// <summary>
        /// Broadcasts the helicopter incoming hint message to all ready players.
        /// </summary>
        public static void BroadcastHelicopterIncoming()
        {
            foreach (Player player in Player.ReadyList)
                player.SendHint(Plugin.Singleton.Config.HelicopterIncomingMessage, 5f);
        }

        #endregion
    }
    #endregion
}