namespace OmegaWarhead
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using MEC;
    using PlayerRoles;
    using LabApi.Features.Wrappers;
    using LabApi.Features.Enums;
    using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;
    using ServerHandler = LabApi.Events.Handlers.ServerEvents;
    using WarheadHandler = LabApi.Events.Handlers.WarheadEvents;
    using OmegaWarhead.Core.LoggingUtils;
    using OmegaWarhead.NotificationUtils;

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
        public EventHandler(Plugin plugin) => _plugin = plugin;
        #endregion

        #region Operational Hook Registration
        /// <summary>
        /// Binds subsystem listeners against the global LabAPI multi-threaded event hub.
        /// </summary>
        public void RegisterEvents()
        {
            LogHelper.Debug(nameof(EventHandler), "Initializing global event pipeline subscription matrix...");

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

            LogHelper.Debug(nameof(EventHandler), "All core architectural event pathways securely attached.");
        }

        /// <summary>
        /// Detaches subsystem listeners to prevent permanent domain assembly holding during hot-reloads.
        /// </summary>
        public void UnregisterEvents()
        {
            LogHelper.Debug(nameof(EventHandler), "Dismantling event pipeline subscription matrix...");

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

            LogHelper.Debug(nameof(EventHandler), "Global event pathways unlinked successfully.");
        }
        #endregion

        #region Multi-Threaded Process Interceptors
        /// <summary>
        /// Cascades thread termination tags across active coroutine blocks.
        /// </summary>
        private void KillTrackedCoroutines()
        {
            foreach (string tag in Shared.CoroutineTags.AllStaticTags)
            {
                Timing.KillCoroutines(tag);
            }
            LogHelper.Debug(nameof(EventHandler), "All active background coroutines flushed from execution queues via structural tags.");
        }
        #endregion

        #region Core Server Interceptors
        /// <summary>
        /// Resets temporary databases and maps hardware parameters for a new round instantiation.
        /// </summary>
        public void OnRoundStart(LabApi.Events.Arguments.ServerEvents.RoundStartingEventArgs ev)
        {
            LogHelper.Debug(nameof(EventHandler), "RoundStarting sequence captured. Refreshing operational databases...");
            KillTrackedCoroutines();
            _plugin.CacheHandler.ResetCache();
            _plugin.OmegaManager.Init();
        }

        /// <summary>
        /// Clears tracking graphs and isolates resources upon round completion state transition.
        /// </summary>
        public void OnRoundEnd(LabApi.Events.Arguments.ServerEvents.RoundEndedEventArgs ev)
        {
            LogHelper.Debug(nameof(EventHandler), $"RoundEnded sequence captured. Leading Team: {ev.LeadingTeam}. Shutting down sub-engines...");
            KillTrackedCoroutines();
            _plugin.CacheHandler.ResetCache();
            _plugin.OmegaManager.Disable();
        }

        /// <summary>  
        /// Intercepts base-game emergency Dead Man Switch sequences and injects strategic mini-wave mechanics.  
        /// </summary>  
        public void OnWarheadDeadManSwitch(LabApi.Events.Arguments.ServerEvents.DeadmanSequenceActivatingEventArgs ev)
        {
            LogHelper.Debug(nameof(EventHandler), $"DeadmanSequenceActivating interceptor triggered. Configuration Constraint Status: {_plugin.Config.DisableDeadManSwitch}");

            if (!_plugin.Config.DisableDeadManSwitch) return;

            LogHelper.Debug(nameof(EventHandler), "Intercepted base Dead Man sequence. Revoking default engine authorization...");
            ev.IsAllowed = false;

            // Evaluate constraints before initializing combat reinforcing mini-waves
            if (_plugin.Config.TriggerMiniWaveOnDmsCancel && !_plugin.OmegaManager.IsOmegaActive && !_plugin.OmegaManager.IsOmegaDetonated)
            {
                LogHelper.Debug(nameof(EventHandler), "Dead Man failure recovery initiated. Constructing dynamic rescue payload parameters...");

                // Inject strategic reinforcement points directly into active mobile strike forces
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

                NotificationUtility.SendCassieMessage(_plugin.Config.CassieMessageMiniWaveOnDmsCancel);

                if (_plugin.Config.OpenBlastDoorsOnDmsCancel)
                {
                    Warhead.OpenBlastDoors();
                }

                // High-Performance LINQ Faction Extraction Mapping (Replaced extreme procedural text walls)
                int ntfCount = Player.List.Count(p => p.Role.GetFaction() is Faction.FoundationStaff);
                int ciCount = Player.List.Count(p => p.Role.GetFaction() is Faction.FoundationEnemy);

                Faction targetFaction = ntfCount < ciCount ? Faction.FoundationStaff : Faction.FoundationEnemy;
                LogHelper.Debug(nameof(EventHandler), $"Strategic tactical evaluation complete. Target Balance Vector: {targetFaction} (Staff Load: {ntfCount} vs Enemy Load: {ciCount})");

                // Route deployment requests through optimized reinforcement sub-channels
                MiniRespawnWave miniWave = targetFaction is Faction.FoundationStaff ? RespawnWaves.MiniMtfWave : RespawnWaves.MiniChaosWave;

                if (miniWave != null)
                {
                    miniWave.InitiateRespawn();
                    LogHelper.Debug(nameof(EventHandler), $"Tactical vanguard reinforcement deployment wave authorized for faction context: {targetFaction}");
                }
            }
        }
        #endregion

        #region Dedicated Warhead Interceptors
        public void OnWarheadStart(LabApi.Events.Arguments.WarheadEvents.WarheadStartingEventArgs ev)
        {
            LogHelper.Debug(nameof(EventHandler), "Warhead Starting signal hooked. Delegating verification logic to core manager...");
            _plugin.OmegaManager.HandleWarheadStart(ev);
        }

        public void OnWarheadStop(LabApi.Events.Arguments.WarheadEvents.WarheadStoppingEventArgs ev)
        {
            LogHelper.Debug(nameof(EventHandler), "Warhead Stopping signal hooked. Delegating safety abort routing to core manager...");
            _plugin.OmegaManager.HandleWarheadStop(ev);
        }

        public void OnWarheadDetonate(LabApi.Events.Arguments.WarheadEvents.WarheadDetonatingEventArgs ev)
        {
            LogHelper.Debug(nameof(EventHandler), "Warhead Detonating critical threshold reached. Passing priority control to manager matrix...");
            _plugin.OmegaManager.HandleWarheadDetonate(ev);
        }
        #endregion

        #region Spatial Entity Interceptors
        /// <summary>
        /// Filters global deployment pipelines and dynamically vectors spawn allowances for restricted teams.
        /// </summary>
        public void OnWaveRespawning(LabApi.Events.Arguments.ServerEvents.WaveRespawningEventArgs ev)
        {
            Faction faction = ev.Wave.Faction;
            LogHelper.Debug(nameof(EventHandler), $"WaveRespawning process trapped. Analyzing target faction deployment footprint: {faction}");

            if (_plugin.CacheHandler.IsFactionDisabled(faction))
            {
                LogHelper.Debug(nameof(EventHandler), $"Deployment unauthorized: Faction signature '{faction}' is locked out by alternative sequence boundaries.");
                ev.IsAllowed = false;
            }
        }

        /// <summary>
        /// Validates runtime mutation updates and dynamically blocks role assignments linking to disabled tactical groupings.
        /// </summary>
        public void OnChangingRole(LabApi.Events.Arguments.PlayerEvents.PlayerChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;

            Faction faction = ev.NewRole.GetFaction();
            LogHelper.Debug(nameof(EventHandler), $"PlayerChangingRole filter executed for '{ev.Player.Nickname}'. Target Role: {ev.NewRole} (Faction Vector: {faction})");

            if (_plugin.CacheHandler.IsFactionDisabled(faction))
            {
                LogHelper.Debug(nameof(EventHandler), $"Mutation blocked: Faction authorization signature '{faction}' denied for active player routing.");
                ev.IsAllowed = false;
            }
        }
        #endregion
    }
}