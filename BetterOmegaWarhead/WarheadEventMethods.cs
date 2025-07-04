namespace BetterOmegaWarhead
{
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Doors;
    using MEC;
    using UnityEngine;
    using Server = Exiled.Events.Handlers.Server;

    public class WarheadEventMethods
    {
        private readonly Plugin _plugin;
        public WarheadEventMethods(Plugin plugin) => _plugin = plugin;

        public void Init()
        {
            Log.Debug("Initializing WarheadEventMethods, subscribing to RoundEnded and RoundStarted events.");
            Server.RoundEnded += _plugin.EventHandlers.OnRoundEnd;
            Server.RoundStarted += _plugin.EventHandlers.OnRoundStart;
        }

        public void Disable()
        {
            Log.Debug("Disabling WarheadEventMethods, cleaning up and unsubscribing events.");
            Clean();
            Server.RoundEnded -= _plugin.EventHandlers.OnRoundEnd;
            Server.RoundStarted -= _plugin.EventHandlers.OnRoundStart;
        }

        public void Clean()
        {
            Log.Debug($"Cleaning WarheadEventMethods, OmegaActivated: {OmegaActivated}, killing {_plugin.EventHandlers.Coroutines.Count} coroutines.");
            OmegaActivated = false;
            Warhead.Status = WarheadStatus.NotArmed;
            Map.ResetLightsColor();
            foreach (var coroutine in _plugin.EventHandlers.Coroutines)
            {
                Timing.KillCoroutines(coroutine);
                Log.Debug($"Killed coroutine: {coroutine}");
            }
        }

        public bool OmegaActivated { get; private set; }

        public bool isOmegaActive()
        {
            bool active = OmegaActivated && Warhead.Controller.isActiveAndEnabled;
            Log.Debug($"Checking isOmegaActive: OmegaActivated={OmegaActivated}, WarheadActive={Warhead.Controller.isActiveAndEnabled}, Result={active}");
            return active;
        }

        public void ActivateOmegaWarhead(float timeToDetonation)
        {
            Log.Debug($"Activating Omega Warhead with detonation time: {timeToDetonation}s.");
            OmegaActivated = true;
            Color lightColor = new Color(_plugin.Config.LightsColorR, _plugin.Config.LightsColorG, _plugin.Config.LightsColorB);
            Log.Debug($"Changing room lights to color: R={lightColor.r}, G={lightColor.g}, B={lightColor.b}");
            ChangeRoomColors(lightColor);

            Log.Debug("Sending Omega activation Cassie message and broadcast.");
            _plugin.NotificationMethods.SendImportantCassieMessage(_plugin.Config.StartingOmegaCassie);
            _plugin.NotificationMethods.BroadcastOmegaActivation();

            Log.Debug("Starting Omega Warhead sequence coroutines.");
            StartOmegaWarheadSequence(timeToDetonation);
        }

        public void StopOmega()
        {
            Log.Debug("Stopping Omega Warhead, sending stop Cassie message and cleaning up.");
            _plugin.NotificationMethods.SendImportantCassieMessage(_plugin.Config.StoppingOmegaCassie);
            Clean();
        }

        public void ChangeRoomColors(Color color)
        {
            Log.Debug($"Changing map lights color to: R={color.r}, G={color.g}, B={color.b}");
            Map.ChangeLightsColor(color);
        }

        public void StartOmegaWarheadSequence(float timeToDetonation)
        {
            Log.Debug($"Starting Omega Warhead sequence with detonation time: {timeToDetonation}s.");
            _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(OmegaWarheadSequenceDetonation(timeToDetonation), "OmegaWarheadSequenceDetonation"));
            Log.Debug("Started OmegaWarheadSequenceDetonation coroutine.");
            _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(OmegaWarheadSequenceHeli(), "OmegaWarheadSequenceHelicopter"));
            Log.Debug("Started OmegaWarheadSequenceHeli coroutine.");
            _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(OmegaWarheadSequenceCheckpointOpen(), "OmegaWarheadSequenceOpenCheckpoints"));
            Log.Debug("Started OmegaWarheadSequenceCheckpointOpen coroutine.");
        }

        public IEnumerator<float> OmegaWarheadSequenceDetonation(float timeToDetonation)
        {
            Log.Debug($"OmegaWarheadSequenceDetonation coroutine started with timeToDetonation: {timeToDetonation}s.");
            int[] notificationTimes = { 300, 240, 180, 120, 60, 25, 10 };

            foreach (var notifyTime in notificationTimes)
            {
                if (timeToDetonation >= notifyTime)
                {
                    Log.Debug($"Waiting {timeToDetonation - notifyTime}s for notification at {notifyTime}s.");
                    yield return Timing.WaitForSeconds(timeToDetonation - notifyTime);
                    if (isOmegaActive())
                    {
                        Log.Debug($"Sending Cassie notification for {notifyTime}s remaining.");
                        if (_plugin.Config.CassieMessageClearBeforeWarheadMessage) Cassie.Clear();
                        _plugin.NotificationMethods.SendCassieMessage($".G3 {notifyTime} Seconds until Omega Warhead Detonation");
                    }
                    timeToDetonation = notifyTime;
                }
            }

            for (int i = 10; i > 0; i--)
            {
                if (isOmegaActive())
                {
                    Log.Debug($"Turning off lights for 0.75s, countdown: {i}s.");
                    Map.TurnOffAllLights(0.75f);
                    yield return Timing.WaitForSeconds(1.0f);
                }
                else
                {
                    Log.Debug($"Omega Warhead no longer active, stopping countdown at {i}s.");
                    yield break;
                }
            }

            if (isOmegaActive())
            {
                Log.Debug("Starting HandleWarheadDetonation coroutine.");
                _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(HandleWarheadDetonation(), "OmegaWarheadDetonationHandler"));
            }
            Log.Debug("OmegaWarheadSequenceDetonation coroutine completed.");
        }

        public IEnumerator<float> OmegaWarheadSequenceHeli()
        {
            Log.Debug($"OmegaWarheadSequenceHeli coroutine started, waiting {_plugin.Config.HelicopterBroadcastDelay}s for broadcast.");
            yield return Timing.WaitForSeconds(_plugin.Config.HelicopterBroadcastDelay);
            if (isOmegaActive())
            {
                Log.Debug("Broadcasting helicopter countdown and starting helicopter escape coroutine.");
                _plugin.NotificationMethods.BroadcastHelicopterCountdown();
                _plugin.EventHandlers.Coroutines.Add(Timing.RunCoroutine(_plugin.PlayerMethods.HandleHelicopterEscape(), "OmegaWarheadHeliEvacuationHandler"));
            }
            else
            {
                Log.Debug("Omega Warhead not active, skipping helicopter sequence.");
            }
            Log.Debug("OmegaWarheadSequenceHeli coroutine completed.");
        }

        public IEnumerator<float> OmegaWarheadSequenceCheckpointOpen()
        {
            Log.Debug($"OmegaWarheadSequenceCheckpointOpen coroutine started, waiting {_plugin.Config.OpenAndLockCheckpointDoorsDelay}s.");
            yield return Timing.WaitForSeconds(_plugin.Config.OpenAndLockCheckpointDoorsDelay);
            if (isOmegaActive())
            {
                Log.Debug("Opening and locking checkpoint doors.");
                OpenAndLockCheckpointDoors();
            }
            else
            {
                Log.Debug("Omega Warhead not active, skipping checkpoint door sequence.");
            }
            Log.Debug("OmegaWarheadSequenceCheckpointOpen coroutine completed.");
        }

        public void OpenAndLockCheckpointDoors()
        {
            Log.Debug("OpenAndLockCheckpointDoors called, sending Cassie message.");
            _plugin.NotificationMethods.SendImportantCassieMessage(_plugin.Config.CheckpointUnlockCassie);
            foreach (Door door in Door.List)
            {
                if (IsCheckpointDoor(door.Type))
                {
                    Log.Debug($"Opening and locking checkpoint door: {door.Type}");
                    door.IsOpen = true;
                    door.PlaySound(DoorBeepType.InteractionAllowed);
                    door.Lock(69420.0f, DoorLockType.Warhead);
                }
            }
            Log.Debug("OpenAndLockCheckpointDoors completed.");
        }

        public bool IsCheckpointDoor(DoorType doorType)
        {
            bool isCheckpoint = doorType == DoorType.CheckpointEzHczA ||
                               doorType == DoorType.CheckpointEzHczB ||
                               doorType == DoorType.CheckpointLczA ||
                               doorType == DoorType.CheckpointLczB;
            Log.Debug($"Checking if door {doorType} is a checkpoint door: {isCheckpoint}");
            return isCheckpoint;
        }

        public IEnumerator<float> HandleWarheadDetonation()
        {
            Log.Debug("HandleWarheadDetonation coroutine started.");
            if(_plugin.Config.CassieMessageClearBeforeWarheadMessage) Cassie.Clear();
            Log.Debug("Sending Omega detonation Cassie message.");
            _plugin.NotificationMethods.SendCassieMessage(_plugin.Config.DetonatingOmegaCassie);
            Log.Debug("Detonating warhead.");
            DetonateWarhead();

            while (true)
            {
                if (_plugin.Config.CassieMessageClearBeforeWarheadMessage) Cassie.Clear();
                yield return Timing.WaitForSeconds(0.175f);
            }
        }

        public void DetonateWarhead()
        {
            Log.Debug("DetonateWarhead called, handling players and triggering effects.");
            _plugin.PlayerMethods.HandlePlayersOnNuke();
            Log.Debug("Triggering warhead detonation and shake.");
            Warhead.Detonate();
            Warhead.Shake();

            foreach (Room room in Room.List)
            {
                Log.Debug($"Processing room: {room.Type}");
                foreach (Window window in room.Windows)
                {
                    Log.Debug($"Breaking window in {room.Type}.");
                    window.BreakWindow();
                }
                foreach (Door door in room.Doors)
                {
                    Log.Debug($"Opening door {door.Type} in {room.Type}.");
                    door.IsOpen = true;
                    if (door is BreakableDoor breakableDoor && !breakableDoor.IsDestroyed)
                    {
                        Log.Debug($"Breaking breakable door {door.Type} in {room.Type}.");
                        breakableDoor.Break();
                    }
                }
                Log.Debug($"Locking down and turning off lights in {room.Type} for 69420s.");
                room.LockDown(69420.0f, DoorLockType.Warhead);
                room.TurnOffLights(69420.0f);
            }
            Log.Debug("DetonateWarhead completed.");
        }
    }
}