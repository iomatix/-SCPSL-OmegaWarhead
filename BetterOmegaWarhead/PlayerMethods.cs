namespace BetterOmegaWarhead
{
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using MEC;


    public class PlayerMethods
    {
        private readonly Plugin _plugin;
        public PlayerMethods(Plugin plugin) => _plugin = plugin;
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
                    player.Hurt(amount: 0.15f, damageType: DamageType.Explosion);
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
            player.EnableEffect(EffectType.Blinded, 5f);

        }
    }
}
