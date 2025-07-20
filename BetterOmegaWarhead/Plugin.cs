namespace BetterOmegaWarhead
{
    using BetterOmegaWarhead.Core.LoggingUtils;
    using BetterOmegaWarhead.Core.PlayerUtils;
    using MEC;
    using System;
    using DoorUtility = BetterOmegaWarhead.Core.DoorUtils;
    using EscapeUtility = BetterOmegaWarhead.Core.PlayerUtils;
    using PlayerUtility = BetterOmegaWarhead.Core.PlayerUtils;
    using RoomUtility = BetterOmegaWarhead.Core.RoomUtils;


    public class Plugin : Exiled.API.Features.Plugin<Config>
    {
        public static Plugin Singleton;
        public override string Author { get; } = "iomatix";
        public override string Name { get; } = "BetterOmegaWarhead";
        public override string Prefix { get; } = "BetterOmegaWarhead";
        public override Version Version { get; } = new Version(7, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 6, 0);

        internal WarheadMethods WarheadMethods { get; private set; }
        internal PlayerMethods PlayerMethods { get; private set; }
        internal EventHandler EventHandler { get; private set; }
        internal CacheHandler CacheHandler { get; private set; }
        internal OmegaWarheadManager OmegaManager { get; private set; }

        public override void OnEnabled()
        {
            LogHelper.Debug("Enabling BetterOmegaWarhead plugin.");
            Config.Validate();
            Singleton = this;
            LogHelper.Debug("Initializing handlers.");
            EventHandler = new EventHandler(this);
            CacheHandler = new CacheHandler(this);
            PlayerMethods = new PlayerMethods(this);
            WarheadMethods = new WarheadMethods(this);
            OmegaManager = new OmegaWarheadManager(this);

            LogHelper.Debug("Registering events.");
            EventHandler.RegisterEvents();
            OmegaManager.Init();
            LogHelper.Debug("BetterOmegaWarhead plugin enabled.");
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            LogHelper.Debug("Disabling BetterOmegaWarhead plugin.");
            LogHelper.Debug("Disabling OmegaWarheadManager.");
            OmegaManager?.Disable();

            LogHelper.Debug("Killing coroutines.");
            foreach (CoroutineHandle handle in EventHandler.Coroutines)
            {
                Timing.KillCoroutines(handle);
                LogHelper.Debug($"Killed coroutine: {handle}");
            }
            EventHandler.Coroutines.Clear();
            LogHelper.Debug("Cleared coroutine list.");

            LogHelper.Debug("Unregistering events.");
            EventHandler.UnregisterEvents();

            LogHelper.Debug("Nullifying handlers and singleton.");
            Singleton = null;
            EventHandler = null;
            PlayerMethods = null;
            WarheadMethods = null;
            OmegaManager = null;

            LogHelper.Debug("BetterOmegaWarhead plugin disabled.");
            base.OnDisabled();
        }


    }
}
