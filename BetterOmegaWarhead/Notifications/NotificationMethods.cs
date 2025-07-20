namespace BetterOmegaWarhead
{

    using LabApi.Features.Wrappers;
    public class NotificationMethods
    {
        private readonly Plugin _plugin;
        public NotificationMethods(Plugin plugin) => _plugin = plugin;
        public void SendCassieMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            Cassie.Message(message, isNoisy: false, isSubtitles: false, isHeld: false);
        }

        public void SendImportantCassieMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (_plugin.Config.CassieMessageClearBeforeImportant) Cassie.Clear();
            Cassie.Message(message, isSubtitles: false, isHeld: false);
        }

        public void BroadcastOmegaActivation()
        {
            foreach (Player player in Player.ReadyList)
                player.SendHint(_plugin.Config.ActivatedMessage, 6f);
        }

        public void BroadcastHelicopterCountdown()
        {
            SendImportantCassieMessage(_plugin.Config.HeliIncomingCassie);
        }

        public void BroadcastHelicopterIncoming()
        {
            foreach (Player player in Player.ReadyList)
                player.SendHint(_plugin.Config.HelicopterIncomingMessage, 5f);
        }

    }
}
