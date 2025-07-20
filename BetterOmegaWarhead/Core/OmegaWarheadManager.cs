namespace BetterOmegaWarhead
{
    using BetterOmegaWarhead.Core.LoggingUtils;
    using BetterOmegaWarhead.NotificationUtils;
    using LabApi.Events.Arguments.WarheadEvents;
    using LabApi.Features.Enums;
    using LabApi.Features.Wrappers;
    using MEC;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;
    using ServerHandler = LabApi.Events.Handlers.ServerEvents;
    using WarheadHandler = LabApi.Events.Handlers.WarheadEvents;

    public class OmegaWarheadManager
    {
        private readonly Plugin _plugin;
        private readonly List<CoroutineHandle> _coroutines = new List<CoroutineHandle>();
        private bool _omegaActivated, _omegaDetonated;
        private static readonly int[] NotifyTimes = { 300, 240, 180, 120, 60, 25, 15, 10, 5, 4, 3, 2, 1 };

        public OmegaWarheadManager(Plugin plugin) => _plugin = plugin;

        public bool IsOmegaActive => _omegaActivated;
        public void AddCoroutines(params CoroutineHandle[] handles)
        {
            foreach (CoroutineHandle handle in handles)
            {
                _coroutines.Add(handle);
            }
        }


        public void Init()
        {
            LogHelper.Debug("Initializing OmegaWarheadManager.");
        }

        public void Disable()
        {
            LogHelper.Debug("Disabling OmegaWarheadManager. Cleaning up...");
            Cleanup();
        }

        public void Cleanup()
        {
            LogHelper.Debug($"Cleaning up OmegaWarheadManager. OmegaActivated: {_omegaActivated}, Coroutines: {_coroutines.Count}.");
            _omegaActivated = false;
            _omegaDetonated = false;
            //Warhead.LeverStatus = false;
            //Warhead.IsLocked = false;
            //Warhead.Stop();
            Warhead.Scenario = default; // Should trigger the reset logic that automatically resets the warhead to its initial scenario 
            Map.ResetColorOfLights();
            foreach (CoroutineHandle coroutine in _coroutines)
            {
                Timing.KillCoroutines(coroutine);
            }
            _coroutines.Clear();
        }

        public void HandleWarheadStart(LabApi.Events.Arguments.WarheadEvents.WarheadStartingEventArgs ev)
        {
            if (!ev.IsAllowed) return;

            if (IsOmegaActive)
            {
                ev.IsAllowed = false;
                return;
            }

            int activeGenerators = Generator.List.Count(g => g.Engaged);
            LogHelper.Debug($"Active Generators: {activeGenerators} (Required: {_plugin.Config.GeneratorsNumGuaranteeOmega})");

            if (activeGenerators < _plugin.Config.GeneratorsNumGuaranteeOmega)
            {
                float chance = _plugin.Config.ReplaceAlphaChance;
                float roll = UnityEngine.Random.Range(0, 100);
                float bonus = activeGenerators * _plugin.Config.GeneratorsIncreaseChanceBy;

                float final = roll + bonus;
                if (chance < 100 && roll >= chance)
                {
                    LogHelper.Debug($"Omega replacement skipped due to random chance roll. (Chance: {chance}%, Roll: {roll} + {bonus} (Activated Generators: {activeGenerators}))");
                    return;
                }
            }
            else
            {
                LogHelper.Debug("Omega forced by generator threshold.");
            }

            LogHelper.Debug("WarheadStart triggered. Initiating Omega sequence...");

            Timing.CallDelayed(_plugin.Config.DelayBeforeOmegaSequence, () =>
            {
                _omegaActivated = true;
                _plugin.WarheadMethods.StartSequence(_plugin.Config.TimeToDetonation);
            });

            LogHelper.Debug("HandleWarheadStart called.");

            LogHelper.Debug("Omega is being activated during warhead start, blocking default warhead.");
            ev.IsAllowed = false; // Prevent default warhead from triggering
        }
        public IEnumerator<float> HandleCountdown(float timeToDetonation)
        {
            foreach (int notifyTime in NotifyTimes)
            {
                if (timeToDetonation >= notifyTime)
                {
                    yield return Timing.WaitForSeconds(timeToDetonation - notifyTime);
                    if (IsOmegaActive)
                    {

                        string message = NotificationUtility.GetCassieCounterNotifyMessage(notifyTime);
                        bool shouldClearCassie = _plugin.Config.CassieMessageClearBeforeWarheadMessage || notifyTime <= 5;

                        if (shouldClearCassie)
                            Exiled.API.Features.Cassie.Clear();
                        float messageDuration = Exiled.API.Features.Cassie.CalculateDuration(message);
                        LogHelper.Debug($"Cassie message '{message}' duration: {messageDuration}s");
                        NotificationUtility.SendCassieMessage(message);
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

            if (_plugin.OmegaManager.IsOmegaActive)
            {
                _plugin.OmegaManager.AddCoroutines(Timing.RunCoroutine(HandleDetonation(), "OmegaDetonation"));
            }
        }

        public IEnumerator<float> HandleHelicopter()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.HelicopterBroadcastDelay);
            if (IsOmegaActive)
            {
                NotificationUtility.BroadcastHelicopterCountdown();
                _coroutines.Add(Timing.RunCoroutine(_plugin.PlayerMethods.HandleHelicopterEscape(), "OmegaHeliEvacuation"));
            }
        }

        public IEnumerator<float> HandleCheckpointDoors()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.OpenAndLockCheckpointDoorsDelay);
            if (_plugin.OmegaManager.IsOmegaActive)
            {
                NotificationUtility.SendImportantCassieMessage(_plugin.Config.CheckpointUnlockCassie);
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


        public IEnumerator<float> HandleDetonation()
        {
            if (_plugin.Config.CassieMessageClearBeforeWarheadMessage)
                Exiled.API.Features.Cassie.Clear();
            string detonationMessage = _plugin.Config.DetonatingOmegaCassie;
            float detonationMessageDuration = Exiled.API.Features.Cassie.CalculateDuration(detonationMessage);
            LogHelper.Debug($"Detonation Cassie message '{detonationMessage}' duration: {detonationMessageDuration}s");
            NotificationUtility.SendCassieMessage(detonationMessage);

            yield return Timing.WaitForSeconds(detonationMessageDuration);

            _plugin.PlayerMethods.HandlePlayersOnNuke();
            _omegaActivated = false;
            _omegaDetonated = true;
            WarheadMethods.DetonateSequence();

            while (true)
            {
                if (_plugin.Config.CassieMessageClearBeforeWarheadMessage)
                    Exiled.API.Features.Cassie.Clear();
                yield return Timing.WaitForSeconds(0.175f);
            }
        }



        public void HandleWarheadStop(WarheadStoppingEventArgs ev)
        {
            if (!ev.IsAllowed) return;

            LogHelper.Debug("HandleWarheadStop called.");
            if (!IsOmegaActive) return;

            if (!_plugin.Config.IsStopAllowed)
            {
                LogHelper.Debug("Omega stop not allowed by config.");
                ev.IsAllowed = false;
                return;
            }

            if (_plugin.Config.ResetOmegaOnWarheadStop && Warhead.IsDetonated)
            {
                LogHelper.Debug("WarheadStop triggered. Resetting Omega sequence...");
                _plugin.WarheadMethods.ResetSequence();
            }

            LogHelper.Debug("Omega is active during warhead stop, stopping Omega...");
            _plugin.WarheadMethods.StopSequence();
            ev.IsAllowed = false;
        }

        public void HandleWarheadDetonate(LabApi.Events.Arguments.WarheadEvents.WarheadDetonatingEventArgs ev)
        {
            LogHelper.Debug("HandleWarheadDetonate called.");
            if (!IsOmegaActive) return;

            LogHelper.Debug("Omega is active during detonation, attempting to block default warhead detonation.");
            ev.IsAllowed = false;
        }






    }
}