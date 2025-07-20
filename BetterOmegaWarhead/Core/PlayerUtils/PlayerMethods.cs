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

        public PlayerMethods(Plugin plugin)
        {
            _plugin = plugin;
        }

        /// <summary>  
        /// Generic escape handler that can process different escape scenarios.  
        /// </summary>  
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
        /// Helicopter escape using the generic escape system.  
        /// </summary>  
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
        /// Generic player escape processing.  
        /// </summary>  
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
        /// Applies standard escape effects to a player.  
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

        /// <summary>  
        /// Disables all spawnable factions except None.  
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
        /// Handles player fate on Omega Warhead detonation.  
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
        /// Determines a player's fate during nuke detonation.  
        /// </summary>  
        private PlayerFate DeterminePlayerFate(Player player)
        {
            bool isInShelter = PlayerUtility.IsInShelter(player, _plugin.Config.ShelterZoneSize);
            bool isEvacuated = _plugin.CacheHandler.IsPlayerEvacuatedByHelicopters(player);

            LogHelper.Debug($"Checking {player.Nickname}: In shelter: {isInShelter}, Evacuated: {isEvacuated}");

            return (isInShelter || isEvacuated) ? PlayerFate.Saved : PlayerFate.Killed;
        }

        /// <summary>  
        /// Processes the determined fate for a player.  
        /// </summary>  
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
        /// Applies nuke exposure effects and kills the player.  
        /// </summary>  
        private void ApplyNukeEffects(Player player)
        {
            player.EnableEffect<Flashed>(duration: 0.75f);
            player.Damage(15f, "Omega Warhead");
            player.EnableEffect<Blurred>(duration: 4.75f);
            player.EnableEffect<Deafened>(duration: 4.75f);
            player.Kill("Omega Warhead");
        }

        /// <summary>  
        /// Coroutine that temporarily saves a player from death by applying effects and God Mode.  
        /// </summary>  
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
    /// Represents possible player fates during events.  
    /// </summary>  
    public enum PlayerFate
    {
        Saved,
        Killed
    }
}