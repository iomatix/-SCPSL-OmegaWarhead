namespace BetterOmegaWarhead
{
    using MEC;
    using BetterOmegaWarhead.Core.LoggingUtils;
    using InventorySystem.Items.Usables;
    using PlayerRoles;
    using System.Collections.Generic;
    using ServerHandler = LabApi.Events.Handlers.ServerEvents;
    using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;
    using WarheadHandler = LabApi.Events.Handlers.WarheadEvents;

    public class EventHandler
    {
        private readonly Plugin _plugin;
        public readonly List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

        public EventHandler(Plugin plugin)
        {
            _plugin = plugin;
        }

        public void RegisterEvents()
        {
            LogHelper.Debug("Registering event handlers.");
            ServerHandler.WaitingForPlayers += OnWaitingForPlayers;
            WarheadHandler.Starting += OnWarheadStart;
            WarheadHandler.Stopping += OnWarheadStop;
            WarheadHandler.Detonating += OnWarheadDetonate;
            ServerHandler.WaveRespawning += OnWaveRespawning;
            PlayerHandler.ChangingRole += OnChangingRole;
            LogHelper.Debug("Event handlers registered.");
        }

        public void UnregisterEvents()
        {
            LogHelper.Debug("Unregistering event handlers.");
            ServerHandler.WaitingForPlayers -= OnWaitingForPlayers;
            WarheadHandler.Starting -= OnWarheadStart;
            WarheadHandler.Stopping -= OnWarheadStop;
            WarheadHandler.Detonating -= OnWarheadDetonate;
            ServerHandler.WaveRespawning -= OnWaveRespawning;
            PlayerHandler.ChangingRole -= OnChangingRole;
            LogHelper.Debug("Event handlers unregistered.");
        }

        public void OnWaitingForPlayers()
        {
            LogHelper.Debug("OnWaitingForPlayers triggered: resetting cache and disabling warhead methods.");
            _plugin.CacheHandler.ResetCache();
            _plugin.OmegaManager.Disable();
        }

        public void OnRoundStart()
        {
            LogHelper.Debug("OnRoundStart triggered: resetting cache and initializing OmegaWarheadManager.");
            _plugin.CacheHandler.ResetCache();
            _plugin.OmegaManager.Init();
        }

        public void OnRoundEnd(LabApi.Events.Arguments.ServerEvents.RoundEndedEventArgs ev)
        {
            LogHelper.Debug($"OnRoundEnd triggered, leading team: {ev.LeadingTeam}. Disabling OmegaWarheadManager.");
            _plugin.CacheHandler.ResetCache();
            _plugin.OmegaManager.Disable();
        }

        public void OnWarheadStart(LabApi.Events.Arguments.WarheadEvents.WarheadStartingEventArgs ev)
        {
            LogHelper.Debug("OnWarheadStart triggered.");
            _plugin.OmegaManager.HandleWarheadStart(ev);
        }

        public void OnWarheadStop(LabApi.Events.Arguments.WarheadEvents.WarheadStoppingEventArgs ev)
        {
            LogHelper.Debug("OnWarheadStop triggered.");
            _plugin.OmegaManager.HandleWarheadStop(ev);
        }

        public void OnWarheadDetonate(LabApi.Events.Arguments.WarheadEvents.WarheadDetonatingEventArgs ev)
        {
            LogHelper.Debug("OnWarheadDetonate triggered.");
            _plugin.OmegaManager.HandleWarheadDetonate(ev);
        }

        public void OnWaveRespawning(LabApi.Events.Arguments.ServerEvents.WaveRespawningEventArgs ev)
        {
            Faction faction = ev.Wave.Faction;
            LogHelper.Debug($"OnWaveRespawning triggered. Faction: {faction}");

            if (_plugin.CacheHandler.IsFactionDisabled(faction))
            {
                LogHelper.Debug($"Blocked wave. Faction {faction} is disabled.");
                ev.IsAllowed = false;
            }
        }

        public void OnChangingRole(LabApi.Events.Arguments.PlayerEvents.PlayerChangingRoleEventArgs ev)
        {
            Faction faction = ev.NewRole.GetFaction();
            LogHelper.Debug($"OnChangingRole triggered. Player: {ev.Player.Nickname}, NewRole: {ev.NewRole}, Faction: {faction}");

            if (_plugin.CacheHandler.IsFactionDisabled(faction))
            {
                LogHelper.Debug($"Blocked role assignment. Faction {faction} is disabled.");
                ev.IsAllowed = false;
            }
        }
    }
}
