using LabApi.Extensions;
using LabApi.Extensions.Misc;
using LabApi.Features.Wrappers;
using MEC;
using OmegaWarhead.Shared;
using PlayerRoles;
using Respawning;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = LabApi.Extensions.Misc.iLogger;

namespace OmegaWarhead.Core.PlayerUtils
{
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
        public PlayerMethods(Plugin plugin) => _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        #endregion

        #region Native Escape Sequence Core
        /// <summary>
        /// Configures and initiates the cinematic tactical helicopter extraction wave using the Fluent API Escape Engine.
        /// </summary>
        public IEnumerator<float> HandleHelicopterEvacuation()
        {
            // FIXED: Decoupled hardcoded positions and routed parameters straight through the plugin Config data contract
            var helicopterScenario = new LabApi.Extensions.Escape.EscapeScenario
            {
                Name = "Helicopter",
                EscapeZone = _plugin.Config.HelicopterEscapeZone,
                EscapeRadius = _plugin.Config.EscapeZoneSize,
                TeleportPosition = _plugin.Config.HelicopterTeleportPosition,

                InitialHint = _plugin.Config.HelicopterIncomingMessage,
                InitialHintDuration = 6f,
                SuccessHint = _plugin.Config.HelicopterEscapeMessage,

                InitialDelay = 13.5f,
                ProcessingDelay = 19.0f,
                FinalDelay = 0.75f,

                FlashDuration = 1.75f,
                DeafenDuration = 30f,

                OnSequenceStarted = () => DisableFactionSpawn(),
                OnSequenceProcessing = () =>
                {
                    RespawnWaves.PrimaryMtfWave?.PlayRespawnEffect();
                },
                OnPlayerValidated = (player) =>
                {
                    _plugin.CacheHandler?.CachePlayerEvacuatedByHelicopter(player);
                }
            };

            return LabApi.Extensions.Escape.EscapeEngine.RunScenarioRoutine(helicopterScenario);
        }
        #endregion

        #region Faction Restriction Management
        /// <summary>
        /// Locks reinforcement capability indicators to block dead units from altering endgame metrics.
        /// </summary>
        public void DisableFactionSpawn()
        {
            Logger.Debug(nameof(PlayerMethods), "Initializing systemic military deployment lockouts...", _plugin.Config.Debug);

            Faction[] factionsToDisable = { Faction.FoundationStaff, Faction.FoundationEnemy };

            foreach (Faction faction in factionsToDisable)
            {
                FactionInfluenceManager.Set(faction, 0f);
                _plugin.CacheHandler.CacheDisabledFaction(faction);
                Logger.Debug(nameof(PlayerMethods), $"Deployment authorization vectors permanently revoked for tactical grouping: [{faction}].", _plugin.Config.Debug);
            }

            Logger.Debug(nameof(PlayerMethods), "Systemic deployment lockouts fully engaged.", _plugin.Config.Debug);
        }
        #endregion

        #region Warhead Blast Allocation & Fate Calculation
        /// <summary>
        /// Loops across active unit maps during atomic compression to trigger consequences or shelter states.
        /// </summary>
        public void HandlePlayersOnNuke()
        {
            Logger.Debug(nameof(PlayerMethods), "Thermal blast calculations initiated across global entity array...", _plugin.Config.Debug);

            foreach (Player player in Player.ReadyList)
            {
                if (player?.IsReady != true) continue;

                PlayerFate fate = DeterminePlayerFate(player);
                ProcessPlayerFate(player, fate);
            }

            Logger.Debug(nameof(PlayerMethods), "Thermal blast wave propagation simulation completed.", _plugin.Config.Debug);
        }

        /// <summary>
        /// Audits positioning records and returns absolute categorical outcome records for an entity.
        /// </summary>
        private PlayerFate DeterminePlayerFate(Player player)
        {
            var shelterLocations = _plugin.CacheHandler.GetCachedShelterLocations();
            bool isInShelter = player.IsInShelter(_plugin.Config.ShelterZoneSize, shelterLocations);
            bool isEvacuated = _plugin.CacheHandler.IsPlayerEvacuatedByHelicopters(player);

            Logger.Debug(nameof(PlayerMethods), $"Diagnostics for '{player.Nickname}': Structural Shelter Protected = {isInShelter}, Air Evacuated = {isEvacuated}", _plugin.Config.Debug);

            if (isInShelter) return PlayerFate.SurvivedShelter;
            if (isEvacuated) return PlayerFate.EvacuatedByHelicopter;

            return PlayerFate.KilledByWarhead;
        }

        /// <summary>
        /// Routes target entities into execution threads or damage pipelines based on fate evaluation.
        /// </summary>
        private void ProcessPlayerFate(Player player, PlayerFate fate)
        {
            _plugin.CacheHandler?.CachePlayerFate(player, fate);

            switch (fate)
            {
                case PlayerFate.SurvivedShelter:
                case PlayerFate.EvacuatedByHelicopter:
                    Logger.Debug(nameof(PlayerMethods), $"Redirecting survivor client '{player.Nickname}' into invulnerability thread loop. Outcome: [{fate}]", _plugin.Config.Debug);

                    string saveTag = $"{CoroutineTags.SavePlayerPrefix}{player.UserId}";
                    saveTag.KillCoroutine();

                    Timing.RunCoroutine(HandleSavePlayer(player), saveTag);
                    break;

                case PlayerFate.KilledByWarhead:
                    Logger.Debug(nameof(PlayerMethods), $"Vaporizing unshielded entity '{player.Nickname}' due to core terminal exposure. Outcome: [{fate}]", _plugin.Config.Debug);
                    ApplyNukeEffects(player);
                    break;

                default:
                    Logger.Warn(nameof(PlayerMethods), $"Anomalous outcome calculated for entity profile '{player.Nickname}': [{fate}]");
                    break;
            }
        }

        /// <summary>
        /// Triggers full visual flash rendering blocks and terminates entity processes via continuous action chains.
        /// </summary>
        private static void ApplyNukeEffects(Player player)
        {
            player.CreateChain()
                .Then(p =>
                {
                    p.ApplySensoryShock(flashDuration: 0.75f, blindDuration: 1.25f, blurDuration: 15.75f, deafenDuration: 15.75f);
                    p.Damage(15f, "Omega Warhead");
                })
                .Wait(0.75f)
                .Then(p => p.Kill("Omega Warhead"))
                .Run(CoroutineTags.Detonation);
        }

        /// <summary>
        /// Manages transient post-blast stabilization timelines for shelter survivors.
        /// </summary>
        private IEnumerator<float> HandleSavePlayer(Player player)
        {
            if (player is null || string.IsNullOrEmpty(player.UserId)) yield break;

            string userId = player.UserId;
            _activeSaveCoroutines.Add(userId);

            try
            {
                Logger.Debug(nameof(PlayerMethods), $"Invulnerability protection window opened for client: '{player.Nickname}'.", _plugin.Config.Debug);

                player.IsGodModeEnabled = true;
                player.EnableEffect(FacilityEffectType.Flashed, intensity: 1, duration: 0.75f);

                yield return Timing.WaitForSeconds(0.75f);

                if (player?.IsReady == true && player.IsAlive)
                {
                    player.IsGodModeEnabled = false;
                    player.EnableEffect(FacilityEffectType.Blindness, intensity: 1, duration: 2.75f);
                    player.EnableEffect(FacilityEffectType.Blurred, intensity: 1, duration: 5.75f);
                    player.EnableEffect(FacilityEffectType.Deafened, intensity: 1, duration: 15.75f);
                }

                Logger.Debug(nameof(PlayerMethods), $"Invulnerability protection window closed cleanly for client: '{player?.Nickname}'.", _plugin.Config.Debug);
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

            if (_plugin.CacheHandler is null) return;

            foreach (var (player, fate) in _plugin.CacheHandler.GetCachedPlayerFates())
            {
                if (player?.IsReady != true) continue;

                player.ApplySensoryShock(flashDuration: 1.75f, blindDuration: 0f, blurDuration: 15.75f, deafenDuration: 15.75f);

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

            string endingCode = SafeRandom.Range(1000f, 99999f).RoundToInt().ToString();
            string formattedBroadcast = _plugin.Config.EndingBroadcast
                .Replace("{survived}", survived.ToString())
                .Replace("{escaped}", escaped.ToString())
                .Replace("{dead}", dead.ToString())
                .Replace("{code}", endingCode);

            var coroutine = Timing.CallDelayed(10f, () =>
            {
                PlayerExtensions.BroadcastHintToAll(formattedBroadcast, duration: 15f);
            });
            coroutine.Tag = CoroutineTags.Scenario;
        }
        #endregion

        #region Historical Outcome Definitions
        public enum PlayerFate
        {
            EvacuatedByHelicopter,
            SurvivedShelter,
            KilledByWarhead,
            Unknown
        }
        #endregion

        #region Dynamic Resource Reclamation
        public void Clean()
        {
            foreach (var userId in _activeSaveCoroutines.ToList())
            {
                $"{CoroutineTags.SavePlayerPrefix}{userId}".KillCoroutine();
            }
            _activeSaveCoroutines.Clear();
            Logger.Debug(nameof(PlayerMethods), "All local dynamic player execution sequences successfully evacuated.", _plugin.Config.Debug);
        }
        #endregion
    }
}