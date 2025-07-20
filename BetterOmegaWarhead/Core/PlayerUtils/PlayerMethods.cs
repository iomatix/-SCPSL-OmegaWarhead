namespace BetterOmegaWarhead.Core.PlayerUtils
{
    using BetterOmegaWarhead.Core.EscapeScenarioUtils;
    using BetterOmegaWarhead.Core.LoggingUtils;
    using CustomPlayerEffects;
    using LabApi.Features.Extensions;
    using LabApi.Features.Wrappers;
    using MEC;
    using PlayerRoles;
    using Respawning;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using static BetterOmegaWarhead.Core.PlayerUtils.PlayerUtility;

    /// <summary>  
    /// Contains core player-related coroutine logic requiring plugin context.  
    /// </summary>  
    public class PlayerMethods
    {
        private readonly Plugin _plugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerMethods"/> class.
        /// </summary>
        /// <param name="plugin">Reference to the core plugin instance.</param>
        public PlayerMethods(Plugin plugin)
        {
            _plugin = plugin;
        }

        /// <summary>
        /// Handles a generic escape sequence defined by an <see cref="EscapeScenario"/>.
        /// </summary>
        /// <param name="scenario">The escape scenario to handle.</param>
        /// <returns>An enumerator for coroutine execution.</returns>
        public IEnumerator<float> HandleEscapeSequence(EscapeScenario scenario)
        {
            LogHelper.Debug($"{scenario.Name} escape sequence started.");
            LogHelper.Debug($"Escape zone set to: {scenario.EscapeZone}");

            DisableFactionSpawn();

            // Single loop to handle both hint sending and delayed escape processing  
            foreach (Player player in Player.ReadyList)
            {
                player.SendHint(scenario.InitialMessage, 6.0f);

                float totalDelay = scenario.InitialDelay + scenario.ProcessingDelay;
                Timing.CallDelayed(totalDelay, () => ProcessPlayerEscape(player, scenario));
            }

            yield return Timing.WaitForSeconds(scenario.InitialDelay);

            scenario.OnEscapeTriggered?.Invoke();

            yield return Timing.WaitForSeconds(scenario.ProcessingDelay);
            LogHelper.Debug($"{scenario.Name} escape sequence completed.");
        }

        /// <summary>
        /// Handles the helicopter escape sequence using the generic escape system.
        /// </summary>
        /// <returns>An enumerator for coroutine execution.</returns>
        public IEnumerator<float> HandleHelicopterEscape()
        {
            var helicopterScenario = new EscapeScenario
            {
                Name = "Helicopter",
                EscapeZone = new Vector3(127f, 295.5f, -43f),
                TeleportPosition = new Vector3(39f, 1015f, 32f),
                InitialMessage = _plugin.Config.HelicopterIncomingMessage,
                EscapeMessage = _plugin.Config.HelicopterEscape,
                InitialDelay = 13.5f,
                ProcessingDelay = 19.0f,
                FinalDelay = 0.75f,
                OnEscapeTriggered = () =>
                {
                    LogHelper.Debug("Summoning NTF chopper.");
                    Exiled.API.Features.Respawn.SummonNtfChopper();
                }
            };

            return HandleEscapeSequence(helicopterScenario);
        }

        /// <summary>
        /// Processes the escape logic for a single player.
        /// </summary>
        /// <param name="player">The player to process.</param>
        /// <param name="scenario">The escape scenario defining the behavior.</param>
        private void ProcessPlayerEscape(Player player, EscapeScenario scenario)
        {
            if (!PlayerUtility.IsEligibleForEscape(player, scenario.EscapeZone, _plugin.Config.EscapeZoneSize))
                return;

            // Cache the evacuation based on scenario type  
            if (scenario.Name == "Helicopter")
                _plugin.CacheHandler.CachePlayerEvacuatedByHelicopter(player);

            ApplyEscapeEffects(player, scenario);

            Timing.CallDelayed(scenario.FinalDelay, () =>
            {
                player.SetRole(RoleTypeId.Spectator, RoleChangeReason.Escaped);
                LogHelper.Debug($"Player {player.Nickname} has escaped by {scenario.Name.ToLower()}.");
            });
        }

        /// <summary>
        /// Applies teleportation, effects, and hint message during player escape.
        /// </summary>
        /// <param name="player">The escaping player.</param>
        /// <param name="scenario">The scenario context.</param>
        private void ApplyEscapeEffects(Player player, EscapeScenario scenario)
        {
            player.IsGodModeEnabled = true;
            player.SendHint(scenario.EscapeMessage);
            player.EnableEffect<Flashed>(duration: 1.75f);
            player.Position = scenario.TeleportPosition;
            player.ClearInventory();
            player.EnableEffect<Ensnared>(duration: 30f);
        }

        /// <summary>
        /// Disables all enemy faction spawns (e.g., NTF, Chaos) to prevent reinforcement after an escape event.
        /// </summary>
        public void DisableFactionSpawn()
        {
            LogHelper.Debug("DisableFactionSpawn called.");

            var spawnableRoles = new[]
            {
                RoleTypeId.NtfCaptain,
                RoleTypeId.NtfSergeant,
                RoleTypeId.NtfSpecialist,
                RoleTypeId.NtfPrivate,
                RoleTypeId.ChaosConscript,
                RoleTypeId.ChaosMarauder,
                RoleTypeId.ChaosRepressor,
                RoleTypeId.ChaosRifleman
            };

            foreach (var role in spawnableRoles)
            {
                Team team = role.GetTeam();
                Faction faction;

                switch (team)
                {
                    case Team.FoundationForces:
                        faction = Faction.FoundationStaff;
                        break;
                    case Team.ChaosInsurgency:
                        faction = Faction.FoundationEnemy;
                        break;
                    default:
                        faction = Faction.Unclassified;
                        break;
                }

                if (faction != Faction.Unclassified)
                {
                    FactionInfluenceManager.Set(faction, 0f);
                    _plugin.CacheHandler.CacheDisabledFaction(faction);
                    LogHelper.Debug($"Disabled faction: {faction} for role: {role}");
                }
            }

            LogHelper.Debug("DisableFactionSpawn completed.");
        }

        /// <summary>
        /// Evaluates and applies consequences to all players when the Omega Warhead detonates.
        /// </summary>
        public void HandlePlayersOnNuke()
        {
            LogHelper.Debug("HandlePlayersOnNuke called.");

            foreach (Player player in Player.ReadyList)
            {
                PlayerFate fate = DeterminePlayerFate(player);
                ProcessPlayerFate(player, fate);
            }

            LogHelper.Debug("HandlePlayersOnNuke completed.");
        }

        /// <summary>
        /// Determines whether the player is saved or killed during the nuke event.
        /// </summary>
        /// <param name="player">The player to evaluate.</param>
        /// <returns>The fate of the player.</returns>
        private PlayerFate DeterminePlayerFate(Player player)
        {
            bool isInShelter = PlayerUtility.IsInShelter(player, _plugin.Config.ShelterZoneSize);
            bool isEvacuated = _plugin.CacheHandler.IsPlayerEvacuatedByHelicopters(player);

            LogHelper.Debug($"Checking {player.Nickname}: In shelter: {isInShelter}, Evacuated: {isEvacuated}");

            return (isInShelter || isEvacuated) ? PlayerFate.Saved : PlayerFate.Killed;
        }

        /// <summary>
        /// Applies the result of a player's fate based on the determined value.
        /// </summary>
        /// <param name="player">The affected player.</param>
        /// <param name="fate">The fate of the player (Saved/Killed).</param>
        private void ProcessPlayerFate(Player player, PlayerFate fate)
        {
            switch (fate)
            {
                case PlayerFate.Saved:
                    LogHelper.Debug($"Saving {player.Nickname}.");
                    _plugin.EventHandler.Coroutines.Add(Timing.RunCoroutine(HandleSavePlayer(player)));
                    break;

                case PlayerFate.Killed:
                    LogHelper.Debug($"Killing {player.Nickname} due to Omega Warhead exposure.");
                    ApplyNukeEffects(player);
                    break;
            }
        }

        /// <summary>
        /// Applies audio-visual effects to simulate nuke exposure and kills the player.
        /// </summary>
        /// <param name="player">The player to affect.</param>
        private void ApplyNukeEffects(Player player)
        {
            player.EnableEffect<Flashed>(duration: 0.75f);
            player.Damage(15f, "Omega Warhead");
            player.EnableEffect<Blurred>(duration: 4.75f);
            player.EnableEffect<Deafened>(duration: 4.75f);
            player.Kill("Omega Warhead");
        }

        /// <summary>
        /// Coroutine that visually affects and briefly saves the player from death.
        /// </summary>
        /// <param name="player">The player to save.</param>
        /// <returns>An enumerator for coroutine execution.</returns>
        private IEnumerator<float> HandleSavePlayer(Player player)
        {
            LogHelper.Debug($"HandleSavePlayer coroutine started for {player.Nickname}.");

            player.IsGodModeEnabled = true;
            player.EnableEffect<Flashed>(duration: 0.75f);

            yield return Timing.WaitForSeconds(0.75f);

            player.IsGodModeEnabled = false;
            player.EnableEffect<Blindness>(duration: 2.5f);
            player.EnableEffect<Blurred>(duration: 4.75f);
            player.EnableEffect<Deafened>(duration: 4.75f);

            LogHelper.Debug($"HandleSavePlayer coroutine completed for {player.Nickname}.");
        }
    }

    /// <summary>
    /// Represents the outcome of a player during a critical event.
    /// </summary>
    public enum PlayerFate
    {
        Saved,
        Killed
    }
}