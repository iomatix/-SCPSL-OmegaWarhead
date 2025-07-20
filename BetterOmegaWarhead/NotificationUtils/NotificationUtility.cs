namespace BetterOmegaWarhead.NotificationUtils
{

    using LabApi.Features.Wrappers;
    using System.Collections.Generic;

    public static class DictionaryExtensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }

    public static class NotificationUtility
    {
        public static void SendCassieMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (Plugin.Singleton.Config.CassieMessageClearBeforeWarheadMessage) Exiled.API.Features.Cassie.Clear();
            Exiled.API.Features.Cassie.Message(message, isNoisy: false, isSubtitles: false, isHeld: false);
        }

        public static void SendImportantCassieMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (Plugin.Singleton.Config.CassieMessageClearBeforeImportant) Exiled.API.Features.Cassie.Clear();
            Exiled.API.Features.Cassie.Message(message, isSubtitles: false, isHeld: false);
        }


        public static float CalculateCassieMessageDuration(string message, float speed = 1)
        {
            return Cassie.CalculateDuration(message, speed: speed);
        }

        public static float CalculateTotalMessagesDurations(Dictionary<string, float> messageSpeedDictionary)
        {
            float totalDuration = 0f;
            foreach ((string message, float speed) in messageSpeedDictionary)
            {
                totalDuration += CalculateCassieMessageDuration(message, speed);
            }
            return totalDuration;
        }

        public static float CalculateTotalMessagesDurations(float defaultSpeed = 1f, params string[] messages)
        {
            float totalDuration = 0f;

            foreach (string message in messages)
            {
                totalDuration += CalculateCassieMessageDuration(message, defaultSpeed);
            }

            return totalDuration;
        }

        public static float CalculateTotalMessagesDurations(List<string> messages, float defaultSpeed = 1f)
        {
            float totalDuration = 0f;

            foreach (string message in messages)
            {
                totalDuration += CalculateCassieMessageDuration(message, defaultSpeed);
            }

            return totalDuration;
        }


        public static string GetCassieCounterNotifyMessage(int notifyTime)
        {
            if (notifyTime <= 5) return ".G5";
            if (notifyTime == 10) return $"{notifyTime} Seconds";
            return $".G3 {notifyTime} Seconds until Omega Warhead Detonation .G5";
        }


        public static void BroadcastOmegaActivation()
        {
            foreach (Player player in Player.ReadyList)
                player.SendHint(Plugin.Singleton.Config.ActivatedMessage, 6f);
        }

        public static void BroadcastHelicopterCountdown()
        {
            SendImportantCassieMessage(Plugin.Singleton.Config.HeliIncomingCassie);
        }

        public static void BroadcastHelicopterIncoming()
        {
            foreach (Player player in Player.ReadyList)
                player.SendHint(Plugin.Singleton.Config.HelicopterIncomingMessage, 5f);
        }

    }
}
