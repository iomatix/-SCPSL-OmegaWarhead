namespace BetterOmegaWarhead
{
    using System;
    using Exiled.API.Features;
    using MEC;

    public class Plugin : Plugin<Config>
    {
        public static Plugin Singleton;
        public override string Author { get; } = "ClaudioPanConQueso & iomatix";
        public override string Name { get; } = "BetterOmegaWarhead";
        public override string Prefix { get; } = "BetterOmegaWarhead";
        public override Version Version { get; } = new Version(6, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 0, 0);

        public Methods Methods { get; private set; }
        internal EventHandlers EventHandlers { get; private set; }

        public override void OnEnabled()
        {
            Singleton = this;
            EventHandlers = new EventHandlers(this);
            Methods = new Methods(this);

            RegisterEvents();
            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            foreach (CoroutineHandle handle in EventHandlers.Coroutines) Timing.KillCoroutines(handle);
            EventHandlers.Coroutines.Clear();

            Singleton = null;
            EventHandlers = null;

            UnregisterEvents();
            base.OnDisabled();
        }
        public void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += EventHandlers.OnWaitingForPlayers;
            Exiled.Events.Handlers.Warhead.Starting += EventHandlers.OnWarheadStart;
        }

        public void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= EventHandlers.OnWaitingForPlayers;
            Exiled.Events.Handlers.Warhead.Starting -= EventHandlers.OnWarheadStart;
        }
    }
}
