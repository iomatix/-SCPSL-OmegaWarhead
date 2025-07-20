namespace BetterOmegaWarhead
{
    using BetterOmegaWarhead.Core.LoggingUtils;
    using LabApi.Events.Arguments.WarheadEvents;
    using LabApi.Features.Enums;
    using LabApi.Features.Wrappers;
    using MEC;
    using System.Collections.Generic;
    using UnityEngine;
    using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;
    using ServerHandler = LabApi.Events.Handlers.ServerEvents;
    using WarheadHandler = LabApi.Events.Handlers.WarheadEvents;

    public class OmegaWarheadManager
    {
        private readonly Plugin _plugin;
        private readonly List<CoroutineHandle> _coroutines = new List<CoroutineHandle>();
        private bool _omegaActivated;

        public OmegaWarheadManager(Plugin plugin) => _plugin = plugin;

        public bool IsOmegaActive => _omegaActivated && Warhead.BaseController.isActiveAndEnabled;

        public void Init()
        {
            LogHelper.Debug("Initializing OmegaWarheadManager, subscribing to events.");
            ServerHandler.RoundEnded += _plugin.EventHandler.OnRoundEnd;
            ServerHandler.RoundStarted += _plugin.EventHandler.OnRoundStart;
            WarheadHandler.Starting += _plugin.EventHandler.OnWarheadStart;
            WarheadHandler.Stopping += _plugin.EventHandler.OnWarheadStop;
            WarheadHandler.Detonating += _plugin.EventHandler.OnWarheadDetonate;
        }

        public void Disable()
        {
            LogHelper.Debug("Disabling OmegaWarheadManager and unsubscribing from events.");
            Cleanup();
            ServerHandler.RoundEnded -= _plugin.EventHandler.OnRoundEnd;
            ServerHandler.RoundStarted -= _plugin.EventHandler.OnRoundStart;
            WarheadHandler.Starting -= _plugin.EventHandler.OnWarheadStart;
            WarheadHandler.Stopping -= _plugin.EventHandler.OnWarheadStop;
            WarheadHandler.Detonating -= _plugin.EventHandler.OnWarheadDetonate;
        }

        public void Cleanup()
        {
            LogHelper.Debug($"Cleaning up OmegaWarheadManager. OmegaActivated: {_omegaActivated}, Coroutines: {_coroutines.Count}.");
            _omegaActivated = false;
            Warhead.LeverStatus = false;
            Warhead.Stop(); // Ensure Alpha Warhead is stopped
            Map.ResetColorOfLights();
            foreach (var coroutine in _coroutines)
            {
                Timing.KillCoroutines(coroutine);
            }
            _coroutines.Clear();
        }

        public void Activate(float timeToDetonation)
        {
            LogHelper.Debug($"Activating Omega Warhead with detonation time: {timeToDetonation}s.");
            _omegaActivated = true;
            Warhead.Stop(); // Stop any existing Alpha Warhead countdown
            Color lightColor = new Color(_plugin.Config.LightsColorR, _plugin.Config.LightsColorG, _plugin.Config.LightsColorB);
            LogHelper.Debug($"Changing room lights to color: R={lightColor.r}, G={lightColor.g}, B={lightColor.b}");
            Timing.CallDelayed(5.75f, () => Map.SetColorOfLights(lightColor));
             
            _plugin.NotificationMethods.SendImportantCassieMessage(_plugin.Config.StartingOmegaCassie);
            _plugin.NotificationMethods.BroadcastOmegaActivation();

            _coroutines.Add(Timing.RunCoroutine(HandleCountdown(timeToDetonation), "OmegaCountdown"));
            _coroutines.Add(Timing.RunCoroutine(HandleHelicopter(), "OmegaHeli"));
            _coroutines.Add(Timing.RunCoroutine(HandleCheckpointDoors(), "OmegaCheckpoints"));
        }

        public void Stop()
        {
            LogHelper.Debug("Stopping Omega Warhead sequence.");
            _plugin.NotificationMethods.SendImportantCassieMessage(_plugin.Config.StoppingOmegaCassie);
            Cleanup();
        }

        private IEnumerator<float> HandleCountdown(float timeToDetonation)
        {
            int[] notifyTimes = { 300, 240, 180, 120, 60, 25, 15, 5, 4, 3, 2, 1 };
            foreach (var notifyTime in notifyTimes)
            {
                if (timeToDetonation >= notifyTime)
                {
                    yield return Timing.WaitForSeconds(timeToDetonation - notifyTime);
                    if (IsOmegaActive)
                    {
                        string message = notifyTime <= 5
                            ? $"{notifyTime} .G5"
                            : $".G3 {notifyTime} Seconds until Omega Warhead Detonation .G5";
                        if (_plugin.Config.CassieMessageClearBeforeWarheadMessage || notifyTime <= 5)
                            Cassie.Clear();
                        float messageDuration = Cassie.CalculateDuration(message);
                        LogHelper.Debug($"Cassie message '{message}' duration: {messageDuration}s");
                        _plugin.NotificationMethods.SendCassieMessage(message);
                        if (notifyTime <= 5)
                            Map.TurnOffLights(0.75f);

                        // Wait for the message to complete, ensuring sync
                        yield return Timing.WaitForSeconds(messageDuration);
                        timeToDetonation = notifyTime;
                    }
                    else
                    {
                        timeToDetonation = notifyTime;
                    }
                }
            }

            if (IsOmegaActive)
            {
                // No additional pause needed, as message durations are accounted for
                _coroutines.Add(Timing.RunCoroutine(HandleDetonation(), "OmegaDetonation"));
            }
        }

        private IEnumerator<float> HandleHelicopter()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.HelicopterBroadcastDelay);
            if (IsOmegaActive)
            {
                _plugin.NotificationMethods.BroadcastHelicopterCountdown();
                _coroutines.Add(Timing.RunCoroutine(_plugin.PlayerMethods.HandleHelicopterEscape(), "OmegaHeliEvacuation"));
            }
        }

        private IEnumerator<float> HandleCheckpointDoors()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.OpenAndLockCheckpointDoorsDelay);
            if (IsOmegaActive)
            {
                _plugin.NotificationMethods.SendImportantCassieMessage(_plugin.Config.CheckpointUnlockCassie);
                foreach (Door door in Door.List)
                {
                    if (door.DoorName is DoorName.LczCheckpointA || door.DoorName is DoorName.LczCheckpointB || door.DoorName is DoorName.HczCheckpoint)
                    {
                        door.IsOpened = true;
                        door.PlayLockBypassDeniedSound();
                        door.Lock(Interactables.Interobjects.DoorUtils.DoorLockReason.Warhead, true);
                    }
                }
            }
        }

        private IEnumerator<float> HandleDetonation()
        {
            if (_plugin.Config.CassieMessageClearBeforeWarheadMessage)
                Cassie.Clear();
            string detonationMessage = _plugin.Config.DetonatingOmegaCassie;
            float detonationMessageDuration = Cassie.CalculateDuration(detonationMessage);
            LogHelper.Debug($"Detonation Cassie message '{detonationMessage}' duration: {detonationMessageDuration}s");
            _plugin.NotificationMethods.SendCassieMessage(detonationMessage);

            yield return Timing.WaitForSeconds(detonationMessageDuration);

            Warhead.Stop(); // Ensure Alpha Warhead is stopped
            _plugin.PlayerMethods.HandlePlayersOnNuke();
            Warhead.Detonate();
            Warhead.Shake();

            foreach (Room room in Room.List)
            {
                foreach (LabApi.Features.Wrappers.Door door in room.Doors)
                {
                    door.IsOpened = true;
                    if (door is LabApi.Features.Wrappers.BreakableDoor breakable && !breakable.IsBroken)
                        breakable.TryBreak();
                    door.Lock(Interactables.Interobjects.DoorUtils.DoorLockReason.Warhead, true);
                }

                foreach (var lightController in room.AllLightControllers)
                {
                    lightController.LightsEnabled = false;
                }
            }

            while (true)
            {
                if (_plugin.Config.CassieMessageClearBeforeWarheadMessage)
                    Cassie.Clear();
                yield return Timing.WaitForSeconds(0.175f);
            }
        }

        public void StartOmegaSequence(float timeToDetonation)
        {
            LogHelper.Debug("StartOmegaSequence called.");
            Activate(timeToDetonation);
        }

        public void ResetOmegaState()
        {
            LogHelper.Debug("ResetOmegaState called.");
            Cleanup();
        }

        public void HandleWarheadStart(LabApi.Events.Arguments.WarheadEvents.WarheadStartingEventArgs ev)
        {
            LogHelper.Debug("HandleWarheadStart called.");
            if (!IsOmegaActive) return;

            LogHelper.Debug("Omega is active during warhead start, blocking default warhead.");
            ev.IsAllowed = false; // Prevent default warhead from triggering
        }

        public void HandleWarheadStop(WarheadStoppingEventArgs ev)
        {
            LogHelper.Debug("HandleWarheadStop called.");
            if (!IsOmegaActive) return;

            if (!_plugin.Config.IsStopAllowed)
            {
                LogHelper.Debug("Omega stop not allowed by config.");
                ev.IsAllowed = false;
                return;
            }

            LogHelper.Debug("Omega is active during warhead stop, stopping Omega.");
            Stop();
            ev.IsAllowed = false;
        }

        public void HandleWarheadDetonate(LabApi.Events.Arguments.WarheadEvents.WarheadDetonatingEventArgs ev)
        {
            LogHelper.Debug("HandleWarheadDetonate called.");
            if (!IsOmegaActive) return;

            LogHelper.Debug("Omega is active during detonation, attempting to block default warhead detonation.");
            ev.IsAllowed = false; // Attempt to block, though it may not work
            Warhead.Stop(); // Fallback to ensure Alpha Warhead is stopped
        }

        public void OnDetonation()
        {
            LogHelper.Debug("OnDetonation called.");
            _omegaActivated = false;
        }

        public void OnRoundStartCleanup()
        {
            LogHelper.Debug("OnRoundStartCleanup called.");
            Cleanup();
        }

        public void Dispose()
        {
            LogHelper.Debug("Dispose called.");
            Disable();
        }
    }
}