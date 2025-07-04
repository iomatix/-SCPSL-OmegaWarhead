namespace BetterOmegaWarhead
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using MEC;
    using PlayerRoles;
    using Respawning.Waves;
    using UnityEngine;
    using UnityEngine.Assertions.Must;

    public class PlayerMethods
    {
        private readonly Plugin _plugin;
        public PlayerMethods(Plugin plugin) => _plugin = plugin;

        public IEnumerator<float> HandleHelicopterEscape()
        {
            Log.Debug("HandleHelicopterEscape coroutine started.");
            Vector3 helicopterZone = new Vector3(127f, 295.5f, -43f);
            Log.Debug($"Helicopter zone set to: {helicopterZone}");

            DisableFactionSpawn();
            Log.Debug("Waiting 12s for initial delay.");
            yield return Timing.WaitForSeconds(12.0f);
            Log.Debug("Waiting additional 1.5s before summoning chopper.");
            yield return Timing.WaitForSeconds(1.5f);
            Log.Debug("Summoning NTF chopper.");
            Respawn.SummonNtfChopper();
            Log.Debug("Waiting 19s for chopper arrival.");
            yield return Timing.WaitForSeconds(19.0f);

            foreach (Player player in Player.List)
            {
                if (!player.IsScp && player.IsAlive && Vector3.Distance(player.Position, helicopterZone) <= _plugin.Config.HelicopterZoneSize)
                {
                    Log.Debug($"Player {player.Nickname} is eligible for helicopter escape (not SCP, alive, within {Vector3.Distance(player.Position, helicopterZone)} of zone).");
                    _plugin.CacheHandlers.CachePlayerEvacuatedByHelicopter(player);
                    player.IsGodModeEnabled = true;
                    Log.Debug($"Enabled god mode for {player.Nickname}.");
                    player.Broadcast(_plugin.Config.HelicopterEscape);
                    Log.Debug($"Broadcasted escape message to {player.Nickname}.");
                    player.EnableEffect(EffectType.Flashed, 1.75f);
                    Log.Debug($"Applied Flashed effect to {player.Nickname} for 1.75s.");
                    Vector3 towerPos = new Vector3(39f, 1015f, 32f);
                    player.Position = towerPos;
                    Log.Debug($"Moved {player.Nickname} to tutorial tower: {towerPos}.");
                    player.ClearInventory();
                    Log.Debug($"Cleared inventory for {player.Nickname}.");
                    player.EnableEffect(EffectType.Ensnared);
                    Log.Debug($"Applied Ensnared effect to {player.Nickname}.");

                    if (player.LeadingTeam == LeadingTeam.FacilityForces)
                    {
                        Round.EscapedScientists++;
                        Log.Debug($"Incremented EscapedScientists for {player.Nickname} (FacilityForces).");
                    }
                    else if (player.LeadingTeam == LeadingTeam.ChaosInsurgency)
                    {
                        Round.EscapedDClasses++;
                        Log.Debug($"Incremented EscapedDClasses for {player.Nickname} (ChaosInsurgency).");
                    }

                    yield return Timing.WaitForSeconds(0.75f);
                    Log.Debug($"Setting {player.Nickname} to Spectator role due to escape.");
                    player.Role.Set(RoleTypeId.Spectator, reason: SpawnReason.Escaped);
                }
            }
            Log.Debug("HandleHelicopterEscape coroutine completed.");
        }

        public void DisableFactionSpawn()
        {
            Log.Debug("DisableFactionSpawn called.");
            foreach (SpawnableFaction spawnableFaction in Enum.GetValues(typeof(SpawnableFaction)))
            {
                if(spawnableFaction == SpawnableFaction.None) continue;
                Faction faction = RoleExtensions.GetFaction(spawnableFaction);
                Log.Debug($"Disabling faction: {faction} (from SpawnableFaction: {spawnableFaction}).");
                _plugin.CacheHandlers.CacheDisabledFaction(faction);
            }
            Log.Debug("DisableFactionSpawn completed.");
        }

        public void HandlePlayersOnNuke()
        {
            Log.Debug("HandlePlayersOnNuke called.");
            foreach (Player player in Player.List)
            {
                bool isInShelter = IsInShelter(player);
                bool isEvacuated = _plugin.CacheHandlers.IsPlayerEvacuatedByHelicopters(player);
                Log.Debug($"Checking {player.Nickname}: In shelter: {isInShelter}, Evacuated: {isEvacuated}");

                if (isInShelter || isEvacuated)
                {
                    Log.Debug($"Saving {player.Nickname} (in shelter or evacuated).");
                    _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(HandleSavePlayer(player)));
                }
                else
                {
                    Log.Debug($"Killing {player.Nickname} due to Omega Warhead exposure.");
                    player.EnableEffect(EffectType.Flashed, 0.75f);
                    Log.Debug($"Applied Flashed effect to {player.Nickname} for 0.75s.");
                    player.Hurt(amount: 0.15f, damageType: DamageType.Explosion);
                    Log.Debug($"Applied 0.15 damage to {player.Nickname} (Explosion).");
                    player.EnableEffect(EffectType.Blurred, 4.75f);
                    Log.Debug($"Applied Blurred effect to {player.Nickname} for 4.75s.");
                    player.Kill("Omega Warhead");
                    Log.Debug($"{player.Nickname} killed by Omega Warhead.");
                }
            }
            Log.Debug("HandlePlayersOnNuke completed.");
        }

        public bool IsInShelter(Player player)
        {
            Log.Debug($"Checking if {player.Nickname} is in shelter.");
            if (player.CurrentRoom != null && player.CurrentRoom.Type == RoomType.EzShelter)
            {
                Log.Debug($"{player.Nickname} is in EzShelter room.");
                return true;
            }

            var shelters = _plugin.CacheHandlers.GetCachedShelterLocations();
            Log.Debug($"Checking {shelters.Count} shelter locations for {player.Nickname}.");
            foreach (Vector3 shelterPos in shelters)
            {
                float distance = Vector3.Distance(player.Position, shelterPos);
                if (distance <= _plugin.Config.ShelterZoneSize)
                {
                    Log.Debug($"{player.Nickname} is within {distance} of shelter at {shelterPos}, within zone size {_plugin.Config.ShelterZoneSize}.");
                    return true;
                }
            }
            Log.Debug($"{player.Nickname} is not in a shelter.");
            return false;
        }

        public IEnumerator<float> HandleSavePlayer(Player player)
        {
            Log.Debug($"HandleSavePlayer coroutine started for {player.Nickname}.");
            player.IsGodModeEnabled = true;
            Log.Debug($"Enabled god mode for {player.Nickname}.");
            player.EnableEffect(EffectType.Flashed, 0.75f);
            Log.Debug($"Applied Flashed effect to {player.Nickname} for 0.75s.");
            yield return Timing.WaitForSeconds(0.75f);
            player.IsGodModeEnabled = false;
            Log.Debug($"Disabled god mode for {player.Nickname}.");
            player.EnableEffect(EffectType.Blinded, 2.5f);
            Log.Debug($"Applied Blinded effect to {player.Nickname} for 2.5s.");
            player.EnableEffect(EffectType.Blurred, 4.75f);
            Log.Debug($"Applied Blurred effect to {player.Nickname} for 4.75s.");
            Log.Debug($"HandleSavePlayer coroutine completed for {player.Nickname}.");
        }
    }
}