namespace BetterOmegaWarhead
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using MEC;
    using PlayerRoles;
    using UnityEngine;

    public class PlayerMethods
    {
        private readonly Plugin _plugin;
        public PlayerMethods(Plugin plugin) => _plugin = plugin;

        public IEnumerator<float> HandleHelicopterEscape(HashSet<Player> hashSetHeliSurvivors)
        {
            Vector3 helicopterZone = new Vector3(128.681f, 995.456f, -42.202f);

            DisableFactionTokens();

            yield return Timing.WaitForSeconds(12.0f);
            yield return Timing.WaitForSeconds(1.5f);
            Respawn.SummonNtfChopper();
            yield return Timing.WaitForSeconds(19.0f);
            foreach (Player player in Player.List)
            {
                if (!player.IsScp && player.IsAlive && Vector3.Distance(player.Position, helicopterZone) <= _plugin.Config.HelicopterZoneSize)
                {
                    hashSetHeliSurvivors.Add(player);
                    player.IsGodModeEnabled = true;
                    player.Broadcast(_plugin.Config.HelicopterEscape);
                    player.EnableEffect(EffectType.Flashed, 1.75f);
                    player.Position = new Vector3(293f, 978f, -52f);
                    player.ClearInventory();
                    player.EnableEffect(EffectType.Ensnared);
                    if (player.LeadingTeam == LeadingTeam.FacilityForces) Round.EscapedScientists++;
                    else if (player.LeadingTeam == LeadingTeam.ChaosInsurgency) Round.EscapedDClasses++;
                    yield return Timing.WaitForSeconds(0.75f);
                    player.Role.Set(RoleTypeId.Spectator, reason: SpawnReason.Escaped);

                }
            }
        }
        public void DisableFactionTokens()
        {
            foreach (SpawnableFaction faction in Enum.GetValues(typeof(SpawnableFaction)))
            {
                Respawn.SetTokens(faction, 0);
            }
            Respawn.PauseWaves();
        }
        public void HandlePlayersOnNuke(HashSet<Player> inHeliSurvivors)
        {
            foreach (Player player in Player.List)
            {

                if (IsInShelter(player) || inHeliSurvivors.Contains(player))
                {
                    _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(HandleSavePlayer(player)));
                }
                else
                {
                  
                    player.EnableEffect(EffectType.Flashed, 0.75f);
                    player.Hurt(amount: 0.15f, damageType: DamageType.Explosion);
                    player.EnableEffect(EffectType.Blurred, 4.75f);
                    player.Kill("Omega Warhead");
                }

            }
        }

        public bool IsInShelter(Player player)
        {
            return player.CurrentRoom.Type == RoomType.EzShelter;
        }

        public IEnumerator<float> HandleSavePlayer(Player player)
        {
            player.IsGodModeEnabled = true;
            player.EnableEffect(EffectType.Flashed, 0.75f);
            yield return Timing.WaitForSeconds(0.75f);
            player.IsGodModeEnabled = false;
            player.EnableEffect(EffectType.Blinded, 2.5f);
            player.EnableEffect(EffectType.Blurred, 4.75f);
        }
    }
}
