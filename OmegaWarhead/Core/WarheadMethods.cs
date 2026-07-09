using LabApi.Extensions;
using LabApi.Features.Wrappers;
using MEC;
using OmegaWarhead.Shared;
using System;
using System.Linq;
using UnityEngine;
using Logger = LabApi.Extensions.Misc.iLogger;

namespace OmegaWarhead
{
    /// <summary>
    /// Manages the activation, stopping, resetting, and detonation lifecycle of the Omega Warhead sequence.
    /// </summary>
    public class WarheadMethods
    {
        #region Private Repositories
        private readonly Plugin _plugin;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="WarheadMethods"/> class bound to a parent lifecycle scope.
        /// </summary>
        public WarheadMethods(Plugin plugin) => _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        #endregion

        #region Sequence Management
        /// <summary>
        /// Starts the Omega Warhead sequence with the specified detonation time.
        /// </summary>
        /// <param name="timeToDetonation">The time in seconds until detonation.</param>
        public void StartSequence(float timeToDetonation)
        {
            if (_plugin.OmegaManager is null) return;

            _plugin.OmegaManager.IsOmegaActive = true;
            Logger.Debug(nameof(WarheadMethods), "Starting Omega Warhead sequence chain...", _plugin.Config.Debug);
            Activate(timeToDetonation);
        }

        /// <summary>
        /// Initiates the core structural sequence for the alternative alternative nuclear detonation protocol.
        /// </summary>
        /// <param name="timeToDetonation">The time in seconds until detonation.</param>
        public void Activate(float timeToDetonation)
        {
            Logger.Debug(nameof(WarheadMethods), $"Activating Omega Warhead with detonation time: {timeToDetonation}s.", _plugin.Config.Debug);

            // 1. Lock round immediately to prevent automatic ending transitions
            _plugin.RoundController?.SetAutoRoundEndLock(true);

            // 2. Light Configuration Pipeline
            Color lightColor = new Color(_plugin.Config.LightsColorR, _plugin.Config.LightsColorG, _plugin.Config.LightsColorB);
            Logger.Debug(nameof(WarheadMethods), $"Changing room lights to color spectrum: R={lightColor.r}, G={lightColor.g}, B={lightColor.b}", _plugin.Config.Debug);

            var coroutineLight = Timing.CallDelayed(_plugin.Config.DelayBeforeOmegaSequence, () =>
            {
                // Replaced non-fluent map calls with collection-wide extensions targeting light controllers directly
                if (_plugin.OmegaManager is not null && _plugin.OmegaManager.IsOmegaActive)
                    Room.List.SetLightsColor(lightColor);
            });
            coroutineLight.Tag = CoroutineTags.Lights;

            // 3. Automated Broadcast and Vocal Notifications Pipeline
            var coroutineCore = Timing.CallDelayed(_plugin.Config.DelayBeforeOmegaSequence, () =>
            {
                if (_plugin.OmegaManager is not null && _plugin.OmegaManager.IsOmegaActive)
                {
                    // Eradicated NotificationUtility completely, streaming directly via verified Fluent API tracks
                    CassieExtensions.ProcessAndDispatchMessage(
                        _plugin.Config.StartingOmegaCassie,
                        _plugin.Config.StartingOmegaMessage,
                        _plugin.Config.CassieMessageClearBeforeImportant,
                        "pitch_1.05",
                        _plugin.Config.CassieMessageImportantPriority,
                        _plugin.Config.DisableCassieMessages
                    );

                    PlayerExtensions.BroadcastHintToAll(_plugin.Config.ActivatedMessage, 6f);
                    _plugin.AudioManager?.PlayOmegaSiren();
                }
            });
            coroutineCore.Tag = "Omega-Core";

            // 4. Timeline Duration Processing & Coroutine Scheduling
            // Utilizing phonetic countdown strings generated natively through fluent integer conversions
            string[] countdownMessages = _plugin.Config.NotifyTimes
                .Select(notifyTime => notifyTime.ToCassieCountdown("Seconds until Omega Warhead Detonation"))
                .ToArray();

            // Intersecting array listings to aggregate timeline bounds cleanly
            double messageDurationAdjustment = CassieExtensions.CalculateTotalMessagesDurations(countdownMessages, (float)_plugin.Config.CassieNotifySpeed);
            Logger.Debug(nameof(WarheadMethods), $"Adjusting timeToDetonation by {messageDurationAdjustment}s for Cassie messages.", _plugin.Config.Debug);

            double adjustedTime = timeToDetonation + messageDurationAdjustment;

            // Bound runtime coroutines directly to explicit instance context tracking tags safely
            Timing.RunCoroutine(_plugin.OmegaManager.HandleCountdown((float)adjustedTime), CoroutineTags.Countdown);
            Timing.RunCoroutine(_plugin.OmegaManager.HandleHelicopter((float)adjustedTime), CoroutineTags.Helicopter);
            Timing.RunCoroutine(_plugin.OmegaManager.HandleCheckpointDoors((float)adjustedTime), CoroutineTags.Checkpoints);
        }

        /// <summary>
        /// Stops the Omega Warhead sequence and performs cleanup.
        /// </summary>
        public void StopSequence()
        {
            if (_plugin.OmegaManager is null) return;

            _plugin.OmegaManager.IsOmegaActive = false;
            Logger.Debug(nameof(WarheadMethods), "Stopping Omega Warhead sequence.", _plugin.Config.Debug);

            CassieExtensions.ProcessAndDispatchMessage(
                _plugin.Config.StoppingOmegaCassie,
                _plugin.Config.StoppingOmegaMessage,
                _plugin.Config.CassieMessageClearBeforeImportant, "pitch_0.95",
                _plugin.Config.CassieMessagePriority,
                _plugin.Config.DisableCassieMessages
            );

            _plugin.OmegaManager.Cleanup();
        }

        /// <summary>
        /// Resets the Omega Warhead state to its initial configuration.
        /// </summary>
        public void ResetSequence()
        {
            Logger.Debug(nameof(WarheadMethods), "Resetting Omega Warhead state.", _plugin.Config.Debug);
            _plugin.OmegaManager?.Init();
        }
        #endregion
    }
}