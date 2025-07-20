namespace BetterOmegaWarhead.NotificationUtils
{
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
        /// Sends a Cassie message with specified settings, optionally clearing previous messages.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public static void SendCassieMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (Plugin.Singleton.Config.CassieMessageClearBeforeWarheadMessage) Exiled.API.Features.Cassie.Clear();
            Exiled.API.Features.Cassie.Message(message, isNoisy: false, isSubtitles: false, isHeld: false);
        }

        /// <summary>
        /// Sends an important Cassie message, optionally clearing previous messages.
        /// </summary>
        /// <param name="message">The important message to send.</param>
        public static void SendImportantCassieMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (Plugin.Singleton.Config.CassieMessageClearBeforeImportant) Exiled.API.Features.Cassie.Clear();
            Exiled.API.Features.Cassie.Message(message, isSubtitles: false, isHeld: false);
        }
        #endregion

        #region Message Duration Calculations
        /// <summary>
        /// Calculates the duration of a Cassie message based on its content and speed.
        /// </summary>
        /// <param name="message">The message to calculate duration for.</param>
        /// <param name="speed">The speed at which the message is played (default is 1).</param>
        /// <returns>The duration of the message in seconds.</returns>
        public static float CalculateCassieMessageDuration(string message, float speed = 1)
        {
            return Cassie.CalculateDuration(message, speed: speed);
        }

        /// <summary>
        /// Calculates the total duration of a dictionary of messages with their respective speeds.
        /// </summary>
        /// <param name="messageSpeedDictionary">A dictionary mapping messages to their playback speeds.</param>
        /// <returns>The total duration of all messages in seconds.</returns>
        public static float CalculateTotalMessagesDurations(Dictionary<string, float> messageSpeedDictionary)
        {
            float totalDuration = 0f;
            foreach ((string message, float speed) in messageSpeedDictionary)
            {
                totalDuration += CalculateCassieMessageDuration(message, speed);
            }
            return totalDuration;
        }

        /// <summary>
        /// Calculates the total duration of an array of messages with a default speed.
        /// </summary>
        /// <param name="defaultSpeed">The default speed for all messages (default is 1).</param>
        /// <param name="messages">The array of messages to calculate duration for.</param>
        /// <returns>The total duration of all messages in seconds.</returns>
        public static float CalculateTotalMessagesDurations(float defaultSpeed = 1f, params string[] messages)
        {
            float totalDuration = 0f;

            foreach (string message in messages)
            {
                totalDuration += CalculateCassieMessageDuration(message, defaultSpeed);
            }

            return totalDuration;
        }

        /// <summary>
        /// Calculates the total duration of a list of messages with a default speed.
        /// </summary>
        /// <param name="messages">The list of messages to calculate duration for.</param>
        /// <param name="defaultSpeed">The default speed for all messages (default is 1).</param>
        /// <returns>The total duration of all messages in seconds.</returns>
        public static float CalculateTotalMessagesDurations(List<string> messages, float defaultSpeed = 1f)
        {
            float totalDuration = 0f;

            foreach (string message in messages)
            {
                totalDuration += CalculateCassieMessageDuration(message, defaultSpeed);
            }

            return totalDuration;
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
            if (notifyTime <= 5) return $"{notifyTime} .G4";
            if (notifyTime == 10 || notifyTime == 15) return $".G3 {notifyTime} Seconds .G5";
            return $".G3 {notifyTime} Seconds until Omega Warhead Detonation .G5";
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
            SendImportantCassieMessage(Plugin.Singleton.Config.HeliIncomingCassie);
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