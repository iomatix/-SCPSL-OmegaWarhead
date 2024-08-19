namespace BetterOmegaWarhead
{
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Doors;
    using MEC;
    using Respawning;
    using UnityEngine;
    using Server = Exiled.Events.Handlers.Server;

    public class Methods
    {
        private readonly Plugin _plugin;
       public Methods(Plugin plugin) => _plugin = plugin;

        private readonly HashSet<Player> heliSurvivors = new HashSet<Player>();
        public void Init()
        {
            Server.RoundEnded += _plugin.EventHandlers.OnRoundEnd;
            Server.RoundStarted += _plugin.EventHandlers.OnRoundStart;
            _plugin.EventHandlers.OnWaitingForPlayers();
        }

        public void Disable()
        {
            Server.RoundEnded -= _plugin.EventHandlers.OnRoundEnd;
            Server.RoundStarted -= _plugin.EventHandlers.OnRoundStart;
        }

        public bool OmegaActivated { get; private set; }


        public bool isOmegaActivated()
        {

            return OmegaActivated;
        }

        public void StopOmega()
        {
            OmegaActivated = false;
            Cassie.Clear();
            heliSurvivors.Clear();
            Cassie.Message(_plugin.Config.StopCassie, false, false);
            foreach (var coroutine in _plugin.EventHandlers.Coroutines)
                Timing.KillCoroutines(coroutine);
            foreach (Room room in Room.List)
                room.ResetColor();
        }

        public bool ActivateOmegaWarhead()
        {
            OmegaActivated = true;
            ChangeRoomColors(Color.cyan);

            SendCassieMessage(_plugin.Config.Cassie);
            BroadcastOmegaActivation();

            StartOmegaWarheadSequence();

            return true;
        }

        public void ChangeRoomColors(Color color)
        {
            foreach (Room room in Room.List)
            {
                room.Color = color;
            }
        }

        public void SendCassieMessage(string message)
        {
            Cassie.Message(message, isSubtitles: false, isHeld: false);
        }

        public void BroadcastOmegaActivation()
        {
            Map.Broadcast(10, _plugin.Config.ActivatedMessage);
        }

        public void StartOmegaWarheadSequence()
        {
            _plugin.EventHandlers.Coroutines.Add(Timing.CallDelayed(150f, OpenAndLockCheckpointDoors));
            _plugin.EventHandlers.Coroutines.Add(Timing.CallDelayed(158f, BroadcastHelicopterCountdown));
            _plugin.EventHandlers.Coroutines.Add(Timing.CallDelayed(179f, HandleWarheadDetonation));
        }

        public void OpenAndLockCheckpointDoors()
        {
            foreach (Door door in Door.List)
            {
                if (IsCheckpointDoor(door.Type))
                {
                    door.IsOpen = true;
                    door.Lock(69420, DoorLockType.Warhead);
                }
            }
        }

        public bool IsCheckpointDoor(DoorType doorType)
        {
            return doorType == DoorType.CheckpointEzHczA ||
                   doorType == DoorType.CheckpointEzHczB ||
                   doorType == DoorType.CheckpointLczA ||
                   doorType == DoorType.CheckpointLczB;
        }

        public void BroadcastHelicopterCountdown()
        {
            for (int i = 10; i > 0; i--)
            {
                Map.Broadcast(1, $"{_plugin.Config.HelicopterMessage} {i}");
            }

            _plugin.EventHandlers.Coroutines.Add(Timing.CallDelayed(12f, HandleHelicopterEscape));
        }

        public void HandleHelicopterEscape()
        {
            Vector3 helicopterZone = new Vector3(178, 993, -59);

            foreach (Player player in Player.List)
            {
                if (Vector3.Distance(player.Position, helicopterZone) <= 10)
                {
                    player.Broadcast(4, _plugin.Config.HelicopterEscape);
                    player.Position = new Vector3(293, 978, -52);
                    player.Scale = Vector3.zero;
                    player.EnableEffect(EffectType.Flashed, 12f);

                    heliSurvivors.Add(player);
                    _plugin.EventHandlers.Coroutines.Add(Timing.CallDelayed(0.5f, () => player.EnableEffect(EffectType.Ensnared)));
                }
            }

            RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.Selection, SpawnableTeamType.NineTailedFox);
        }

        public void HandleWarheadDetonation()
        {
            DetonateWarhead();

            foreach (Player player in Player.List)
            {
                if (IsInShelter(player))
                {
                    HandlePlayerInShelter(player);
                }
                else if (!heliSurvivors.Contains(player))
                {
                    player.Kill("Omega Warhead");
                }
            }

            ChangeRoomColors(Color.blue);
        }

        public void DetonateWarhead()
        {
            Warhead.Detonate();
            Warhead.Shake();
        }

        public bool IsInShelter(Player player)
        {
            return player.CurrentRoom.Type == RoomType.EzShelter;
        }

        public void HandlePlayerInShelter(Player player)
        {
            player.IsGodModeEnabled = true;

            _plugin.EventHandlers.Coroutines.Add(Timing.CallDelayed(0.2f, () =>
            {
                player.IsGodModeEnabled = false;
                player.EnableEffect(EffectType.Flashed, 2f);
                player.Position = new Vector3(-53, 988, -50);
                player.EnableEffect(EffectType.Blinded, 5f);
            }));
        }
    }
}
