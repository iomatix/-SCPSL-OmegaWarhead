namespace OmegaWarhead
{
    using LabApi.Features.Wrappers;
    using MEC;
    using OmegaWarhead.Core.LoggingUtils;
    using PlayerRoles;
    using System.Collections.Generic;
    using System.Linq;
    using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;
    using ServerHandler = LabApi.Events.Handlers.ServerEvents;
    using WarheadHandler = LabApi.Events.Handlers.WarheadEvents;

    /// <summary>
    /// Manages event handling for the Omega Warhead plugin, including server, player, and warhead events.
    /// </summary>
    #region EventHandler Class
    public class EventHandler
    {
        #region Fields
        private readonly Plugin _plugin;

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

            // Server Events
            ServerHandler.RoundStarting += OnRoundStart;
            ServerHandler.RoundEnded += OnRoundEnd;
            ServerHandler.WaveRespawning += OnWaveRespawning;
            ServerHandler.DeadmanSequenceActivating += OnWarheadDeadManSwitch;

            // Warhead Events
            WarheadHandler.Starting += OnWarheadStart;
            WarheadHandler.Stopping += OnWarheadStop;
            WarheadHandler.Detonating += OnWarheadDetonate;

            // Player Events
            PlayerHandler.ChangingRole += OnChangingRole;

            LogHelper.Debug("Event handlers registered.");
        }

        /// <summary>
        /// Unregisters all event handlers to clean up resources.
        /// </summary>
        public void UnregisterEvents()
        {
            LogHelper.Debug("Unregistering event handlers.");

            // Server Events
            ServerHandler.RoundStarting -= OnRoundStart;
            ServerHandler.RoundEnded -= OnRoundEnd;
            ServerHandler.WaveRespawning -= OnWaveRespawning;
            ServerHandler.DeadmanSequenceActivating -= OnWarheadDeadManSwitch;

            // Warhead Events
            WarheadHandler.Starting -= OnWarheadStart;
            WarheadHandler.Stopping -= OnWarheadStop;
            WarheadHandler.Detonating -= OnWarheadDetonate;

            // Player Events
            PlayerHandler.ChangingRole -= OnChangingRole;

            LogHelper.Debug("Event handlers unregistered.");
        }
        #endregion

        #region Coroutine Management
        /// <summary>
        /// Safely kills all coroutines tracked by the event handler.
        /// </summary>
        private void KillTrackedCoroutines()
        {
            foreach (string tag in Shared.CoroutineTags.AllStaticTags)
            {
                Timing.KillCoroutines(tag);
            }
            LogHelper.Debug("Killed all tracked coroutines in EventHandler via tags.");
        }
        #endregion

        #region Server Event Handlers
        /// <summary>
        /// Handles the RoundStart event, resetting the cache and initializing the Omega Warhead manager.
        /// </summary>
        public void OnRoundStart(LabApi.Events.Arguments.ServerEvents.RoundStartingEventArgs ev)
        {
            LogHelper.Debug("OnRoundStart triggered: resetting cache and initializing OmegaWarheadManager.");
            KillTrackedCoroutines();
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
            KillTrackedCoroutines();
            _plugin.CacheHandler.ResetCache();
            _plugin.OmegaManager.Disable();
        }

        /// <summary>  
        /// Handles the Deadman Sequence activating event, allowing cancellation based on plugin configuration.  
        /// </summary>  
        /// <param name="ev">Event arguments containing the Deadman Sequence activation data and cancellation flag.</param>  
        public void OnWarheadDeadManSwitch(LabApi.Events.Arguments.ServerEvents.DeadmanSequenceActivatingEventArgs ev)
        {
            LogHelper.Debug("OnWarheadDeadManSwitch triggered.");
            LogHelper.Debug($"Config value of DisableDeadManSwitch is {_plugin.Config.DisableDeadManSwitch.ToString()}.");
            if (_plugin.Config.DisableDeadManSwitch)
            {
                LogHelper.Debug("Cancelling event.");
                ev.IsAllowed = false;

                // trigger a mini-wave for immediate action  
                if (_plugin.Config.TriggerMiniWaveOnDmsCancel && !_plugin.OmegaManager.IsOmegaActive && !_plugin.OmegaManager.IsOmegaDetonated)
                {
                    LogHelper.Debug("Triggering mini-wave due to Deadman Switch cancellation.");
                    // Add respawn tokens to both factions to prolong the match  
                    RespawnWave mtfWave = RespawnWaves.PrimaryMtfWave;
                    if (mtfWave != null)
                    {
                        mtfWave.RespawnTokens += _plugin.Config.TokensAddedOnDmsCancel;
                        mtfWave.Influence += _plugin.Config.InfluenceBoostOnDmsCancel;
                    }

                    RespawnWave chaosWave = RespawnWaves.PrimaryChaosWave;
                    if (chaosWave != null)
                    {
                        chaosWave.RespawnTokens += _plugin.Config.TokensAddedOnDmsCancel;
                        chaosWave.Influence += _plugin.Config.InfluenceBoostOnDmsCancel;
                    }


                    NotificationUtils.NotificationUtility.SendCassieMessage(_plugin.Config.CassieMessageMiniWaveOnDmsCancel); 
                    if (_plugin.Config.OpenBlastDoorsOnDmsCancel) Warhead.OpenBlastDoors();


                    // Get the faction with fewer players to balance  
                    int ntfCount = Player.List.Count(p =>
                    p.Role == RoleTypeId.NtfPrivate ||
                    p.Role == RoleTypeId.NtfSergeant ||
                    p.Role == RoleTypeId.NtfCaptain ||
                    p.Role == RoleTypeId.NtfSpecialist ||
                    p.Role == RoleTypeId.Scientist
                    );
                    int ciCount = Player.List.Count(p =>
                    p.Role == RoleTypeId.ChaosRepressor ||
                    p.Role == RoleTypeId.ChaosMarauder ||
                    p.Role == RoleTypeId.ChaosRifleman ||
                    p.Role == RoleTypeId.ChaosConscript ||
                    p.Role == RoleTypeId.ClassD);

                    Faction targetFaction = ntfCount < ciCount ? Faction.FoundationStaff : Faction.FoundationEnemy;
                    LogHelper.Debug($"Target faction for mini-wave: {targetFaction}. NTF count: {ntfCount}, Chaos count: {ciCount}.");

                    // Trigger mini-wave for the disadvantaged faction  
                    MiniRespawnWave miniWave = null;
                    if (targetFaction == Faction.FoundationStaff)
                    {
                        miniWave = RespawnWaves.MiniMtfWave;
                    }
                    else
                    {
                        miniWave = RespawnWaves.MiniChaosWave;
                    }

                    if (miniWave != null)
                    {
                        miniWave.InitiateRespawn();
                        LogHelper.Debug($"Mini-wave initiated for {targetFaction}.");
                    }
                }
            }
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