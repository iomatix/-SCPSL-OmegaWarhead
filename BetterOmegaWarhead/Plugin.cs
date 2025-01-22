﻿namespace BetterOmegaWarhead
{
    using System;
    using Exiled.API.Features;
    using Exiled.Events;
    using MEC;
    using Server = Exiled.Events.Handlers.Server;
    using Warhead = Exiled.Events.Handlers.Warhead;
    public class Plugin : Plugin<Config>
    {
        public static Plugin Singleton;
        public override string Author { get; } = "ClaudioPanConQueso & iomatix";
        public override string Name { get; } = "BetterOmegaWarhead";
        public override string Prefix { get; } = "BetterOmegaWarhead";
        public override Version Version { get; } = new Version(6, 4, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 4, 0);

        internal WarheadEventMethods EventMethods { get; private set; }
        internal PlayerMethods PlayerMethods { get; private set; }

        internal NotificationMethods NotificationMethods { get; private set; }
        internal EventHandlers EventHandlers { get; private set; }

        public override void OnEnabled()
        {
            Singleton = this;
            EventHandlers = new EventHandlers(this);
            PlayerMethods = new PlayerMethods(this);
            EventMethods = new WarheadEventMethods(this);
            NotificationMethods = new NotificationMethods(this);

            RegisterEvents();
            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            foreach (CoroutineHandle handle in EventHandlers.Coroutines) Timing.KillCoroutines(handle);
            EventHandlers.Coroutines.Clear();

            Singleton = null;
            EventHandlers = null;
            PlayerMethods = null;
            EventMethods = null;
            NotificationMethods = null;

            UnregisterEvents();
            base.OnDisabled();
        }
        public void RegisterEvents()
        {
            Server.WaitingForPlayers += EventHandlers.OnWaitingForPlayers;
            Warhead.Starting += EventHandlers.OnWarheadStart;
            //API error CS0019: Operator '+=' cannot be applied to operands of type 'Event
            //Warhead.Stopping += EventHandlers.OnWarheadStop;
            //Warhead.Detonating += EventHandlers.OnWarheadDetonate;
        }

        public void UnregisterEvents()
        {
            Server.WaitingForPlayers -= EventHandlers.OnWaitingForPlayers;
            Warhead.Starting -= EventHandlers.OnWarheadStart;
            //API error CS0019: Operator '-=' cannot be applied to operands of type 'Event
            //Warhead.Stopping -= EventHandlers.OnWarheadStop;
            //Warhead.Detonating -= EventHandlers.OnWarheadDetonate;
        }
    }
}
