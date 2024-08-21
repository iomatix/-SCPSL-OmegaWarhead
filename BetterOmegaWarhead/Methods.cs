namespace BetterOmegaWarhead
{
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Doors;
    using Exiled.CustomModules.API.Extensions;
    using MapGeneration.Distributors;
    using MEC;
    using PlayerRoles;
    using PluginAPI.Core.Zones.Heavy;
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
            Clean();
            Server.RoundEnded -= _plugin.EventHandlers.OnRoundEnd;
            Server.RoundStarted -= _plugin.EventHandlers.OnRoundStart;
        }

        public void Clean()
        {
            OmegaActivated = false;
            heliSurvivors.Clear();
        }
        public bool OmegaActivated { get; private set; }


        public bool isOmegaActivated()
        {

            return OmegaActivated;
        }

        public void ActivateOmegaWarhead()
        {

            Warhead.Controller.CurScenario.TimeToDetonate = (int)_plugin.Config.TimeToDetonation;
            OmegaActivated = true;
            Warhead.Start();
            ChangeRoomColors(new Color(_plugin.Config.LightsColorR, _plugin.Config.LightsColorG, _plugin.Config.LightsColorB));

            SendImportantCassieMessage(_plugin.Config.StartingOmegaCassie);
            BroadcastOmegaActivation();

            StartOmegaWarheadSequence();

        }

        public void StopOmega()
        {
            Warhead.Stop();
            Clean();
            SendImportantCassieMessage(_plugin.Config.StoppingOmegaCassie);
            foreach (var coroutine in _plugin.EventHandlers.Coroutines) Timing.KillCoroutines(coroutine);
            foreach (Room room in Room.List) room.ResetColor();
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
            if (!(message.Length > 0)) return;
            Cassie.Message(message, isNoisy: false, isSubtitles: false, isHeld: false);
        }

        public void SendImportantCassieMessage(string message)
        {
            if (!(message.Length > 0)) return;
            Cassie.Clear();
            Cassie.Message(message, isSubtitles: false, isHeld: false);
        }

        public void BroadcastOmegaActivation()
        {
            Map.Broadcast(8, _plugin.Config.ActivatedMessage);
        }

        public void StartOmegaWarheadSequence()
        {
            _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(OmegaWarheadSequenceDetonation()));
            _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(OmegaWarheadSequenceHeli()));
            _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(OmegaWarheadSequenceCheckpointOpen()));
        }


        public IEnumerator<float> OmegaWarheadSequenceDetonation()
        {
            float timeToDetonation = _plugin.Config.TimeToDetonation;

            int[] notificationTimes = { 300, 240, 180, 120, 60, 30, 20 };

            
            foreach (var notifyTime in notificationTimes)
            {
                if (timeToDetonation >= notifyTime)
                {
                    yield return Timing.WaitForSeconds(timeToDetonation - notifyTime);
                    if (isOmegaActivated()) SendCassieMessage($"{notifyTime} Seconds until Omega Warhead Detonation");
                    timeToDetonation = notifyTime;
                }
            }

            yield return Timing.WaitForSeconds(10f);
            for (int i = 10; i > 0; i--)
            {
                if (isOmegaActivated())
                {
                    string msg = $"{i}";
                    Cassie.Clear();
                    SendCassieMessage(msg);
                    Map.TurnOffAllLights(1.25f);
                    yield return Timing.WaitForSeconds(2.5f);
                }
            }

            if (isOmegaActivated()) HandleWarheadDetonation();
        }

        public IEnumerator<float> OmegaWarheadSequenceHeli()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.HelicopterBroadcastDelay);
            if (isOmegaActivated()) BroadcastHelicopterCountdown();
        }

        public IEnumerator<float> OmegaWarheadSequenceCheckpointOpen()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.OpenAndLockCheckpointDoorsDelay);
            if (isOmegaActivated()) OpenAndLockCheckpointDoors();
        }

        public void OpenAndLockCheckpointDoors()
        {
            SendImportantCassieMessage(_plugin.Config.CheckpointUnlockCassie);
            foreach (Door door in Door.List)
            {
                if (IsCheckpointDoor(door.Type))
                {
                    door.IsOpen = true;
                    door.PlaySound(DoorBeepType.InteractionAllowed);
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
            SendImportantCassieMessage(_plugin.Config.HeliIncomingCassie);
            _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(HandleHelicopterEscape()));
        }

        public IEnumerator<float> HandleHelicopterEscape()
        {
            yield return Timing.WaitForSeconds(12f);
            Vector3 escapePrimaryPos = Door.Get(DoorType.EscapePrimary).WorldPosition(new Vector3(0f,0f,0f));
            Vector3 helicopterZone = new Vector3(escapePrimaryPos.x - 3.66f, escapePrimaryPos.y - 0.23f, escapePrimaryPos.z - 17.68f);

            RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.Selection, SpawnableTeamType.NineTailedFox);
            yield return Timing.WaitForSeconds(19f);
            foreach (Player player in Player.List)
            {
                if (!player.IsScp && player.IsAlive && Vector3.Distance(player.Position, helicopterZone) <= 8.33f)
                {
                    player.Broadcast(4, _plugin.Config.HelicopterEscape);
                    player.Position = new Vector3(293f, 978f, -52f);
                    player.Scale = Vector3.zero;
                    player.EnableEffect(EffectType.Flashed, 15f);
                    heliSurvivors.Add(player);
                    _plugin.EventHandlers.Coroutines.Add(Timing.CallDelayed(0.5f, () => player.EnableEffect(EffectType.Ensnared)));
                }

            }

            
        }

        public void HandleWarheadDetonation()
        {

            Cassie.Clear();
            SendCassieMessage(_plugin.Config.DetonatingOmegaCassie);
            

            foreach (Player player in Player.List)
            {
                
                if (heliSurvivors.Contains(player) || IsInShelter(player))
                {
                    HandleSafePlayer(player); 
                }
                else
                {
                    if (player.IsAlive) player.Kill("Omega Warhead");
                }

                if(!player.IsAlive) player.RoleManager.ServerSetRole(RoleTypeId.Spectator, RoleChangeReason.Died);

                if (player.IsAlive && heliSurvivors.Contains(player)) player.RoleManager.ServerSetRole(RoleTypeId.Spectator, RoleChangeReason.Escaped);
                

            }
            DetonateWarhead();

            foreach (Room room in Room.List)
            {
                room.LockDown(DoorLockType.NoPower);
                room.TurnOffLights();
            }
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

        public IEnumerable<float> HandleSafePlayer(Player player)
        {
            player.IsGodModeEnabled = true;
            player.EnableEffect(EffectType.Flashed, 2.5f);
            yield return Timing.WaitForSeconds(2f);
            player.IsGodModeEnabled = false;
            player.Position = new Vector3(-53, 988, -50);
            player.EnableEffect(EffectType.Blinded, 5f);

        }
    }
}
