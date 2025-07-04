namespace BetterOmegaWarhead
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.Events;
    using MEC;
    using UnityEngine;
    using Server = Exiled.Events.Handlers.Server;
    using Warhead = Exiled.Events.Handlers.Warhead;
    using Player = Exiled.Events.Handlers.Player;

    public class Plugin : Plugin<Config>
    {
        public static Plugin Singleton;
        public override string Author { get; } = "ClaudioPanConQueso & iomatix";
        public override string Name { get; } = "BetterOmegaWarhead";
        public override string Prefix { get; } = "BetterOmegaWarhead";
        public override Version Version { get; } = new Version(6, 6, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 5, 0);

        internal WarheadEventMethods EventMethods { get; private set; }
        internal PlayerMethods PlayerMethods { get; private set; }
        internal NotificationMethods NotificationMethods { get; private set; }
        internal EventHandlers EventHandlers { get; private set; }
        internal CacheHandlers CacheHandlers { get; private set; }

        public override void OnEnabled()
        {
            Log.Debug("Enabling BetterOmegaWarhead plugin.");
            Singleton = this;
            Log.Debug("Initializing handlers.");
            EventHandlers = new EventHandlers(this);
            CacheHandlers = new CacheHandlers(this);
            PlayerMethods = new PlayerMethods(this);
            EventMethods = new WarheadEventMethods(this);
            NotificationMethods = new NotificationMethods(this);

            Log.Debug("Registering events.");
            RegisterEvents();
            Log.Debug("BetterOmegaWarhead plugin enabled.");
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Log.Debug("Disabling BetterOmegaWarhead plugin.");
            Log.Debug($"Killing {EventHandlers.Coroutines.Count} coroutines.");
            foreach (CoroutineHandle handle in EventHandlers.Coroutines)
            {
                Timing.KillCoroutines(handle);
                Log.Debug($"Killed coroutine: {handle}");
            }
            EventHandlers.Coroutines.Clear();
            Log.Debug("Cleared coroutine list.");

            Log.Debug("Nullifying handlers and singleton.");
            Singleton = null;
            EventHandlers = null;
            PlayerMethods = null;
            EventMethods = null;
            NotificationMethods = null;

            Log.Debug("Unregistering events.");
            UnregisterEvents();
            Log.Debug("BetterOmegaWarhead plugin disabled.");
            base.OnDisabled();
        }

        public void RegisterEvents()
        {
            Log.Debug("Registering event handlers.");
            Server.WaitingForPlayers += EventHandlers.OnWaitingForPlayers;
            Warhead.Starting += EventHandlers.OnWarheadStart;
            Warhead.Stopping += EventHandlers.OnWarheadStop;
            Warhead.Detonating += EventHandlers.OnWarheadDetonate;
            Player.ChangingRole += EventHandlers.OnChangingRole;
            Log.Debug("Event handlers registered.");
        }

        public void UnregisterEvents()
        {
            Log.Debug("Unregistering event handlers.");
            Server.WaitingForPlayers -= EventHandlers.OnWaitingForPlayers;
            Warhead.Starting -= EventHandlers.OnWarheadStart;
            Warhead.Stopping -= EventHandlers.OnWarheadStop;
            Warhead.Detonating -= EventHandlers.OnWarheadDetonate;
            Player.ChangingRole -= EventHandlers.OnChangingRole;
            Log.Debug("Event handlers unregistered.");
        }
    }
}