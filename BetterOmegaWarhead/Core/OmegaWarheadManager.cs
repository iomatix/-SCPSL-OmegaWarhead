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
        private static readonly int[] NotifyTimes = { 300, 240, 180, 120, 60, 25, 15, 5, 4, 3, 2, 1 };

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
            ForceStopAlphaWarhead();
            Map.ResetColorOfLights();
            foreach (var coroutine in _coroutines)
            {
                Timing.KillCoroutines(coroutine);
            }
            _coroutines.Clear();
        }

        private void ForceStopAlphaWarhead()
        {
            LogHelper.Debug("Attempting to force stop Alpha Warhead.");
            bool lockedState = Warhead.IsLocked;
            if (lockedState)
            {
                LogHelper.Debug("Alpha Warhead is locked, attempting to unlock.");
                Warhead.IsLocked = false; // Unlock to allow stopping
            }
            if (Warhead.IsDetonationInProgress)
            {
                Warhead.Stop();
                LogHelper.Debug("Alpha Warhead stopped.");
            }
            Warhead.IsLocked = lockedState;
        }

        public void Activate(float timeToDetonation)
        {
            LogHelper.Debug($"Activating Omega Warhead with detonation time: {timeToDetonation}s.");
            _omegaActivated = true;
            ForceStopAlphaWarhead(); // Stop Alpha Warhead at start
            Color lightColor = new Color(_plugin.Config.LightsColorR, _plugin.Config.LightsColorG, _plugin.Config.LightsColorB);
            LogHelper.Debug($"Changing room lights to color: R={lightColor.r}, G={lightColor.g}, B={lightColor.b}");
            Map.SetColorOfLights(lightColor);

            _plugin.NotificationMethods.SendImportantCassieMessage(_plugin.Config.StartingOmegaCassie);
            _plugin.NotificationMethods.BroadcastOmegaActivation();

            float messageDurationAdjustment = CalculateTotalMessageDurations(timeToDetonation);
            LogHelper.Debug($"Adjusting timeToDetonation by {messageDurationAdjustment}s for Cassie messages.");
            _coroutines.Add(Timing.RunCoroutine(HandleCountdown(timeToDetonation - messageDurationAdjustment), "OmegaCountdown"));
            _coroutines.Add(Timing.RunCoroutine(HandleHelicopter(), "OmegaHeli"));
            _coroutines.Add(Timing.RunCoroutine(HandleCheckpointDoors(), "OmegaCheckpoints"));
        }

        private float CalculateTotalMessageDurations(float timeToDetonation)
        {
            float totalDuration = 0f;
            foreach (var notifyTime in NotifyTimes)
            {
                if (timeToDetonation >= notifyTime)
                {
                    string message = GetCassieMessage(notifyTime);
                    totalDuration += Exiled.API.Features.Cassie.CalculateDuration(message);
                }
            }
            totalDuration += Exiled.API.Features.Cassie.CalculateDuration(_plugin.Config.DetonatingOmegaCassie);
            return totalDuration;
        }

        private string GetCassieMessage(int notifyTime)
        {
            if (notifyTime < 5) return "";
            if (notifyTime == 5) return $"{notifyTime}";
            return $".G3 {notifyTime} Seconds until Omega Warhead Detonation .G5";
        }

        public void Stop()
        {
            LogHelper.Debug("Stopping Omega Warhead sequence.");
            _plugin.NotificationMethods.SendImportantCassieMessage(_plugin.Config.StoppingOmegaCassie);
            Cleanup();
        }

        private IEnumerator<float> HandleCountdown(float timeToDetonation)
        {
            foreach (var notifyTime in NotifyTimes)
            {
                if (timeToDetonation >= notifyTime)
                {
                    yield return Timing.WaitForSeconds(timeToDetonation - notifyTime);
                    if (IsOmegaActive)
                    {
                        string message = GetCassieMessage(notifyTime);
                        bool shouldClearCassie = _plugin.Config.CassieMessageClearBeforeWarheadMessage || notifyTime <= 5;
                        
                        if (shouldClearCassie)
                            Exiled.API.Features.Cassie.Clear();
                        float messageDuration = Exiled.API.Features.Cassie.CalculateDuration(message);
                        LogHelper.Debug($"Cassie message '{message}' duration: {messageDuration}s");
                        _plugin.NotificationMethods.SendCassieMessage(message);
                        if (notifyTime <= 5)
                            Map.TurnOffLights(0.75f);
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
                ForceStopAlphaWarhead(); // Ensure Alpha Warhead is stopped before detonation
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
                Exiled.API.Features.Cassie.Clear();
            string detonationMessage = _plugin.Config.DetonatingOmegaCassie;
            float detonationMessageDuration = Exiled.API.Features.Cassie.CalculateDuration(detonationMessage);
            LogHelper.Debug($"Detonation Cassie message '{detonationMessage}' duration: {detonationMessageDuration}s");
            _plugin.NotificationMethods.SendCassieMessage(detonationMessage);

            yield return Timing.WaitForSeconds(detonationMessageDuration);

            ForceStopAlphaWarhead(); // Final check to stop Alpha Warhead
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
                    Exiled.API.Features.Cassie.Clear();
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
            ForceStopAlphaWarhead(); // Fallback to ensure Alpha Warhead is stopped
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