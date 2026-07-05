namespace OmegaWarhead
{
    using LabApi.Features.Wrappers;
    using MEC;
    using OmegaWarhead.Core.LoggingUtils;
    using OmegaWarhead.Shared;
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
        /// Initiates the core structural sequence for the alternative alternative nuclear detonation protocol.
        /// </summary>
        public void Activate(float timeToDetonation)
        {
            LogHelper.Debug(nameof(WarheadMethods), $"Activating Omega Warhead with detonation time: {timeToDetonation}s.");

            #region Round Configuration
            // Lock round immediately to prevent automatic ending  
            _plugin.RoundController.SetAutoRoundEndLock(true);
            #endregion

            #region Light Configuration
            Color lightColor = new Color(_plugin.Config.LightsColorR, _plugin.Config.LightsColorG, _plugin.Config.LightsColorB);
            LogHelper.Debug(nameof(WarheadMethods), $"Changing room lights to color: R={lightColor.r}, G={lightColor.g}, B={lightColor.b}");

            var coroutine_light = Timing.CallDelayed(_plugin.Config.DelayBeforeOmegaSequence, () =>
            {
                if (_plugin.OmegaManager.IsOmegaActive) Map.SetColorOfLights(lightColor);
            });
            coroutine_light.Tag = "Omega-Lights";
            #endregion

            #region Notifications
            var coroutine_core = Timing.CallDelayed(_plugin.Config.DelayBeforeOmegaSequence, () =>
            {
                if (_plugin.OmegaManager.IsOmegaActive)
                {
                    NotificationUtility.SendImportantCassieMessage(_plugin.Config.StartingOmegaCassie, _plugin.Config.StartingOmegaMessage);
                    NotificationUtility.BroadcastOmegaActivation();
                    _plugin.AudioManager.PlayOmegaSiren();
                }
            });
            coroutine_core.Tag = "Omega-Core";
            #endregion

            #region Coroutine Setup
            // FIX: Replaced obsolete static method call with clean instance-based property pipeline access
            string[] countdownMessages = _plugin.OmegaManager.NotifyTimes
                .Select(notifyTime => NotificationUtility.GetCassieCounterNotifyMessage(notifyTime))
                .ToArray();

            double messageDurationAdjustment = NotificationUtility.CalculateTotalMessagesDurations((float)_plugin.Config.CassieNotifySpeed, countdownMessages);
            LogHelper.Debug(nameof(WarheadMethods), $"Adjusting timeToDetonation by {messageDurationAdjustment}s for Cassie messages.");

            double adjustedTime = timeToDetonation + messageDurationAdjustment;

            // Bound runtime coroutines directly to the explicit instance context mappings
            Timing.RunCoroutine(_plugin.OmegaManager.HandleCountdown((float)adjustedTime), CoroutineTags.Countdown);
            Timing.RunCoroutine(_plugin.OmegaManager.HandleHelicopter((float)adjustedTime), CoroutineTags.Helicopter);
            Timing.RunCoroutine(_plugin.OmegaManager.HandleCheckpointDoors((float)adjustedTime), CoroutineTags.Checkpoints);
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
            Plugin.Singleton.OmegaManager.Init();
        }
        #endregion
    }
    #endregion
}