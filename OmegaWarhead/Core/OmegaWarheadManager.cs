namespace OmegaWarhead
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using MEC;
    using LabApi.Events.Arguments.WarheadEvents;
    using LabApi.Features.Wrappers;
    using LabApi.Features.Enums;
    using OmegaWarhead.Core.LoggingUtils;
    using OmegaWarhead.Core.RoundScenarioUtils;
    using OmegaWarhead.NotificationUtils;
    using OmegaWarhead.Shared;

    /// <summary>
    /// Core operational controller managing the alternative Omega Warhead sequence, countdown execution timelines, 
    /// and facility environmental lockouts inside the LabAPI framework.
    /// </summary>
    public class OmegaWarheadManager
    {
        #region Private Repositories & States
        private readonly Plugin _plugin;
        private bool _omegaActivated;
        private bool _omegaDetonated;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="OmegaWarheadManager"/> class linked to a parent lifecycle scope.
        /// </summary>
        public OmegaWarheadManager(Plugin plugin) => _plugin = plugin;
        #endregion

        #region Operational Properties
        /// <summary>  
        /// Gets or sets a value indicating whether the alternative Omega Warhead sequence is currently active.  
        /// </summary>  
        public bool IsOmegaActive
        {
            get => _omegaActivated;
            set => _omegaActivated = value;
        }

        /// <summary>  
        /// Gets a value indicating whether the facility has been successfully erased by the nuclear blast.  
        /// </summary>  
        public bool IsOmegaDetonated => _omegaDetonated;

        /// <summary>
        /// Resolves the collection of active structural notification timestamps from the configuration layer.
        /// </summary>
        public List<int> NotifyTimes => _plugin.Config.NotifyTimes;
        #endregion

        #region Subsystem Lifecycle Management
        /// <summary>
        /// Establishes early hardware operational states and clears leftover round artifacts.
        /// </summary>
        public void Init()
        {
            LogHelper.Debug(nameof(OmegaWarheadManager), "Initializing subsystem state components.");
            Cleanup();
        }

        /// <summary>
        /// Safely detaches the manager layer and executes an absolute resource reclamation sequence.
        /// </summary>
        public void Disable()
        {
            LogHelper.Debug(nameof(OmegaWarheadManager), "Disabling subsystem engine. Initiating cascade cleanup...");
            Cleanup();
        }

        /// <summary>
        /// Terminates running execution threads, releases lighting spectrum channels, and purges state variables.
        /// </summary>
        public void Cleanup()
        {
            LogHelper.Debug(nameof(OmegaWarheadManager), $"Executing operational purge. Current activation state: {_omegaActivated}");

            // Revert default warhead state behavior tracking
            Warhead.Scenario = default;

            _omegaActivated = false;
            _omegaDetonated = false;

            _plugin.AudioManager?.StopOmegaSiren();
            Map.ResetColorOfLights();

            // Prevent thread leaking by systematically killing background operations
            foreach (string tag in CoroutineTags.AllStaticTags)
            {
                Timing.KillCoroutines(tag);
            }

            LogHelper.Debug(nameof(OmegaWarheadManager), "All background coroutines bound to static tags have been structurally terminated.");
            _plugin.PlayerMethods?.Clean();
        }
        #endregion

        #region LabAPI Core Event Interceptors
        /// <summary>
        /// Evaluates active round parameters during Alpha activation and dynamically decides whether to swap payloads with Omega.
        /// </summary>
        public void HandleWarheadStart(WarheadStartingEventArgs ev)
        {
            if (!ev.IsAllowed) return;

            if (IsOmegaActive || IsOmegaDetonated)
            {
                ev.IsAllowed = false;
                return;
            }

            // Evaluate alpha-to-omega conversion constraints via active automation grids
            int activeGenerators = Generator.List.Count(g => g.Engaged);
            LogHelper.Debug(nameof(OmegaWarheadManager), $"Current online generator grid load: {activeGenerators}/{_plugin.Config.GeneratorsNumGuaranteeOmega}");

            if (activeGenerators < _plugin.Config.GeneratorsNumGuaranteeOmega)
            {
                float chance = _plugin.Config.ReplaceAlphaChance;
                float roll = UnityEngine.Random.Range(0f, 100f);
                float bonus = activeGenerators * _plugin.Config.GeneratorsIncreaseChanceBy;

                if (chance < 100f && roll >= (chance + bonus))
                {
                    LogHelper.Debug(nameof(OmegaWarheadManager), $"Alternative payload swap skipped due to probability matrix logic. Target Chance: {chance}%, Roll Result: {roll} (Bonus: +{bonus}%)");
                    return;
                }
            }
            else
            {
                LogHelper.Debug(nameof(OmegaWarheadManager), "Alternative payload injection forced by generator threshold completion.");
            }

            LogHelper.Debug(nameof(OmegaWarheadManager), "Alpha Warhead successfully intercepted. Swapping command pipelines to Omega payload matrix...");
            _plugin.WarheadMethods.StartSequence(_plugin.Config.TimeToDetonation);

            // Cancel the standard base-game detonation sequence loop
            ev.IsAllowed = false;
        }

        /// <summary>
        /// Intercepts abort commands and verifies authorization parameters to decide if the sequence can be vented.
        /// </summary>
        public void HandleWarheadStop(WarheadStoppingEventArgs ev)
        {
            if (!ev.IsAllowed) return;

            LogHelper.Debug(nameof(OmegaWarheadManager), "Intercepted sequence abort request tracking loop.");
            if (!IsOmegaActive) return;

            if (!_plugin.Config.IsStopAllowed)
            {
                LogHelper.Debug(nameof(OmegaWarheadManager), "Sequence abort canceled: Configuration constraints deny player manual cancellation.");
                ev.IsAllowed = false;
                return;
            }

            if (_plugin.Config.ResetOmegaOnWarheadStop)
            {
                LogHelper.Debug(nameof(OmegaWarheadManager), "Abort accepted. Full chronological timeline normalization triggered.");
                _plugin.WarheadMethods.ResetSequence();
            }
            else
            {
                LogHelper.Debug(nameof(OmegaWarheadManager), "Abort accepted. Pausing sequence timeline at current coordinates.");
                _plugin.WarheadMethods.StopSequence();
            }

            ev.IsAllowed = false;
        }

        /// <summary>
        /// Prevents standard engine internal detonation code from overriding custom ending scenarios.
        /// </summary>
        public void HandleWarheadDetonate(WarheadDetonatingEventArgs ev)
        {
            if (!IsOmegaActive) return;

            LogHelper.Debug(nameof(OmegaWarheadManager), "Base engine detonation code blocked successfully to hand over execution to alternative script matrix.");
            ev.IsAllowed = false;
        }
        #endregion

        #region High-Performance Execution Coroutines
        /// <summary>
        /// Controls the chronological counting loop, handles early multi-threading sync, and processes the final dramatic timeline stretch.
        /// </summary>
        public IEnumerator<float> HandleCountdown(float timeToDetonation)
        {
            double warheadStartTime = Timing.LocalTime + timeToDetonation;
            var validNotifyTimes = NotifyTimes.Where(t => t <= timeToDetonation).OrderByDescending(t => t);

            foreach (int notifyTime in validNotifyTimes)
            {
                if (!IsOmegaActive) yield break;

                string message = NotificationUtility.GetCassieCounterNotifyMessage(notifyTime);
                if (string.IsNullOrEmpty(message)) continue;

                double msgDuration = NotificationUtility.CalculateCassieMessageDuration(message, _plugin.Config.CassieNotifySpeed);
                double buffer = _plugin.Config.CassieTimingBuffer;

                if (notifyTime > 5)
                {
                    // Phase 1: High-Precision Synchronous Timing
                    double targetStart = warheadStartTime - notifyTime - msgDuration - buffer;
                    double now = Timing.LocalTime;

                    if (now > targetStart) continue; // Skip announcement if processing frame drop detected to avoid queue overlap

                    double wait = targetStart - now;
                    if (wait > 0)
                        yield return Timing.WaitForSeconds((float)wait);
                }
                else
                {
                    // Phase 2: Cinematic Tension Stretcher (Unconditional Finale Playback)
                    double targetStart = warheadStartTime - notifyTime;
                    double wait = targetStart - Timing.LocalTime;

                    if (wait > 0)
                        yield return Timing.WaitForSeconds((float)wait);
                }

                if (!IsOmegaActive) yield break;

                NotificationUtility.SendCassieMessage(message, $"Warhead -> {notifyTime}...");

                // Dim environmental illumination maps during the final crucial steps
                if (notifyTime <= 5)
                {
                    Map.TurnOffLights(0.75f);
                    yield return Timing.WaitForSeconds(0.75f);
                }

                // Intentionally stall execution loop for full voice clip duration to build claustrophobic anxiety
                yield return Timing.WaitForSeconds((float)(msgDuration + buffer));
            }

            if (!IsOmegaActive) yield break;

            // Trigger the absolute final electronic alarm system
            string finalSirenMessage = ".G5 Pitch_1.75 .G5 .G5 .G5 .G5 .G5";
            double sirenDuration = NotificationUtility.CalculateCassieMessageDuration(finalSirenMessage, _plugin.Config.CassieNotifySpeed);
            double finalBuffer = _plugin.Config.CassieTimingBuffer;

            NotificationUtility.SendCassieMessage(finalSirenMessage, "Warhead...");
            yield return Timing.WaitForSeconds((float)(sirenDuration + finalBuffer));

            if (IsOmegaActive)
            {
                Timing.RunCoroutine(HandleDetonation(), CoroutineTags.Detonation);
            }
        }

        /// <summary>
        /// Tracks chronological thresholds to automatically dispatch tactical extraction waves to the surface zone.
        /// </summary>
        public IEnumerator<float> HandleHelicopter()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.HelicopterBroadcastDelay);
            if (!IsOmegaActive) yield break;

            NotificationUtility.BroadcastHelicopterCountdown();
            Timing.RunCoroutine(_plugin.PlayerMethods.HandleHelicopterEvacuation(), CoroutineTags.HeliEvacuation);
        }

        /// <summary>
        /// Processes facility structural breaches, automatically dropping magnetic containment seals on core checkpoint gates.
        /// </summary>
        public IEnumerator<float> HandleCheckpointDoors()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.OpenAndLockCheckpointDoorsDelay);
            if (!IsOmegaActive) yield break;

            NotificationUtility.SendImportantCassieMessage(_plugin.Config.CheckpointUnlockCassie, _plugin.Config.CheckpointUnlockMessage);

            foreach (Door door in Door.List)
            {
                // Modern C# 9.0 logical pattern matching syntax implementation
                if (door.DoorName is DoorName.LczCheckpointA
                                 or DoorName.LczCheckpointB
                                 or DoorName.HczCheckpoint
                                 or DoorName.EzGateA
                                 or DoorName.EzGateB
                                 or DoorName.SurfaceGate)
                {
                    door.IsOpened = true;
                    door.PlayLockBypassDeniedSound();
                    door.Lock(Interactables.Interobjects.DoorUtils.DoorLockReason.Warhead, true);
                }
            }
        }

        /// <summary>
        /// Executes the termination sequence, calculating casualties, shifting post-apocalyptic audio tracks, and jamming regular comms.
        /// </summary>
        public IEnumerator<float> HandleDetonation()
        {
            string detonationMessage = _plugin.Config.DetonatingOmegaCassie;
            double messageDuration = NotificationUtility.CalculateCassieMessageDuration(detonationMessage, _plugin.Config.CassieDetonationSpeed);
            float buffer = _plugin.Config.CassieTimingBuffer;

            LogHelper.Debug(nameof(OmegaWarheadManager), $"Broadcasting final detonation transmission. Calculated execution block width: {messageDuration}s");
            NotificationUtility.SendCassieMessage(detonationMessage, _plugin.Config.DetonatingOmegaMessage);

            yield return Timing.WaitForSeconds((float)messageDuration + buffer);

            // Vaporize unshielded entities on the surface and facility grid layers
            _plugin.PlayerMethods.HandlePlayersOnNuke();

            _omegaActivated = false;
            _omegaDetonated = true;

            _plugin.RoundController.ExecuteScenario(_plugin.RoundController.GetScenario<DetonationEndingScenario>());
            _plugin.AudioManager.PlayEndingMusic();

            // Permanent terminal jamming loop to block base game radio or broadcast interventions
            while (true)
            {
                if (_plugin.Config.CassieMessageClearBeforeWarheadMessage)
                {
                    Announcer.Clear();
                }
                yield return Timing.WaitForSeconds(0.175f);
            }
        }
        #endregion
    }
}