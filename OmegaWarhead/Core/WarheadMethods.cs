namespace OmegaWarhead
{
    using CustomPlayerEffects;
    using Exiled.API.Enums;
    using LabApi.Features.Wrappers;
    using MEC;
    using OmegaWarhead.Core.AudioUtils;
    using OmegaWarhead.Core.LoggingUtils;
    using OmegaWarhead.NotificationUtils;
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
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="WarheadMethods"/> class.
        /// </summary>
        /// <param name="plugin">Reference to the core plugin instance.</param>
        public WarheadMethods(Plugin plugin) => _plugin = plugin;
        #endregion

        #region Sequence Management
        /// <summary>
        /// Starts the Omega Warhead sequence with the specified detonation time.
        /// </summary>
        /// <param name="timeToDetonation">The time in seconds until detonation.</param>
        public void StartSequence(float timeToDetonation)
        {

                Plugin.Singleton.OmegaManager.IsOmegaActive = true;
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

            #region Round Configuration
            // Lock round immediately to prevent automatic ending  
            _plugin.RoundController.SetAutoRoundEndLock(true);
            #endregion

            #region Light Configuration
            Color lightColor = new Color(_plugin.Config.LightsColorR, _plugin.Config.LightsColorG, _plugin.Config.LightsColorB);
            LogHelper.Debug($"Changing room lights to color: R={lightColor.r}, G={lightColor.g}, B={lightColor.b}");
            Timing.CallDelayed(_plugin.Config.DelayBeforeOmegaSequence, () => { if (_plugin.OmegaManager.IsOmegaActive) Map.SetColorOfLights(lightColor); });

            #endregion

            #region Notifications
            Timing.CallDelayed(_plugin.Config.DelayBeforeOmegaSequence, () =>
            {
                if (_plugin.OmegaManager.IsOmegaActive)
                {
                    NotificationUtility.SendImportantCassieMessage(_plugin.Config.StartingOmegaCassie, _plugin.Config.StartingOmegaMessage);
                    NotificationUtility.BroadcastOmegaActivation();
                    _plugin.AudioManager.PlayOmegaSiren();
                }
            });
            #endregion

            #region Coroutine Setup
            string[] countdownMessages = OmegaWarheadManager.GetNotifyTimes().Select(notifyTime => NotificationUtility.GetCassieCounterNotifyMessage(notifyTime)).ToArray();
            float messageDurationAdjustment = NotificationUtility.CalculateTotalMessagesDurations(1f, countdownMessages);
            LogHelper.Debug($"Adjusting timeToDetonation by {messageDurationAdjustment}s for Cassie messages.");
            float adjustedTime = timeToDetonation + messageDurationAdjustment;


            Plugin.Singleton.OmegaManager.AddCoroutines(
                    Timing.RunCoroutine(Plugin.Singleton.OmegaManager.HandleCountdown(timeToDetonation), "OmegaCountdown"),
                    Timing.RunCoroutine(Plugin.Singleton.OmegaManager.HandleHelicopter(), "OmegaHeli"),
                    Timing.RunCoroutine(Plugin.Singleton.OmegaManager.HandleCheckpointDoors(), "OmegaCheckpoints")
                );
            #endregion
        }
        /// <summary>
        /// Stops the Omega Warhead sequence and performs cleanup.
        /// </summary>
        public void StopSequence()
        {
            Plugin.Singleton.OmegaManager.IsOmegaActive = false;
            LogHelper.Debug("Stopping Omega Warhead sequence.");

            NotificationUtility.SendImportantCassieMessage(Plugin.Singleton.Config.StoppingOmegaCassie, _plugin.Config.StoppingOmegaMessage);
            Plugin.Singleton.OmegaManager.Cleanup();
        }

        /// <summary>
        /// Resets the Omega Warhead state to its initial configuration.
        /// </summary>
        public void ResetSequence()
        {
            LogHelper.Debug("Resetting Omega Warhead state.");
            Plugin.Singleton.OmegaManager.Cleanup();
            Plugin.Singleton.OmegaManager.Init();
        }
        #endregion
    }
    #endregion
}