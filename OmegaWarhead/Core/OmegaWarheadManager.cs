using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Extensions;
using LabApi.Extensions.Misc;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MEC;
using OmegaWarhead.Core.RoundScenarioUtils;
using OmegaWarhead.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DoorLockReason = Interactables.Interobjects.DoorUtils.DoorLockReason;
using Logger = LabApi.Extensions.Misc.iLogger;

namespace OmegaWarhead
{
    /// <summary>
    /// Core operational controller managing the alternative Omega Warhead sequence, countdown execution timelines, 
    /// and facility environmental lockdowns inside the LabAPI framework.
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
        public OmegaWarheadManager(Plugin plugin) => _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
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
        public void Init()
        {
            Logger.Debug(nameof(OmegaWarheadManager), "Initializing subsystem state components.", _plugin.Config.Debug);
            Cleanup();
        }

        public void Disable()
        {
            Logger.Debug(nameof(OmegaWarheadManager), "Disabling subsystem engine. Initiating cascade cleanup...", _plugin.Config.Debug);
            Cleanup();
        }

        public void Cleanup()
        {
            Logger.Debug(nameof(OmegaWarheadManager), $"Executing operational purge. Current activation state: {_omegaActivated}", _plugin.Config.Debug);

            // Revert default warhead state behavior tracking
            Warhead.Scenario = default;

            _omegaActivated = false;
            _omegaDetonated = false;

            _plugin.AudioManager?.StopOmegaSiren();

            // Revert room lighting spectrum channels using collection-wide extensions fluently
            Room.List.SetLightsColor(Color.white);

            // Prevent thread leaking by systematically killing background operations atomically via collection pipes
            CoroutineTags.AllStaticTags.KillCoroutines();

            Logger.Debug(nameof(OmegaWarheadManager), "All background coroutines bound to static tags have been structurally terminated.", _plugin.Config.Debug);
            _plugin.PlayerMethods?.Clean();
        }
        #endregion

        #region LabAPI Core Event Interceptors
        public void HandleWarheadStart(WarheadStartingEventArgs ev)
        {
            if (ev is null || !ev.IsAllowed) return;

            if (IsOmegaActive || IsOmegaDetonated)
            {
                ev.IsAllowed = false;
                return;
            }

            // Replaced manual LINQ allocation sweeps with high-performance native Map extensions
            int activeGenerators = MapExtensions.GetEngagedGeneratorsCount();
            Logger.Debug(nameof(OmegaWarheadManager), $"Current online generator grid load: {activeGenerators}/{_plugin.Config.GeneratorsNumGuaranteeOmega}", _plugin.Config.Debug);

            if (activeGenerators < _plugin.Config.GeneratorsNumGuaranteeOmega)
            {
                float chance = _plugin.Config.ReplaceAlphaChance;
                float roll = SafeRandom.Range(0f, 100f);
                float bonus = activeGenerators * _plugin.Config.GeneratorsIncreaseChanceBy;

                if (chance < 100f && roll >= (chance + bonus))
                {
                    Logger.Debug(nameof(OmegaWarheadManager), $"Alternative payload swap skipped due to probability matrix logic. Target Chance: {chance}%, Roll Result: {roll} (Bonus: +{bonus}%)", _plugin.Config.Debug);
                    return;
                }
            }
            else
            {
                Logger.Debug(nameof(OmegaWarheadManager), "Alternative payload injection forced by generator threshold completion.", _plugin.Config.Debug);
            }

            Logger.Debug(nameof(OmegaWarheadManager), "Alpha Warhead successfully intercepted. Swapping command pipelines to Omega payload matrix...", _plugin.Config.Debug);
            _plugin.WarheadMethods?.StartSequence(_plugin.Config.TimeToDetonation);

            ev.IsAllowed = false;
        }

        public void HandleWarheadStop(WarheadStoppingEventArgs ev)
        {
            if (ev is null || !ev.IsAllowed) return;

            Logger.Debug(nameof(OmegaWarheadManager), "Intercepted sequence abort request tracking loop.", _plugin.Config.Debug);
            if (!IsOmegaActive) return;

            if (!_plugin.Config.IsStopAllowed)
            {
                Logger.Debug(nameof(OmegaWarheadManager), "Sequence abort canceled: Configuration constraints deny player manual cancellation.", _plugin.Config.Debug);
                ev.IsAllowed = false;
                return;
            }

            if (_plugin.Config.ResetOmegaOnWarheadStop)
            {
                Logger.Debug(nameof(OmegaWarheadManager), "Abort accepted. Full chronological timeline normalization triggered.", _plugin.Config.Debug);
                _plugin.WarheadMethods?.ResetSequence();
            }
            else
            {
                Logger.Debug(nameof(OmegaWarheadManager), "Abort accepted. Pausing sequence timeline at current coordinates.", _plugin.Config.Debug);
                _plugin.WarheadMethods?.StopSequence();
            }

            ev.IsAllowed = false;
        }

        public void HandleWarheadDetonate(WarheadDetonatingEventArgs ev)
        {
            if (ev is null || !IsOmegaActive) return;

            Logger.Debug(nameof(OmegaWarheadManager), "Base engine detonation code blocked successfully to hand over execution to alternative script matrix.", _plugin.Config.Debug);
            ev.IsAllowed = false;
        }
        #endregion

        #region High-Performance Execution Coroutines
        public IEnumerator<float> HandleCountdown(float timeToDetonation)
        {
            double warheadStartTime = Timing.LocalTime + timeToDetonation;
            var validNotifyTimes = NotifyTimes.Where(t => t <= timeToDetonation).OrderByDescending(t => t);

            foreach (int notifyTime in validNotifyTimes)
            {
                if (!IsOmegaActive) yield break;

                // MASTER-LEVEL ARCHITECTURE ALIGNMENT:
                // Utilizing the high-clarity phonetic countdown builder embedded natively within Fluent extensions
                string message = notifyTime.ToCassieCountdown("Seconds until Omega Warhead Detonation");
                if (string.IsNullOrEmpty(message)) continue;

                double msgDuration = CassieExtensions.CalculateCassieMessageDuration(message, _plugin.Config.CassieNotifySpeed);
                double buffer = _plugin.Config.CassieTimingBuffer;

                if (notifyTime > 5)
                {
                    double targetStart = warheadStartTime - notifyTime - msgDuration - buffer;
                    double now = Timing.LocalTime;

                    if (now > targetStart) continue;

                    double wait = targetStart - now;
                    if (wait > 0)
                        yield return Timing.WaitForSeconds((float)wait);
                }
                else
                {
                    double targetStart = warheadStartTime - notifyTime;
                    double wait = targetStart - Timing.LocalTime;

                    if (wait > 0)
                        yield return Timing.WaitForSeconds((float)wait);
                }

                if (!IsOmegaActive) yield break;

                CassieExtensions.ProcessAndDispatchMessage(
                    message,
                    $"Warhead -> {notifyTime}...",
                    _plugin.Config.CassieMessageClearBeforeWarheadMessage,
                    "pitch_0.95",
                    _plugin.Config.CassieMessagePriority,
                    _plugin.Config.DisableCassieMessages
                );

                if (notifyTime <= 5)
                {
                    // Dim environmental lighting grids across all rooms smoothly using a rapid iteration cascade
                    foreach (Room room in Room.List)
                    {
                        room.TurnOffLights(0.75f);
                    }
                    yield return Timing.WaitForSeconds(0.75f);
                }

                yield return Timing.WaitForSeconds((float)(msgDuration + buffer));
            }

            if (!IsOmegaActive) yield break;

            string finalSirenMessage = ".G5 Pitch_1.75 .G5 .G5 .G5 .G5 .G5";
            double sirenDuration = CassieExtensions.CalculateCassieMessageDuration(finalSirenMessage, _plugin.Config.CassieNotifySpeed);
            double finalBuffer = _plugin.Config.CassieTimingBuffer;

            CassieExtensions.ProcessAndDispatchMessage(
                finalSirenMessage,
                "Warhead...",
                _plugin.Config.CassieMessageClearBeforeWarheadMessage,
                "pitch_0.95",
                _plugin.Config.CassieMessagePriority,
                _plugin.Config.DisableCassieMessages
            );

            yield return Timing.WaitForSeconds((float)(sirenDuration + finalBuffer));

            if (IsOmegaActive)
            {
                Timing.RunCoroutine(HandleDetonation(), CoroutineTags.Detonation);
            }
        }

        public IEnumerator<float> HandleHelicopter(float totalDetonationTime)
        {
            float delay = totalDetonationTime - _plugin.Config.HelicopterBroadcastTMinus;
            if (delay > 0f)
            {
                yield return Timing.WaitForSeconds(delay);
            }

            if (!IsOmegaActive) yield break;

            CassieExtensions.ProcessAndDispatchMessage(
                _plugin.Config.HeliIncomingCassie,
                _plugin.Config.HelicopterIncomingMessage,
                _plugin.Config.CassieMessageClearBeforeImportant, "pitch_1.05",
                _plugin.Config.CassieMessageImportantPriority,
                _plugin.Config.DisableCassieMessages
            );

            Timing.RunCoroutine(_plugin.PlayerMethods.HandleHelicopterEvacuation(), CoroutineTags.HeliEvacuation);
        }

        public IEnumerator<float> HandleCheckpointDoors(float totalDetonationTime)
        {
            float delay = totalDetonationTime - _plugin.Config.OpenAndLockCheckpointDoorsTMinus;
            if (delay > 0f)
            {
                yield return Timing.WaitForSeconds(delay);
            }

            if (!IsOmegaActive) yield break;

            CassieExtensions.ProcessAndDispatchMessage(
                _plugin.Config.CheckpointUnlockCassie,
                _plugin.Config.CheckpointUnlockMessage,
                _plugin.Config.CassieMessageClearBeforeImportant,
                "pitch_1.05",
                _plugin.Config.CassieMessageImportantPriority,
                _plugin.Config.DisableCassieMessages
            );

            // MASTER-LEVEL ARCHITECTURE ALIGNMENT:
            // Complete elimination of nested loops and string checks.
            // Using a single fluent filter and mass-action execution cascade safely.
            Door.List.WhereNameIn(
                DoorName.LczCheckpointA,
                DoorName.LczCheckpointB,
                DoorName.HczCheckpoint,
                DoorName.EzGateA,
                DoorName.EzGateB,
                DoorName.SurfaceGate
            ).OpenAndLock(DoorLockReason.Warhead, playSound: true);
        }

        public IEnumerator<float> HandleDetonation()
        {
            string detonationMessage = _plugin.Config.DetonatingOmegaCassie;
            double messageDuration = CassieExtensions.CalculateCassieMessageDuration(detonationMessage, _plugin.Config.CassieDetonationSpeed);
            float buffer = _plugin.Config.CassieTimingBuffer;

            Logger.Debug(nameof(OmegaWarheadManager), $"Broadcasting final detonation transmission. Calculated execution block width: {messageDuration}s", _plugin.Config.Debug);

            CassieExtensions.ProcessAndDispatchMessage(
                detonationMessage,
                _plugin.Config.DetonatingOmegaMessage,
                _plugin.Config.CassieMessageClearBeforeWarheadMessage,
                "pitch_0.65",
                _plugin.Config.CassieMessagePriority,
                _plugin.Config.DisableCassieMessages
            );

            yield return Timing.WaitForSeconds((float)messageDuration + buffer);

            _plugin.PlayerMethods?.HandlePlayersOnNuke();

            _omegaActivated = false;
            _omegaDetonated = true;

            _plugin.AudioManager?.PlayEndingMusic();
            _plugin.RoundController?.ExecuteScenario(_plugin.RoundController.GetScenario<DetonationEndingScenario>());

            // Permanent terminal jamming loop to block base game radio or broadcast interventions
            while (true)
            {
                if (_plugin.Config.CassieMessageClearBeforeWarheadMessage)
                {
                    CassieExtensions.CassieClear();
                }
                yield return Timing.WaitForSeconds(0.175f);
            }
        }
        #endregion
    }
}