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

    /// <summary>
    /// Manages event handling for the Omega Warhead plugin, including server, player, and warhead events.
    /// </summary>
    #region EventHandler Class
    public class EventHandler
    {
        #region Fields
        private readonly Plugin _plugin;

        /// <summary>
        /// Gets the list of active coroutine handles managed by the event handler.
        /// </summary>
        public readonly List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandler"/> class.
        /// </summary>
        /// <param name="plugin">Reference to the core plugin instance.</param>
        public EventHandler(Plugin plugin)
        {
            _plugin = plugin;
        }
        #endregion

        #region Event Registration
        /// <summary>
        /// Registers event handlers for server, player, and warhead events.
        /// </summary>
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

        /// <summary>
        /// Unregisters all event handlers to clean up resources.
        /// </summary>
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
        #endregion

        #region Server Event Handlers
        /// <summary>
        /// Handles the WaitingForPlayers event, resetting the cache and disabling warhead methods.
        /// </summary>
        public void OnWaitingForPlayers()
        {
            LogHelper.Debug("OnWaitingForPlayers triggered: resetting cache and disabling warhead methods.");
            _plugin.CacheHandler.ResetCache();
            _plugin.OmegaManager.Disable();
        }

        /// <summary>
        /// Handles the RoundStart event, resetting the cache and initializing the Omega Warhead manager.
        /// </summary>
        public void OnRoundStart()
        {
            LogHelper.Debug("OnRoundStart triggered: resetting cache and initializing OmegaWarheadManager.");
            _plugin.CacheHandler.ResetCache();
            _plugin.OmegaManager.Init();
        }

        /// <summary>
        /// Handles the RoundEnd event, resetting the cache and disabling the Omega Warhead manager.
        /// </summary>
        /// <param name="ev">The round ended event arguments, including the leading team.</param>
        public void OnRoundEnd(LabApi.Events.Arguments.ServerEvents.RoundEndedEventArgs ev)
        {
            LogHelper.Debug($"OnRoundEnd triggered, leading team: {ev.LeadingTeam}. Disabling OmegaWarheadManager.");
            _plugin.CacheHandler.ResetCache();
            _plugin.OmegaManager.Disable();
        }
        #endregion

        #region Warhead Event Handlers
        /// <summary>
        /// Handles the WarheadStarting event, triggering the Omega Warhead start logic.
        /// </summary>
        /// <param name="ev">The warhead starting event arguments.</param>
        public void OnWarheadStart(LabApi.Events.Arguments.WarheadEvents.WarheadStartingEventArgs ev)
        {
            LogHelper.Debug("OnWarheadStart triggered.");
            _plugin.OmegaManager.HandleWarheadStart(ev);
        }

        /// <summary>
        /// Handles the WarheadStopping event, triggering the Omega Warhead stop logic.
        /// </summary>
        /// <param name="ev">The warhead stopping event arguments.</param>
        public void OnWarheadStop(LabApi.Events.Arguments.WarheadEvents.WarheadStoppingEventArgs ev)
        {
            LogHelper.Debug("OnWarheadStop triggered.");
            _plugin.OmegaManager.HandleWarheadStop(ev);
        }

        /// <summary>
        /// Handles the WarheadDetonating event, triggering the Omega Warhead detonation logic.
        /// </summary>
        /// <param name="ev">The warhead detonating event arguments.</param>
        public void OnWarheadDetonate(LabApi.Events.Arguments.WarheadEvents.WarheadDetonatingEventArgs ev)
        {
            LogHelper.Debug("OnWarheadDetonate triggered.");
            _plugin.OmegaManager.HandleWarheadDetonate(ev);
        }
        #endregion

        #region Player and Respawn Event Handlers
        /// <summary>
        /// Handles the WaveRespawning event, blocking faction spawns if disabled.
        /// </summary>
        /// <param name="ev">The wave respawning event arguments, including the faction.</param>
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

        /// <summary>
        /// Handles the PlayerChangingRole event, blocking role assignments for disabled factions.
        /// </summary>
        /// <param name="ev">The player changing role event arguments, including the player and new role.</param>
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
        #endregion
    }
    #endregion
}