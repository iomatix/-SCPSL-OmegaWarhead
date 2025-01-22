namespace BetterOmegaWarhead
{
    using Exiled.API.Features;


    public class NotificationMethods
    {
        private readonly Plugin _plugin;
        public NotificationMethods(Plugin plugin) => _plugin = plugin;
        public void SendCassieMessage(string message)
        {
            if (!(message.Length > 0)) return;
            Cassie.Message(message, isNoisy: false, isSubtitles: false, isHeld: false);
        }

        public void SendImportantCassieMessage(string message)
        {
            if (!(message.Length > 0)) return;
            Cassie.Clear();
            Cassie.Message(message, isSubtitles: false, isHeld: false);
        }

        public void BroadcastOmegaActivation()
        {
            Map.Broadcast(_plugin.Config.ActivatedMessage);
        }

        public void BroadcastHelicopterCountdown()
        {
            SendImportantCassieMessage(_plugin.Config.HeliIncomingCassie);
        }

    }
}
