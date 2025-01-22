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
        public void HandlePlayersOnNuke(HashSet<Player> inHeliSurvivors, EventHandlers handlers)
        {
            foreach (Player player in Player.List)
            {

                if (IsInShelter(player) || inHeliSurvivors.Contains(player))
                {
                    handlers.Coroutines.Add(Timing.RunCoroutine(HandlePlayerInShelter(player)));
                }
                else
                {
                    player.Hurt(amount: 0.075f, damageType: DamageType.Warhead);
                    player.Kill("Omega Warhead");
                }

            }
        }
        public bool IsInShelter(Player player)
        {
            return player.CurrentRoom.Type == RoomType.EzShelter;
        }

        public IEnumerator<float> HandlePlayerInShelter(Player player)
        {
            player.IsGodModeEnabled = true;
            player.EnableEffect(EffectType.Flashed, 0.75f);
            yield return Timing.WaitForSeconds(0.75f);
            player.IsGodModeEnabled = false;
            player.EnableEffect(EffectType.Blinded, 5f);

        }
    }
}
