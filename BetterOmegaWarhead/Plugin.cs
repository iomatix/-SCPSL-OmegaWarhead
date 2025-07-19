namespace BetterOmegaWarhead
{
    using System;
    using Exiled.API.Features;
    using BetterOmegaWarhead.Core.PlayerUtils;
    using MEC;

    using ServerHandler = LabApi.Events.Handlers.ServerEvents;
    using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;
    using WarheadHandler = LabApi.Events.Handlers.WarheadEvents;
    using OmegaPlayer = BetterOmegaWarhead.Core.PlayerUtils;


    public class Plugin : Plugin<Config>
    {
        public static Plugin Singleton;
        public override string Author { get; } = "iomatix";
        public override string Name { get; } = "BetterOmegaWarhead";
        public override string Prefix { get; } = "BetterOmegaWarhead";
        public override Version Version { get; } = new Version(7, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 6, 0);

        internal WarheadEventMethods EventMethods { get; private set; }
        internal PlayerMethods PlayerMethods { get; private set; }
        internal NotificationMethods NotificationMethods { get; private set; }
        internal EventHandler EventHandler { get; private set; }
        internal CacheHandler CacheHandler { get; private set; }
        internal OmegaWarheadManager OmegaManager { get; private set; }

        public override void OnEnabled()
        {
            Log.Debug("Enabling BetterOmegaWarhead plugin.");
            Config.Validate();
            Singleton = this;
            Log.Debug("Initializing handlers.");
            EventHandler = new EventHandler(this);
            CacheHandler = new CacheHandler(this);
            PlayerMethods = new PlayerMethods(this);
            EventMethods = new WarheadEventMethods(this);
            NotificationMethods = new NotificationMethods(this);
            OmegaManager = new OmegaWarheadManager(this);

            Log.Debug("Registering events.");
            RegisterEvents();
            OmegaManager.Init();
            Log.Debug("BetterOmegaWarhead plugin enabled.");
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Log.Debug("Disabling BetterOmegaWarhead plugin.");
            Log.Debug("Disabling OmegaWarheadManager.");
            OmegaManager?.Disable();

            Log.Debug("Killing coroutines.");
            foreach (CoroutineHandle handle in EventHandler.Coroutines)
            {
                Timing.KillCoroutines(handle);
                Log.Debug($"Killed coroutine: {handle}");
            }
            EventHandler.Coroutines.Clear();
            Log.Debug("Cleared coroutine list.");

            Log.Debug("Nullifying handlers and singleton.");
            Singleton = null;
            EventHandler = null;
            PlayerMethods = null;
            EventMethods = null;
            NotificationMethods = null;
            OmegaManager = null;

            Log.Debug("Unregistering events.");
            UnregisterEvents();
            Log.Debug("BetterOmegaWarhead plugin disabled.");
            base.OnDisabled();
        }

        public void RegisterEvents()
        {
            Log.Debug("Registering event handlers.");
            ServerHandler.WaitingForPlayers += EventHandler.OnWaitingForPlayers;
            WarheadHandler.Starting += EventHandler.OnWarheadStart;
            WarheadHandler.Stopping += EventHandler.OnWarheadStop;
            WarheadHandler.Detonating += EventHandler.OnWarheadDetonate;
            PlayerHandler.ChangingRole += EventHandler.OnChangingRole;
            Log.Debug("Event handlers registered.");
        }

        public void UnregisterEvents()
        {
            Log.Debug("Unregistering event handlers.");
            ServerHandler.WaitingForPlayers -= EventHandler.OnWaitingForPlayers;
            WarheadHandler.Starting -= EventHandler.OnWarheadStart;
            WarheadHandler.Stopping -= EventHandler.OnWarheadStop;
            WarheadHandler.Detonating -= EventHandler.OnWarheadDetonate;
            PlayerHandler.ChangingRole -= EventHandler.OnChangingRole;
            Log.Debug("Event handlers unregistered.");
        }
    }
}
