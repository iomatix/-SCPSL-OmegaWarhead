namespace OmegaWarhead.Core.PlayerUtils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using MEC;
    using PlayerRoles;
    using Respawning;
    using CustomPlayerEffects;
    using LabApi.Features.Extensions;
    using LabApi.Features.Wrappers;
    using OmegaWarhead.Core.EscapeScenarioUtils;
    using OmegaWarhead.Core.LoggingUtils;
    using OmegaWarhead.Shared;
    using static OmegaWarhead.Core.PlayerUtils.PlayerUtility;

    /// <summary>  
    /// Coordinates high-performance player structural states, air evacuation lifecycles, 
    /// and blast exposure fate calculations inside the LabAPI framework.
    /// </summary>  
    public class PlayerMethods
    {
        #region Private Repositories
        private readonly Plugin _plugin;
        private readonly HashSet<string> _activeSaveCoroutines = new();
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerMethods"/> class linked to a parent plugin context.
        /// </summary>
        public PlayerMethods(Plugin plugin) => _plugin = plugin;
        #endregion

        #region Generic Escape Sequence Core
        /// <summary>
        /// Handles a generic escape sequence defined by an <see cref="EscapeScenario"/>.
        /// </summary>
        public IEnumerator<float> HandleEscapeSequence(EscapeScenario scenario)
        {
            LogHelper.Debug(nameof(PlayerMethods), $"{scenario.Name} escape sequence initiated.");
            LogHelper.Debug(nameof(PlayerMethods), $"Target spatial extraction coordinates set to: {scenario.EscapeZone}");

            DisableFactionSpawn();

            // Broadcast the initial hint to all active players seamlessly
            foreach (Player player in Player.ReadyList)
            {
                if (player?.IsReady == true)
                {
                    player.SendHint(scenario.InitialMessage, 6.0f);
                }
            }

            // Stall execution pipeline for entry vehicles (e.g., helicopter travel animation time)
            yield return Timing.WaitForSeconds(scenario.InitialDelay);

            // Execute tactical physics event binding
            scenario.OnEscapeTriggered?.Invoke();

            // Stall execution pipeline for boarding structures (e.g., deployment time)
            yield return Timing.WaitForSeconds(scenario.ProcessingDelay);

            // Process extraction bounds checking for all units in a single linear sweep
            foreach (Player player in Player.ReadyList)
            {
                if (player?.IsReady == true && player.IsAlive)
                {
                    ProcessPlayerEscape(player, scenario);
                }
            }

            LogHelper.Debug(nameof(PlayerMethods), $"{scenario.Name} escape sequence processing completed successfully.");
        }

        /// <summary>
        /// Configures and executes the cinematic tactical helicopter extraction wave.
        /// </summary>
        public IEnumerator<float> HandleHelicopterEvacuation()
        {
            var helicopterScenario = new EscapeScenario
            {
                Name = "Helicopter",
                EscapeZone = new Vector3(127f, 295.5f, -43f),
                TeleportPosition = new Vector3(39f, 1015f, 32f),
                InitialMessage = _plugin.Config.HelicopterIncomingMessage,
                EscapeMessage = _plugin.Config.HelicopterEscapeMessage,
                InitialDelay = 13.5f,
                ProcessingDelay = 19.0f,
                FinalDelay = 0.75f,
                OnEscapeTriggered = () =>
                {
                    LogHelper.Debug(nameof(PlayerMethods), "Dispatching target NTF air reinforcement vehicle to surface sector...");
                    RespawnWaves.PrimaryMtfWave?.PlayRespawnEffect();
                }
            };

            return HandleEscapeSequence(helicopterScenario);
        }

        /// <summary>
        /// Evaluates positioning matrices to determine single player extraction validity.
        /// </summary>
        private void ProcessPlayerEscape(Player player, EscapeScenario scenario)
        {
            if (!PlayerUtility.IsEligibleForEscape(player, scenario.EscapeZone, _plugin.Config.EscapeZoneSize))
                return;

            // Securely commit extraction profile states into transient databases
            if (scenario.Name is "Helicopter")
            {
                _plugin.CacheHandler.CachePlayerEvacuatedByHelicopter(player);
            }

            ApplyEscapeEffects(player, scenario);

            var coroutine = Timing.CallDelayed(scenario.FinalDelay, () =>
            {
                // Verify entity connection persistence before mutating game roles
                if (player?.IsReady == true)
                {
                    player.SetRole(RoleTypeId.Spectator, RoleChangeReason.Escaped);
                    LogHelper.Debug(nameof(PlayerMethods), $"Player '{player.Nickname}' successfully committed to server Spectator state via extraction scenario: [{scenario.Name}].");
                }
            });
            coroutine.Tag = CoroutineTags.Escape;
        }

        /// <summary>
        /// Sets invulnerability hooks and shifts player layout fields post-extraction.
        /// </summary>
        private void ApplyEscapeEffects(Player player, EscapeScenario scenario)
        {
            player.IsGodModeEnabled = true;
            player.SendHint(scenario.EscapeMessage);
            player.EnableEffect<Flashed>(duration: 1.75f);
            player.Position = scenario.TeleportPosition;
            player.ClearInventory();
            player.EnableEffect<Ensnared>(duration: 30f);
        }
        #endregion

        #region Faction Restriction Management
        /// <summary>
        /// Locks reinforcement capability indicators to block dead units from altering endgame metrics.
        /// </summary>
        public void DisableFactionSpawn()
        {
            LogHelper.Debug(nameof(PlayerMethods), "Initializing systemic military deployment lockouts...");

            Faction[] factionsToDisable = { Faction.FoundationStaff, Faction.FoundationEnemy };

            foreach (Faction faction in factionsToDisable)
            {
                FactionInfluenceManager.Set(faction, 0f);
                _plugin.CacheHandler.CacheDisabledFaction(faction);
                LogHelper.Debug(nameof(PlayerMethods), $"Deployment authorization vectors permanently revoked for tactical grouping: [{faction}].");
            }

            LogHelper.Debug(nameof(PlayerMethods), "Systemic deployment lockouts fully engaged.");
        }
        #endregion

        #region Ghead Blast Allocation & Fate Calculation
        /// <summary>
        /// Loops across active unit maps during atomic compression to trigger consequences or shelter states.
        /// </summary>
        public void HandlePlayersOnNuke()
        {
            LogHelper.Debug(nameof(PlayerMethods), "Thermal blast calculations initiated across global entity array...");

            foreach (Player player in Player.ReadyList)
            {
                // Defensive structural guard to avoid memory tracking on disconnected client artifacts
                if (player?.IsReady != true) continue;

                PlayerFate fate = DeterminePlayerFate(player);
                ProcessPlayerFate(player, fate);
            }

            LogHelper.Debug(nameof(PlayerMethods), "Thermal blast wave propagation simulation completed.");
        }

        /// <summary>
        /// Audits positioning records and returns absolute categorical outcome records for an entity.
        /// </summary>
        private PlayerFate DeterminePlayerFate(Player player)
        {
            bool isInShelter = PlayerUtility.IsInShelter(player, _plugin.Config.ShelterZoneSize);
            bool isEvacuated = _plugin.CacheHandler.IsPlayerEvacuatedByHelicopters(player);

            LogHelper.Debug(nameof(PlayerMethods), $"Diagnostics for '{player.Nickname}': Structural Shelter Protected = {isInShelter}, Air Evacuated = {isEvacuated}");

            if (isInShelter) return PlayerFate.SurvivedShelter;
            if (isEvacuated) return PlayerFate.EvacuatedByHelicopter;

            return PlayerFate.KilledByWarhead;
        }

        /// <summary>
        /// Routes target entities into execution threads or damage pipelines based on fate evaluation.
        /// </summary>
        private void ProcessPlayerFate(Player player, PlayerFate fate)
        {
            _plugin.CacheHandler.CachePlayerFate(player, fate);

            switch (fate)
            {
                case PlayerFate.SurvivedShelter:
                case PlayerFate.EvacuatedByHelicopter:
                    LogHelper.Debug(nameof(PlayerMethods), $"Redirecting survivor client '{player.Nickname}' into invulnerability thread loop. Outcome: [{fate}]");

                    string saveTag = $"{Shared.CoroutineTags.SavePlayerPrefix}{player.UserId}";
                    Timing.KillCoroutines(saveTag); // Mitigate visual effect stack overlaps
                    Timing.RunCoroutine(HandleSavePlayer(player), saveTag);
                    break;

                case PlayerFate.KilledByWarhead:
                    LogHelper.Debug(nameof(PlayerMethods), $"Vaporizing unshielded entity '{player.Nickname}' due to core terminal exposure. Outcome: [{fate}]");
                    ApplyNukeEffects(player);
                    break;

                default:
                    LogHelper.Warning(nameof(PlayerMethods), $"Anomalous outcome calculated for entity profile '{player.Nickname}': [{fate}]");
                    break;
            }
        }

        /// <summary>
        /// Triggers full visual flash rendering blocks and terminates entity processes.
        /// </summary>
        private void ApplyNukeEffects(Player player)
        {
            player.EnableEffect<Flashed>(duration: 0.75f);
            player.EnableEffect<Blindness>(duration: 1.25f);
            player.Damage(15f, "Omega Warhead");
            player.EnableEffect<Blurred>(duration: 15.75f);
            player.EnableEffect<Deafened>(duration: 15.75f);
            player.Kill("Omega Warhead");
        }

        /// <summary>
        /// Manages transient post-blast stabilization timelines for shelter survivors.
        /// </summary>
        private IEnumerator<float> HandleSavePlayer(Player player)
        {
            if (player == null || string.IsNullOrEmpty(player.UserId)) yield break;

            string userId = player.UserId;
            _activeSaveCoroutines.Add(userId);

            try
            {
                LogHelper.Debug(nameof(PlayerMethods), $"Invulnerability protection window opened for client: '{player.Nickname}'.");

                player.IsGodModeEnabled = true;
                player.EnableEffect<Flashed>(duration: 0.75f);

                yield return Timing.WaitForSeconds(0.75f);

                // Re-verify instance and client session health after temporal suspension delay
                if (player?.IsReady == true && player.IsAlive)
                {
                    player.IsGodModeEnabled = false;
                    player.EnableEffect<Blindness>(duration: 2.75f);
                    player.EnableEffect<Blurred>(duration: 5.75f);
                    player.EnableEffect<Deafened>(duration: 15.75f);
                }

                LogHelper.Debug(nameof(PlayerMethods), $"Invulnerability protection window closed cleanly for client: '{player?.Nickname}'.");
            }
            finally
            {
                _activeSaveCoroutines.Remove(userId);
            }
        }
        #endregion

        #region Statistical Summary Displays
        /// <summary>
        /// Aggregates historical metrics from the round and formats custom post-apocalyptic summary layouts.
        /// </summary>
        public void HandleEndingByFate()
        {
            ushort survived = 0, escaped = 0, dead = 0;

            foreach (var entry in _plugin.CacheHandler.GetCachedPlayerFates())
            {
                Player player = entry.Key;
                PlayerFate fate = entry.Value;

                // CRITICAL FIXED DEFENSIVE GUARD: Avoid calling methods on disconnected/null player references
                if (player?.IsReady != true) continue;

                player.EnableEffect<Flashed>(duration: 1.75f);
                player.EnableEffect<Blurred>(duration: 15.75f);
                player.EnableEffect<Deafened>(duration: 15.75f);

                switch (fate)
                {
                    case PlayerFate.EvacuatedByHelicopter:
                        player.SendHint(_plugin.Config.EvacuatedMessage, 10f);
                        escaped++;
                        break;
                    case PlayerFate.SurvivedShelter:
                        player.SendHint(_plugin.Config.SurvivorMessage, 10f);
                        survived++;
                        break;
                    case PlayerFate.KilledByWarhead:
                        player.SendHint(_plugin.Config.KilledMessage, 10f);
                        dead++;
                        break;
                }
            }

            // String Reallocation Optimization: Pre-format broadcast content structure outside the array loop
            string endingCode = UnityEngine.Random.Range(1000, 99999).ToString();
            string formattedBroadcast = _plugin.Config.EndingBroadcast
                .Replace("{survived}", survived.ToString())
                .Replace("{escaped}", escaped.ToString())
                .Replace("{dead}", dead.ToString())
                .Replace("{code}", endingCode);

            var coroutine = Timing.CallDelayed(10f, () =>
            {
                foreach (Player player in Player.ReadyList)
                {
                    if (player?.IsReady == true)
                    {
                        player.SendHint(formattedBroadcast, duration: 15f);
                    }
                }
            });
            coroutine.Tag = CoroutineTags.Scenario;
        }
        #endregion

        #region Historical Outcome Definitions
        /// <summary>
        /// Defines absolute categorical outcomes for an entity inside an atomic event environment.
        /// </summary>
        public enum PlayerFate
        {
            EvacuatedByHelicopter,
            SurvivedShelter,
            KilledByWarhead,
            Unknown
        }
        #endregion

        #region Dynamic Resource Reclamation
        /// <summary>
        /// Flushes tracking lists and explicitly terminates survivor tracking threads.
        /// </summary>
        public void Clean()
        {
            foreach (var userId in _activeSaveCoroutines.ToList())
            {
                Timing.KillCoroutines($"{Shared.CoroutineTags.SavePlayerPrefix}{userId}");
            }
            _activeSaveCoroutines.Clear();
            LogHelper.Debug(nameof(PlayerMethods), "All local dynamic player execution sequences successfully evacuated.");
        }
        #endregion
    }
}