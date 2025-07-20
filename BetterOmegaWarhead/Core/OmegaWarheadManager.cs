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

    /// <summary>
    /// Manages the Omega Warhead functionality, including activation, countdown, and detonation sequences.
    /// </summary>
    #region OmegaWarheadManager Class
    public class OmegaWarheadManager
    {
        #region Fields
        private readonly Plugin _plugin;
        private readonly List<CoroutineHandle> _coroutines = new List<CoroutineHandle>();
        private bool _omegaActivated, _omegaDetonated;
        private static readonly int[] NotifyTimes = { 300, 240, 180, 120, 60, 25, 15, 10, 5, 4, 3, 2, 1 };
        public static int[] GetNotifyTimes() => NotifyTimes;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="OmegaWarheadManager"/> class.
        /// </summary>
        /// <param name="plugin">Reference to the core plugin instance.</param>
        public OmegaWarheadManager(Plugin plugin) => _plugin = plugin;
        #endregion

        #region Properties
        /// <summary>  
        /// Gets or sets a value indicating whether the Omega Warhead is currently active.  
        /// </summary>  
        public bool IsOmegaActive
        {
            get => _omegaActivated;
            set => _omegaActivated = value;
        }

        /// <summary>  
        /// Gets  a value indicating whether the Omega Warhead is already detonated.  
        /// </summary>  
        public bool IsOmegaDetonated => _omegaDetonated;

        #endregion

        #region Initialization and Cleanup
        /// <summary>
        /// Initializes the Omega Warhead manager and logs the initialization process.
        /// </summary>
        public void Init()
        {
            LogHelper.Debug("Initializing OmegaWarheadManager.");
        }

        /// <summary>
        /// Disables the Omega Warhead manager and performs cleanup operations.
        /// </summary>
        public void Disable()
        {
            LogHelper.Debug("Disabling OmegaWarheadManager. Cleaning up...");
            Cleanup();
        }

        /// <summary>
        /// Cleans up all active coroutines and resets the Omega Warhead state.
        /// </summary>
        public void Cleanup()
        {
            LogHelper.Debug($"Cleaning up OmegaWarheadManager. OmegaActivated: {_omegaActivated}, Coroutines: {_coroutines.Count}.");
            Warhead.Scenario = default; // Should trigger the reset logic that automatically resets the warhead to its initial scenario 
            _omegaActivated = false;
            _omegaDetonated = false;
            Map.ResetColorOfLights();
            foreach (CoroutineHandle coroutine in _coroutines)
            {
                Timing.KillCoroutines(coroutine);
            }
            _coroutines.Clear();
        }
        #endregion

        #region Coroutine Management
        /// <summary>
        /// Adds one or more coroutine handles to the manager's coroutine list.
        /// </summary>
        /// <param name="handles">The coroutine handles to add.</param>
        public void AddCoroutines(params CoroutineHandle[] handles)
        {
            foreach (CoroutineHandle handle in handles)
            {
                _coroutines.Add(handle);
            }
        }
        #endregion

        #region Warhead Event Handlers
        /// <summary>
        /// Handles the warhead start event, initiating the Omega Warhead sequence if conditions are met.
        /// </summary>
        /// <param name="ev">The warhead starting event arguments.</param>
        public void HandleWarheadStart(WarheadStartingEventArgs ev)
        {
            if (!ev.IsAllowed) return;

            if (IsOmegaActive ||  IsOmegaDetonated)
            {
                ev.IsAllowed = false;
                return;
            }

            #region Generator Check
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
            #endregion

            #region Omega Activation
            LogHelper.Debug("WarheadStart triggered. Initiating Omega sequence...");


            _plugin.WarheadMethods.StartSequence(_plugin.Config.TimeToDetonation);


            LogHelper.Debug("HandleWarheadStart called.");

            LogHelper.Debug("Omega is being activated during warhead start, blocking default warhead.");
            ev.IsAllowed = false; // Prevent default warhead from triggering
            #endregion
        }

        /// <summary>
        /// Handles the warhead stop event, stopping the Omega Warhead sequence if allowed by configuration.
        /// </summary>
        /// <param name="ev">The warhead stopping event arguments.</param>
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

            if (_plugin.Config.ResetOmegaOnWarheadStop && IsOmegaDetonated)
            {
                LogHelper.Debug("WarheadStop triggered. Resetting Omega sequence...");
                _plugin.WarheadMethods.ResetSequence();
            }

            LogHelper.Debug("Omega is active during warhead stop, stopping Omega...");
            _plugin.WarheadMethods.StopSequence();
            ev.IsAllowed = false;
        }

        /// <summary>
        /// Handles the warhead detonation event, blocking default detonation if Omega Warhead is active.
        /// </summary>
        /// <param name="ev">The warhead detonating event arguments.</param>
        public void HandleWarheadDetonate(WarheadDetonatingEventArgs ev)
        {
            LogHelper.Debug("HandleWarheadDetonate called.");
            if (!IsOmegaActive) return;

            LogHelper.Debug("Omega is active during detonation, attempting to block default warhead detonation.");
            ev.IsAllowed = false;
        }
        #endregion

        #region Coroutine Handlers
        /// <summary>
        /// Manages the countdown sequence for the Omega Warhead, including notifications and light effects.
        /// </summary>
        /// <param name="timeToDetonation">The total time in seconds until detonation.</param>
        /// <returns>An enumerator for coroutine execution.</returns>
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
                        float messageDuration = NotificationUtility.CalculateCassieMessageDuration(message);
                        LogHelper.Debug($"Cassie message '{message}' duration: {messageDuration}s");
                        NotificationUtility.SendCassieMessage(message);

                        if (notifyTime <= 5) Map.TurnOffLights(0.75f);
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

        /// <summary>
        /// Manages the helicopter evacuation sequence, including broadcast and coroutine initiation.
        /// </summary>
        /// <returns>An enumerator for coroutine execution.</returns>
        public IEnumerator<float> HandleHelicopter()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.HelicopterBroadcastDelay);
            if (IsOmegaActive)
            {
                NotificationUtility.BroadcastHelicopterCountdown();
                _coroutines.Add(Timing.RunCoroutine(_plugin.PlayerMethods.HandleHelicopterEscape(), "OmegaHeliEvacuation"));
            }
        }

        /// <summary>
        /// Manages the unlocking and locking of checkpoint doors during the Omega Warhead sequence.
        /// </summary>
        /// <returns>An enumerator for coroutine execution.</returns>
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

        /// <summary>
        /// Manages the detonation sequence for the Omega Warhead, including final notifications and player effects.
        /// </summary>
        /// <returns>An enumerator for coroutine execution.</returns>
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
        #endregion
    }
    #endregion
}