using LabApi.Extensions;
using LabApi.Features.Wrappers;
using PlayerRoles;
using System;
using Logger = LabApi.Extensions.Misc.iLogger;
using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;
using ServerHandler = LabApi.Events.Handlers.ServerEvents;
using WarheadHandler = LabApi.Events.Handlers.WarheadEvents;

namespace OmegaWarhead
{
    /// <summary>
    /// Centralized event interceptor matrix routing native LabAPI game lifecycle events into the OmegaWarhead core engine.
    /// </summary>
    public class EventHandler
    {
        #region Private Repositories
        private readonly Plugin _plugin;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandler"/> class bound to the root plugin token.
        /// </summary>
        public EventHandler(Plugin plugin) => _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        #endregion

        #region Operational Hook Registration
        /// <summary>
        /// Binds subsystem listeners against the global LabAPI multi-threaded event hub.
        /// </summary>
        public void RegisterEvents()
        {
            Logger.Debug(_plugin.Name, "Initializing global event pipeline subscription matrix...", _plugin.Config.Debug);

            // Core Server Infrastructure Hooks
            ServerHandler.RoundStarting += OnRoundStart;
            ServerHandler.RoundEnded += OnRoundEnd;
            ServerHandler.WaveRespawning += OnWaveRespawning;
            ServerHandler.DeadmanSequenceActivating += OnWarheadDeadManSwitch;

            // Dedicated Warhead Engine Hooks
            WarheadHandler.Starting += OnWarheadStart;
            WarheadHandler.Stopping += OnWarheadStop;
            WarheadHandler.Detonating += OnWarheadDetonate;

            // Active Entity Profiler Hooks
            PlayerHandler.ChangingRole += OnChangingRole;

            Logger.Debug(_plugin.Name, "All core architectural event pathways securely attached.", _plugin.Config.Debug);
        }

        /// <summary>
        /// Detaches subsystem listeners to prevent permanent domain assembly holding during hot-reloads.
        /// </summary>
        public void UnregisterEvents()
        {
            Logger.Debug(_plugin.Name, "Dismantling event pipeline subscription matrix...", _plugin.Config.Debug);

            // Core Server Infrastructure Hooks
            ServerHandler.RoundStarting -= OnRoundStart;
            ServerHandler.RoundEnded -= OnRoundEnd;
            ServerHandler.WaveRespawning -= OnWaveRespawning;
            ServerHandler.DeadmanSequenceActivating -= OnWarheadDeadManSwitch;

            // Dedicated Warhead Engine Hooks
            WarheadHandler.Starting -= OnWarheadStart;
            WarheadHandler.Stopping -= OnWarheadStop;
            WarheadHandler.Detonating -= OnWarheadDetonate;

            // Active Entity Profiler Hooks
            PlayerHandler.ChangingRole -= OnChangingRole;

            Logger.Debug(_plugin.Name, "Global event pathways unlinked successfully.", _plugin.Config.Debug);
        }
        #endregion

        #region Core Server Interceptors
        /// <summary>
        /// Resets temporary databases and maps hardware parameters for a new round instantiation.
        /// </summary>
        public void OnRoundStart(LabApi.Events.Arguments.ServerEvents.RoundStartingEventArgs ev)
        {
            Logger.Debug(_plugin.Name, "RoundStarting sequence captured. Refreshing operational databases...", _plugin.Config.Debug);
            Shared.CoroutineTags.AllStaticTags.KillCoroutines();

            _plugin.CacheHandler?.ResetCache();
            _plugin.OmegaManager?.Init();
        }

        /// <summary>
        /// Clears tracking graphs and isolates resources upon round completion state transition.
        /// </summary>
        public void OnRoundEnd(LabApi.Events.Arguments.ServerEvents.RoundEndedEventArgs ev)
        {
            Logger.Debug(_plugin.Name, $"RoundEnded sequence captured. Leading Team: {ev.LeadingTeam}. Shutting down sub-engines...", _plugin.Config.Debug);
            Shared.CoroutineTags.AllStaticTags.KillCoroutines();

            _plugin.CacheHandler?.ResetCache();
            _plugin.OmegaManager?.Disable();
        }

        /// <summary>  
        /// Intercepts base-game emergency Dead Man Switch sequences and injects strategic mini-wave mechanics.  
        /// </summary>  
        public void OnWarheadDeadManSwitch(LabApi.Events.Arguments.ServerEvents.DeadmanSequenceActivatingEventArgs ev)
        {
            Logger.Debug(_plugin.Name, $"DeadmanSequenceActivating interceptor triggered. Configuration Constraint Status: {_plugin.Config.DisableDeadManSwitch}", _plugin.Config.Debug);

            if (!_plugin.Config.DisableDeadManSwitch) return;

            Logger.Debug(_plugin.Name, "Intercepted base Dead Man sequence. Revoking default engine authorization...", _plugin.Config.Debug);
            ev.IsAllowed = false;

            // Evaluate constraints before initializing combat reinforcing mini-waves
            if (_plugin.Config.TriggerMiniWaveOnDmsCancel && !_plugin.OmegaManager.IsOmegaActive && !_plugin.OmegaManager.IsOmegaDetonated)
            {
                Logger.Debug(_plugin.Name, "Dead Man failure recovery initiated. Constructing dynamic rescue payload parameters...", _plugin.Config.Debug);

                // Inject strategic reinforcement points directly into active mobile strike forces
                RespawnWave mtfWave = RespawnWaves.PrimaryMtfWave;
                if (mtfWave is not null)
                {
                    mtfWave.RespawnTokens += _plugin.Config.TokensAddedOnDmsCancel;
                    mtfWave.Influence += _plugin.Config.InfluenceBoostOnDmsCancel;
                }

                RespawnWave chaosWave = RespawnWaves.PrimaryChaosWave;
                if (chaosWave is not null)
                {
                    chaosWave.RespawnTokens += _plugin.Config.TokensAddedOnDmsCancel;
                    chaosWave.Influence += _plugin.Config.InfluenceBoostOnDmsCancel;
                }

                // Architectural Fix: Eradicated NotificationUtility wrapper, streaming directly via Fluent API channels
                CassieExtensions.ProcessAndDispatchMessage(
                    _plugin.Config.CassieMessageMiniWaveOnDmsCancel,
                    string.Empty,
                    _plugin.Config.CassieMessageClearBeforeWarheadMessage,
                    _plugin.Config.CassieMessagePriority,
                    _plugin.Config.DisableCassieMessages
                );

                if (_plugin.Config.OpenBlastDoorsOnDmsCancel)
                {
                    Warhead.OpenBlastDoors();
                }

                int ntfCount = 0;
                int ciCount = 0;

                foreach (Player player in Player.List)
                {
                    Faction faction = player.Role.GetFaction();
                    if (faction is Faction.FoundationStaff) ntfCount++;
                    else if (faction is Faction.FoundationEnemy) ciCount++;
                }

                Faction targetFaction = ntfCount < ciCount ? Faction.FoundationStaff : Faction.FoundationEnemy;
                Logger.Debug(_plugin.Name, $"Strategic tactical evaluation complete. Target Balance Vector: {targetFaction} (Staff Load: {ntfCount} vs Enemy Load: {ciCount})", _plugin.Config.Debug);

                // Route deployment requests through optimized reinforcement sub-channels
                MiniRespawnWave miniWave = targetFaction is Faction.FoundationStaff ? RespawnWaves.MiniMtfWave : RespawnWaves.MiniChaosWave;

                if (miniWave is not null)
                {
                    miniWave.InitiateRespawn();
                    Logger.Debug(_plugin.Name, $"Tactical vanguard reinforcement deployment wave authorized for faction context: {targetFaction}", _plugin.Config.Debug);
                }
            }
        }
        #endregion

        #region Dedicated Warhead Interceptors
        public void OnWarheadStart(LabApi.Events.Arguments.WarheadEvents.WarheadStartingEventArgs ev)
        {
            Logger.Debug(_plugin.Name, "Warhead Starting signal hooked. Delegating verification logic to core manager...", _plugin.Config.Debug);
            _plugin.OmegaManager?.HandleWarheadStart(ev);
        }

        public void OnWarheadStop(LabApi.Events.Arguments.WarheadEvents.WarheadStoppingEventArgs ev)
        {
            Logger.Debug(_plugin.Name, "Warhead Stopping signal hooked. Delegating safety abort routing to core manager...", _plugin.Config.Debug);
            _plugin.OmegaManager?.HandleWarheadStop(ev);
        }

        public void OnWarheadDetonate(LabApi.Events.Arguments.WarheadEvents.WarheadDetonatingEventArgs ev)
        {
            Logger.Debug(_plugin.Name, "Warhead Detonating critical threshold reached. Passing control to manager matrix...", _plugin.Config.Debug);
            _plugin.OmegaManager?.HandleWarheadDetonate(ev);
        }
        #endregion

        #region Spatial Faction Interceptors
        /// <summary>
        /// Filters global deployment pipelines and dynamically vectors spawn allowances for restricted teams.
        /// </summary>
        public void OnWaveRespawning(LabApi.Events.Arguments.ServerEvents.WaveRespawningEventArgs ev)
        {
            if (ev?.Wave is null) return;

            Faction faction = ev.Wave.Faction;
            Logger.Debug(_plugin.Name, $"WaveRespawning process trapped. Analyzing target faction deployment footprint: {faction}", _plugin.Config.Debug);

            if (_plugin.CacheHandler is not null && _plugin.CacheHandler.IsFactionDisabled(faction))
            {
                Logger.Debug(_plugin.Name, $"Deployment unauthorized: Faction signature '{faction}' is locked out by alternative sequence boundaries.", _plugin.Config.Debug);
                ev.IsAllowed = false;
            }
        }

        /// <summary>
        /// Validates runtime mutation updates and dynamically blocks role assignments linking to disabled tactical groupings.
        /// </summary>
        public void OnChangingRole(LabApi.Events.Arguments.PlayerEvents.PlayerChangingRoleEventArgs ev)
        {
            if (ev?.Player is null || ev?.NewRole is null) return;

            Faction faction = ev.NewRole.GetFaction();
            Logger.Debug(_plugin.Name, $"PlayerChangingRole filter executed for '{ev.Player.Nickname}'. Target Role: {ev.NewRole} (Faction Vector: {faction})", _plugin.Config.Debug);

            if (_plugin.CacheHandler is not null && _plugin.CacheHandler.IsFactionDisabled(faction))
            {
                Logger.Debug(_plugin.Name, $"Mutation blocked: Faction authorization signature '{faction}' denied for active player routing.", _plugin.Config.Debug);
                ev.IsAllowed = false;
            }
        }
        #endregion
    }
}