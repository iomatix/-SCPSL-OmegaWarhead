namespace BetterOmegaWarhead
{
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Doors;
    using Exiled.Events.EventArgs.Map;
    using MEC;
    using PlayerRoles;
    using Respawning;
    using Respawning.Waves;
    using UnityEngine;
    using Server = Exiled.Events.Handlers.Server;

    public class WarheadEventMethods
    {
        private readonly Plugin _plugin;
        public WarheadEventMethods(Plugin plugin) => _plugin = plugin;

        private readonly HashSet<Player> heliSurvivors = new HashSet<Player>();
        public void Init()
        {
            Server.RoundEnded += _plugin.EventHandlers.OnRoundEnd;
            Server.RoundStarted += _plugin.EventHandlers.OnRoundStart;
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
            foreach (Player player in heliSurvivors) player.IsGodModeEnabled = false;
            heliSurvivors.Clear();
            Respawn.RestartWaves();
            Warhead.Status = WarheadStatus.NotArmed;
            Map.ResetLightsColor();
            foreach (var coroutine in _plugin.EventHandlers.Coroutines) Timing.KillCoroutines(coroutine);
            _plugin.EventHandlers.Coroutines.Clear();
            


        }
        public bool OmegaActivated { get; private set; }

        public bool isOmegaActive()
        {
            return OmegaActivated; //&& Warhead.Controller.isActiveAndEnabled;
        }

        public void ActivateOmegaWarhead(float timeToDetonation)
        {
            OmegaActivated = true;
            ChangeRoomColors(new Color(_plugin.Config.LightsColorR, _plugin.Config.LightsColorG, _plugin.Config.LightsColorB));

            _plugin.NotificationMethods.SendImportantCassieMessage(_plugin.Config.StartingOmegaCassie);
            _plugin.NotificationMethods.BroadcastOmegaActivation();

            StartOmegaWarheadSequence(timeToDetonation);

        }

        public void StopOmega()
        {
            _plugin.NotificationMethods.SendImportantCassieMessage(_plugin.Config.StoppingOmegaCassie);
            Warhead.Stop();
            Clean();
        }

        public void ChangeRoomColors(Color color)
        {
            Map.ChangeLightsColor(color);
        }

        public void StartOmegaWarheadSequence(float timeToDetonation)
        {
            _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(OmegaWarheadSequenceDetonation(timeToDetonation)));
            _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(OmegaWarheadSequenceHeli()));
            _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(OmegaWarheadSequenceCheckpointOpen()));
        }
        public IEnumerator<float> OmegaWarheadSequenceDetonation(float timeToDetonation)
        {

            int[] notificationTimes = { 300, 240, 180, 120, 60, 25, 10 };

            foreach (var notifyTime in notificationTimes)
            {
                if (timeToDetonation >= notifyTime)
                {
                    yield return Timing.WaitForSeconds(timeToDetonation - notifyTime);
                    if (isOmegaActive())
                    {
                        Cassie.Clear();
                        _plugin.NotificationMethods.SendCassieMessage($".G3 {notifyTime} Seconds until Omega Warhead Detonation");

                    }
                    timeToDetonation = notifyTime;
                }
            }

            for (int i = 10; i > 0; i--)
            {
                if (isOmegaActive())
                {
                    Map.TurnOffAllLights(0.75f);
                    yield return Timing.WaitForSeconds(1.0f);
                }
            }

            if (isOmegaActive()) _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(HandleWarheadDetonation()));
        }

        public IEnumerator<float> OmegaWarheadSequenceHeli()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.HelicopterBroadcastDelay);
            if (isOmegaActive())
            {
                _plugin.NotificationMethods.BroadcastHelicopterCountdown();
                _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(HandleHelicopterEscape()));
            }
        }

        public IEnumerator<float> OmegaWarheadSequenceCheckpointOpen()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.OpenAndLockCheckpointDoorsDelay);
            if (isOmegaActive()) OpenAndLockCheckpointDoors();
        }

        public void OpenAndLockCheckpointDoors()
        {
            _plugin.NotificationMethods.SendImportantCassieMessage(_plugin.Config.CheckpointUnlockCassie);
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

        public IEnumerator<float> HandleHelicopterEscape()
        {
            yield return Timing.WaitForSeconds(12.0f);
            Vector3 escapePrimaryPos = Door.Get(DoorType.EscapePrimary).GameObject.transform.position;
            Vector3 helicopterZone = new Vector3(escapePrimaryPos.x - 3.66f, escapePrimaryPos.y - 0.23f, escapePrimaryPos.z - 17.68f);
            yield return Timing.WaitForSeconds(1.5f);
            Respawn.PauseWaves();
            Respawn.SummonNtfChopper();
            yield return Timing.WaitForSeconds(19.0f);
            foreach (Player player in Player.List)
            {
                if (!player.IsScp && player.IsAlive && Vector3.Distance(player.Position, helicopterZone) <= 8.33f)
                {
                    heliSurvivors.Add(player);
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

        public IEnumerator<float> HandleWarheadDetonation()
        {

            Cassie.Clear();
            _plugin.NotificationMethods.SendCassieMessage(_plugin.Config.DetonatingOmegaCassie);
            _plugin.PlayerMethods.HandlePlayersOnNuke(heliSurvivors, _plugin.EventHandlers);
            DetonateWarhead();


            while (true)
            {
                Cassie.Clear();
                yield return Timing.WaitForSeconds(0.175f);
            }

        }

        void DetonateWarhead()
        {
            Warhead.Status = WarheadStatus.Detonated;
            Warhead.Shake();

            foreach (Room room in Room.List)
            {
                foreach (Window window in room.Windows)
                {
                    window.BreakWindow();
                }
                foreach (Door door in room.Doors)
                {
                    door.IsOpen = true;
                    if (door is BreakableDoor breakableDoor && !breakableDoor.IsDestroyed)
                    {
                        breakableDoor.Break();
                    }
                }
                room.LockDown(69420, DoorLockType.Warhead);
                room.TurnOffLights();
            }
        }

    }
}
