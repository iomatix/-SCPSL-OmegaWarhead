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
            Map.ResetColorOfLights();
            foreach (var coroutine in _coroutines)
            {
                Timing.KillCoroutines(coroutine);
                LogHelper.Debug($"Killed coroutine: {coroutine}");
            }
            _coroutines.Clear();
        }

        public void Activate(float timeToDetonation)
        {
            LogHelper.Debug($"Activating Omega Warhead with detonation time: {timeToDetonation}s.");
            _omegaActivated = true;
            Color lightColor = new Color(_plugin.Config.LightsColorR, _plugin.Config.LightsColorG, _plugin.Config.LightsColorB);
            LogHelper.Debug($"Changing room lights to color: R={lightColor.r}, G={lightColor.g}, B={lightColor.b}");
            Map.SetColorOfLights(lightColor);

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
            int[] notifyTimes = { 300, 240, 180, 120, 60, 25, 10 };
            foreach (var notifyTime in notifyTimes)
            {
                if (timeToDetonation >= notifyTime)
                {
                    yield return Timing.WaitForSeconds(timeToDetonation - notifyTime);
                    if (IsOmegaActive)
                    {
                        if (_plugin.Config.CassieMessageClearBeforeWarheadMessage) Cassie.Clear();
                        _plugin.NotificationMethods.SendCassieMessage($".G3 {notifyTime} Seconds until Omega Warhead Detonation");
                    }
                    timeToDetonation = notifyTime;
                }
            }

            for (int i = 10; i > 0; i--)
            {
                if (!IsOmegaActive) yield break;
                Map.TurnOffLights(0.75f);
                yield return Timing.WaitForSeconds(1.0f);
            }

            if (IsOmegaActive)
                _coroutines.Add(Timing.RunCoroutine(HandleDetonation(), "OmegaDetonation"));
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
            if (_plugin.Config.CassieMessageClearBeforeWarheadMessage) Cassie.Clear();
            _plugin.NotificationMethods.SendCassieMessage(_plugin.Config.DetonatingOmegaCassie);

            _plugin.PlayerMethods.HandlePlayersOnNuke();
            Warhead.Detonate();
            Warhead.Shake();

            foreach (Room room in Room.List)
            {
                //foreach (Window window in room.windows)
                //window.BreakWindow();

                foreach (LabApi.Features.Wrappers.Door door in room.Doors)
                {
                    door.IsOpened = true;
                    if (door is LabApi.Features.Wrappers.BreakableDoor breakable && !breakable.IsBroken) breakable.TryBreak();

                    door.Lock(Interactables.Interobjects.DoorUtils.DoorLockReason.Warhead, true);
                }


                foreach (var lightController in room.AllLightControllers)
                {
                    lightController.LightsEnabled = false;
                }
            }

            while (true)
            {
                if (_plugin.Config.CassieMessageClearBeforeWarheadMessage) Cassie.Clear();
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

            LogHelper.Debug("Omega is active during detonation, calling OnDetonation.");
            OnDetonation();
        }

        public void OnDetonation()
        {
            LogHelper.Debug("OnDetonation called.");
            // Optional: trigger post-detonation logic here, like logging, cleanup, visual effects, etc.
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
