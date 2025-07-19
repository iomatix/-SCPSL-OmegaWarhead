namespace BetterOmegaWarhead.Core.PlayerUtils
{
    using BetterOmegaWarhead.Core.LoggingUtils;
    using CustomPlayerEffects;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using LabApi.Features.Wrappers;
    using MEC;
    using PlayerRoles;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Contains core player-related coroutine logic requiring plugin context.
    /// </summary>
    public class PlayerMethods
    {
        private readonly Plugin _plugin;
        private readonly PlayerUtility _playerUtility;

        public PlayerMethods(Plugin plugin)
        {
            _plugin = plugin;
            _playerUtility = new PlayerUtility(_plugin);
        }

        /// <summary>
        /// Coroutine handling the helicopter escape sequence for eligible players.
        /// </summary>
        public IEnumerator<float> HandleHelicopterEscape()
        {
            LogHelper.Debug("HandleHelicopterEscape coroutine started.");

            Vector3 helicopterZone = new Vector3(127f, 295.5f, -43f);
            LogHelper.Debug($"Helicopter zone set to: {helicopterZone}");

            DisableFactionSpawn();

            yield return Timing.WaitForSeconds(13.5f);

            LogHelper.Debug("Summoning NTF chopper.");
            Exiled.API.Features.Respawn.SummonNtfChopper();

            yield return Timing.WaitForSeconds(19.0f);

            foreach (Player player in Player.List)
            {
                if (!_playerUtility.IsEligibleForEscape(player, helicopterZone))
                    continue;

                _plugin.CacheHandler.CachePlayerEvacuatedByHelicopter(player);

                player.IsGodModeEnabled = true;
                player.SendHint(_plugin.Config.HelicopterEscape);
                player.EnableEffect<Flashed>(duration: 1.75f);
                player.Position = new Vector3(39f, 1015f, 32f); // Tutorial tower
                player.ClearInventory();
                player.EnableEffect<Ensnared>(duration: 30f);

                yield return Timing.WaitForSeconds(0.75f);
                player.SetRole(RoleTypeId.Spectator, RoleChangeReason.Escaped);

                LogHelper.Debug($"Player {player.Nickname} has escaped by helicopter.");
            }

            LogHelper.Debug("HandleHelicopterEscape coroutine completed.");
        }

        /// <summary>
        /// Disables all spawnable factions except None.
        /// </summary>
        public void DisableFactionSpawn()
        {
            LogHelper.Debug("DisableFactionSpawn called.");

            foreach (SpawnableFaction spawnableFaction in Enum.GetValues(typeof(SpawnableFaction)))
            {
                if (spawnableFaction == SpawnableFaction.None)
                    continue;

                Faction faction = RoleExtensions.GetFaction(spawnableFaction);
                _plugin.CacheHandler.CacheDisabledFaction(faction);
                LogHelper.Debug($"Disabled faction: {faction}");
            }

            LogHelper.Debug("DisableFactionSpawn completed.");
        }

        /// <summary>
        /// Handles player fate on Omega Warhead detonation.
        /// </summary>
        public void HandlePlayersOnNuke()
        {
            LogHelper.Debug("HandlePlayersOnNuke called.");

            foreach (Player player in Player.List)
            {
                bool isInShelter = _playerUtility.IsInShelter(player);
                bool isEvacuated = _plugin.CacheHandler.IsPlayerEvacuatedByHelicopters(player);

                LogHelper.Debug($"Checking {player.Nickname}: In shelter: {isInShelter}, Evacuated: {isEvacuated}");

                if (isInShelter || isEvacuated)
                {
                    LogHelper.Debug($"Saving {player.Nickname}.");
                    _plugin.EventHandler.Coroutines.Add(Timing.RunCoroutine(HandleSavePlayer(player)));
                }
                else
                {
                    LogHelper.Debug($"Killing {player.Nickname} due to Omega Warhead exposure.");
                    player.EnableEffect<Flashed>(duration: 0.75f);
                    player.Damage(15f, "Omega Warhead");
                    player.EnableEffect<Blurred>(duration: 4.75f);
                    player.EnableEffect<Deafened>(duration: 4.75f);
                    player.Kill("Omega Warhead");
                }
            }

            LogHelper.Debug("HandlePlayersOnNuke completed.");
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
}