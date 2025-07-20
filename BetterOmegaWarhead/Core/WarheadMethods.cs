namespace BetterOmegaWarhead
{
    using BetterOmegaWarhead.Core.LoggingUtils;
    using BetterOmegaWarhead.NotificationUtils;
    using LabApi.Features.Wrappers;
    using MEC;
    using System;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Manages the activation, stopping, resetting, and detonation of the Omega Warhead sequence.
    /// </summary>
    #region WarheadMethods Class
    public class WarheadMethods
    {
        #region Fields
        private readonly Plugin _plugin;
        private readonly OmegaWarheadManager _omegaWarheadManager;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="WarheadMethods"/> class.
        /// </summary>
        /// <param name="plugin">Reference to the core plugin instance.</param>
        public WarheadMethods(Plugin plugin)
        {
            _plugin = plugin;
            _omegaWarheadManager = _plugin.OmegaManager;
        }
        #endregion

        #region Sequence Management
        /// <summary>
        /// Starts the Omega Warhead sequence with the specified detonation time.
        /// </summary>
        /// <param name="timeToDetonation">The time in seconds until detonation.</param>
        public void StartSequence(float timeToDetonation)
        {
            LogHelper.Debug("Starting Omega Warhead sequence chain...");
            Activate(timeToDetonation);
        }

        /// <summary>
        /// Activates the Omega Warhead, setting up lights, notifications, and coroutines.
        /// </summary>
        /// <param name="timeToDetonation">The time in seconds until detonation.</param>
        public void Activate(float timeToDetonation)
        {
            LogHelper.Debug($"Activating Omega Warhead with detonation time: {timeToDetonation}s.");

            #region Light Configuration
            Color lightColor = new Color(_plugin.Config.LightsColorR, _plugin.Config.LightsColorG, _plugin.Config.LightsColorB);
            LogHelper.Debug($"Changing room lights to color: R={lightColor.r}, G={lightColor.g}, B={lightColor.b}");
            Timing.CallDelayed(_plugin.Config.DelayBeforeOmegaSequence, () => {Map.SetColorOfLights(lightColor)});

            #endregion

            #region Notifications
            NotificationUtility.SendImportantCassieMessage(_plugin.Config.StartingOmegaCassie);
            NotificationUtility.BroadcastOmegaActivation();
            #endregion

            #region Coroutine Setup
            string[] countdownMessages = OmegaWarheadManager.GetNotifyTimes().Select(notifyTime => NotificationUtility.GetCassieCounterNotifyMessage(notifyTime)).ToArray();
            float messageDurationAdjustment = NotificationUtility.CalculateTotalMessagesDurations(1f, countdownMessages);
            LogHelper.Debug($"Adjusting timeToDetonation by {messageDurationAdjustment}s for Cassie messages.");
            float adjustedTime = timeToDetonation - messageDurationAdjustment;
            _omegaWarheadManager.AddCoroutines(
                Timing.RunCoroutine(_plugin.OmegaManager.HandleCountdown(adjustedTime), "OmegaCountdown"),
                Timing.RunCoroutine(_plugin.OmegaManager.HandleHelicopter(), "OmegaHeli"),
                Timing.RunCoroutine(_plugin.OmegaManager.HandleCheckpointDoors(), "OmegaCheckpoints")
            );
            #endregion

            Warhead.DetonationTime = timeToDetonation; // activate synced Alpha
        }

        /// <summary>
        /// Stops the Omega Warhead sequence and performs cleanup.
        /// </summary>
        public void StopSequence()
        {
            LogHelper.Debug("Stopping Omega Warhead sequence.");

            NotificationUtility.SendImportantCassieMessage(Plugin.Singleton.Config.StoppingOmegaCassie);
            _omegaWarheadManager.Cleanup();
        }

        /// <summary>
        /// Resets the Omega Warhead state to its initial configuration.
        /// </summary>
        public void ResetSequence()
        {
            LogHelper.Debug("Resetting Omega Warhead state.");
            _omegaWarheadManager.Cleanup();
        }
        #endregion

        #region Detonation Logic
        /// <summary>
        /// Executes the detonation sequence for the Omega Warhead, affecting doors and lights.
        /// </summary>
        public static void DetonateSequence()
        {
            #region Warhead Detonation
            // Alpha warhead detonation  
            Warhead.Detonate();
            Warhead.Shake();
            #endregion

            #region Facility Effects
            // Facility-wide door effects  
            foreach (Room room in Room.List)
            {
                foreach (LabApi.Features.Wrappers.Door door in room.Doors)
                {
                    door.IsOpened = true;
                    if (door is LabApi.Features.Wrappers.BreakableDoor breakable && !breakable.IsBroken)
                        breakable.TryBreak();
                    door.Lock(Interactables.Interobjects.DoorUtils.DoorLockReason.Warhead, true);
                }

                // Turn off all lights  
                foreach (var lightController in room.AllLightControllers)
                {
                    lightController.LightsEnabled = false;
                }
            }
            #endregion
        }
        #endregion
    }
    #endregion
}