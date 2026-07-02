namespace OmegaWarhead
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using UnityEngine;
    using OmegaWarhead.Core.LoggingUtils;

    /// <summary>
    /// Configuration settings for OmegaWarhead inside the LabAPI execution environment.
    /// </summary>
    public sealed class Config
    {
        #region Core Settings
        /// <summary>
        /// Gets or sets a value indicating whether the plugin is enabled.
        /// </summary>
        [Description("Plugin enabled?")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the chance (0-100) that the Alpha Warhead will be replaced with Omega Warhead.
        /// </summary>
        [Description("Chance that the Alpha Warhead will be replaced with Omega Warhead.")]
        public int ReplaceAlphaChance { get; set; } = 15;

        /// <summary>
        /// Gets or sets the number of engaged generators that guarantees Omega Warhead launch.
        /// </summary>
        [Description("Number of engaged generators that guarantees Omega Warhead launch.")]
        public int GeneratorsNumGuaranteeOmega { get; set; } = 3;

        /// <summary>
        /// Gets or sets the percentage increase in chance per activated generator.
        /// </summary>
        [Description("How much the chance is increased per each activated generator.")]
        public float GeneratorsIncreaseChanceBy { get; set; } = 15;

        /// <summary>
        /// Gets or sets a value indicating whether players can stop the Omega Warhead.
        /// </summary>
        [Description("Whether players can stop the Omega Warhead.")]
        public bool IsStopAllowed { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the Omega countdown should be reset on stop.
        /// </summary>
        [Description("Should Omega countdown be reset on stop?")]
        public bool ResetOmegaOnWarheadStop { get; set; } = false;
        #endregion

        #region Timing Settings
        /// <summary>
        /// Gets or sets the time (in seconds) until Omega Warhead detonation.
        /// </summary>
        [Description("Time until Omega detonation (in seconds).")]
        public float TimeToDetonation { get; set; } = 345f;

        /// <summary>
        /// Notification times (in seconds) at which Cassie announcements are triggered.
        /// Must be sorted in descending order.
        /// </summary>
        [Description("Notification times (in seconds), descending order (e.g. 300,240,...,1).")]
        public List<int> NotifyTimes { get; set; } = new List<int>()
        {
            300, 240, 180, 120, 60, 25, 15, 10, 5, 4, 3, 2, 1
        };

        /// <summary>
        /// Multiplier used to calculate how long Cassie countdown notifications play.
        /// </summary>
        [Description("Multiplier for calculating estimated Cassie duration during countdown notifications (does not control actual speech speed).")]
        public double CassieNotifySpeed { get; set; } = 0.75;

        /// <summary>
        /// Estimation multiplier for the final Omega Warhead detonation Cassie message.
        /// </summary>
        [Description("Multiplier for estimating Cassie duration of final detonation message (does not affect actual voice speed).")]
        public double CassieDetonationSpeed { get; set; } = 0.35;

        /// <summary>
        /// Extra buffer time (in seconds) added to each Cassie announcement to prevent overlapping.
        /// </summary>
        [Description("Buffer time (in seconds) added to each Cassie message to avoid skips during countdown.")]
        public float CassieTimingBuffer { get; set; } = 0.65f;

        /// <summary>
        /// Gets or sets the delay (in seconds) before checkpoint doors open and lock.
        /// </summary>
        [Description("Delay before checkpoint doors open and lock (in seconds).")]
        public float OpenAndLockCheckpointDoorsDelay { get; set; } = 225f;

        /// <summary>
        /// Gets or sets the delay (in seconds) before the helicopter broadcast begins.
        /// </summary>
        [Description("Delay before the helicopter broadcast begins (in seconds).")]
        public float HelicopterBroadcastDelay { get; set; } = 250f;

        /// <summary>
        /// Gets or sets the delay (in seconds) before the Omega sequence starts.
        /// </summary>
        [Description("Delay before Omega sequence starts (in seconds).")]
        public float DelayBeforeOmegaSequence { get; set; } = 0.15f;
        #endregion

        #region Zones Settings
        /// <summary>
        /// Gets or sets the size of the escape zone.
        /// </summary>
        [Description("Size of the escape zone.")]
        public float EscapeZoneSize { get; set; } = 7.75f;

        /// <summary>
        /// Gets or sets the size of the breach shelter zone.
        /// </summary>
        [Description("Size of the breach shelter zone.")]
        public float ShelterZoneSize { get; set; } = 7.75f;
        #endregion

        #region Visuals Settings
        /// <summary>
        /// Gets or sets the red channel of Omega room lighting (0.0 - 1.0).
        /// </summary>
        [Description("Red channel of Omega room lighting (0.0 - 1.0).")]
        public float LightsColorR { get; set; } = 0.05f;

        /// <summary>
        /// Gets or sets the green channel of Omega room lighting (0.0 - 1.0).
        /// </summary>
        [Description("Green channel of Omega room lighting (0.0 - 1.0).")]
        public float LightsColorG { get; set; } = 0.85f;

        /// <summary>
        /// Gets or sets the blue channel of Omega room lighting (0.0 - 1.0).
        /// </summary>
        [Description("Blue channel of Omega room lighting (0.0 - 1.0).")]
        public float LightsColorB { get; set; } = 0.35f;
        #endregion

        #region UI and Audio Messages
        [Description("Priority for Cassie messages.")]
        public float CassieMessagePriority { get; set; } = 7.51f;

        [Description("Priority for important Cassie messages.")]
        public float CassieMessageImportantPriority { get; set; } = 10.1f;

        [Description("Hint message displayed when the evacuation helicopter is inbound to the landing zone.")]
        public string HelicopterIncomingMessage { get; set; } = "Incoming evacuation helicopter!";

        [Description("Hint message shown when helicopter escape is successful.")]
        public string HelicopterEscapeMessage { get; set; } = "You escaped in the helicopter.";

        [Description("Message shown to players who successfully survive Omega Warhead detonation.")]
        public string SurvivorMessage { get; set; } = "The facility may be gone, but hope lives on through survivors like you. A new dawn awaits.\n<b><color=#00FA9A>Your story of survival will inspire generations.</color></b>";

        [Description("Message shown to players who successfully evacuate during Omega Warhead detonation.")]
        public string EvacuatedMessage { get; set; } = "Your escape was swift, but the scars will linger.\n<b><color=#00FA9A>You didn’t just run — you made it through hell to tell the tale.</color></b>";

        [Description("Message shown to players who die during Omega Warhead detonation.")]
        public string KilledMessage { get; set; } = "You became part of the blast that erased a legacy.\n<b><color=#6C1133>The facility consumed you... but whispers of your courage echo in the ruins.</color></b>";

        [Description("Global ending broadcast showing survival statistics with post-nuclear hope theme.")]
        public string EndingBroadcast { get; set; } = "<size=26><color=#FFA500>--- OMEGA WARHEAD AFTERMATH ---</color></size>\n<b>CASUALTY REPORT</b>\n" +
            "\n<color=green> 🌿 Survived: {survived}</color> souls preserved in shelters." +
            "\n<color=blue> 🚁 Evacuated: {escaped}</color> heroes airlifted to safety." +
            "\n<color=red> 💀 Lost: {dead}</color> absorbed by the nuclear blast.\n" +
            "\n<b><color=#DAA520>FINAL TRANSMISSION FROM SITE-██</color></b>" +
            "\nDespite catastrophic losses, <color=#FFD700><b>humanity endures</b></color>." +
            "\nThe Foundation's work continues at <i>Secondary Locations</i>." +
            "\n<size=20>Those who remain will rebuild...</size>\n" +
            "\n<size=18><color=#A9A9A9>// Automated System Log #{code} from SITE-██</color></size>;";

        [Description("Hint message when Omega Warhead is activated.")]
        public string ActivatedMessage { get; set; } = "<b><color=#ff0040>OMEGA WARHEAD ACTIVATED</color></b>\nPLEASE EVACUATE IMMEDIATELY";

        [Description("Cassie message when Omega is stopped.")]
        public string StoppingOmegaCassie { get; set; } = "pitch_0.9 Omega Warhead detonation has been stopped";

        [Description("Cassie message when Omega is activated.")]
        public string StartingOmegaCassie { get; set; } = "pitch_0.2 .g3 .g3 .g3 pitch_0.9 attention . attention . activating omega warhead . Please evacuate in the . breach shelter or in the helicopter . please evacuate immediately .";

        [Description("Cassie message during detonation.")]
        public string DetonatingOmegaCassie { get; set; } = "pitch_0.65 Detonating OMEGA pitch_0.5 Warhead";

        [Description("Cassie message announcing incoming helicopter.")]
        public string HeliIncomingCassie { get; set; } = "pitch_0.25 .g3 .g3 .g3 pitch_0.9 attention . attention . the helicopter is in coming . Please evacuate . Attention . the helicopter is in coming . Please evacuate immediately";

        [Description("Cassie message announcing checkpoint unlock.")]
        public string CheckpointUnlockCassie { get; set; } = "pitch_0.25 .g3 .g3 .g3 pitch_0.9 attention . attention . the checkpoint doors are open . Attention . the checkpoint doors are open . Please evacuate immediately";

        [Description("Message when Omega is stopped.")]
        public string StoppingOmegaMessage { get; set; } = "Omega Warhead detonation has been successfully aborted";

        [Description("Message when Omega is activated.")]
        public string StartingOmegaMessage { get; set; } = "Attention: Omega Warhead activation sequence initiated. All personnel must evacuate through secure shelters or extraction zones";

        [Description("Message during detonation.")]
        public string DetonatingOmegaMessage { get; set; } = "Omega Warhead detonation in progress. Evacuation is no longer possible";

        [Description("Message announcing incoming helicopter.")]
        public string HeliIncomingMessage { get; set; } = "Alert: Extraction helicopter approaching the facility. Proceed to the surface zone immediately";

        [Description("Message announcing checkpoint unlock.")]
        public string CheckpointUnlockMessage { get; set; } = "Facility update: Checkpoint doors are now accessible. Proceed with caution";

        [Description("Disable Cassie string messages during message broadcasts?")]
        public bool DisableCassieMessages { get; set; } = true;

        [Description("Clear Cassie queue before important messages?")]
        public bool CassieMessageClearBeforeImportant { get; set; } = true;

        [Description("Clear Cassie queue before warhead messages?")]
        public bool CassieMessageClearBeforeWarheadMessage { get; set; } = false;
        #endregion

        #region Dead Man Switch Settings
        
        [Description("Disables Dead Man Sequence if set to true.")]
        public bool DisableDeadManSwitch { get; set; } = false;

        [Description("Trigger a mini wave when the Dead Man Switch is canceled?")]
        public bool TriggerMiniWaveOnDmsCancel { get; set; } = true;

        [Description("Cassie message played when the Dead Man Switch is disabled.")]
        public string CassieMessageMiniWaveOnDmsCancel { get; set; } = "Deadman switch disabled. Substantial threat to safety remains within the facility. Exercise caution";

        [Description("Open blast doors when the Dead Man Switch is canceled?")]
        public bool OpenBlastDoorsOnDmsCancel { get; set; } = true;

        [Description("Number of respawn tokens added when the Dead Man Switch is canceled.")]
        public int TokensAddedOnDmsCancel { get; set; } = 1;

        [Description("Influence boost applied to team when the Dead Man Switch is canceled.")]
        public float InfluenceBoostOnDmsCancel { get; set; } = 12.69f;
        #endregion

        #region Permissions & Debug Settings
        [Description("Permission string required to use Omega Warhead commands.")]
        public string Permissions { get; set; } = "omegawarhead";

        [Description("Enable debug logging.")]
        public bool Debug { get; set; } = false;
        #endregion

        #region High-Performance Validation Engine
        /// <summary>
        /// Validates, normalizes, and structurally clamps all configuration variables to guarantee deterministic execution.
        /// </summary>
        public void Validate()
        {
            if (!IsEnabled)
            {
                LogHelper.Warning("[OmegaWarhead Config] Core Engine is disabled. The plugin assembly will remain passive.");
                return;
            }

            // 1. Core Randomization and Limits Clamping
            ReplaceAlphaChance = Mathf.Clamp(ReplaceAlphaChance, 0, 100);
            GeneratorsNumGuaranteeOmega = Mathf.Clamp(GeneratorsNumGuaranteeOmega, 0, 5);
            GeneratorsIncreaseChanceBy = Mathf.Max(0f, GeneratorsIncreaseChanceBy);
            TokensAddedOnDmsCancel = Mathf.Max(0, TokensAddedOnDmsCancel);

            // 2. Main Timeline Execution Baselines
            if (TimeToDetonation < 10f)
            {
                LogHelper.Warning($"[OmegaWarhead Config] Critical TimeToDetonation ({TimeToDetonation}s) is too low for extraction scenarios. Normalizing to safe factory limit (10s).");
                TimeToDetonation = 10f;
            }

            // 3. Relational Structural Delays Guards (Prevents event execution post-blast)
            OpenAndLockCheckpointDoorsDelay = Mathf.Clamp(OpenAndLockCheckpointDoorsDelay, 0f, TimeToDetonation - 1f);
            HelicopterBroadcastDelay = Mathf.Clamp(HelicopterBroadcastDelay, 0f, TimeToDetonation - 1f);
            DelayBeforeOmegaSequence = Mathf.Clamp(DelayBeforeOmegaSequence, 0f, TimeToDetonation - 1f);

            // 4. Spatial Zone Grid Multipliers
            EscapeZoneSize = Mathf.Max(0.1f, EscapeZoneSize);
            ShelterZoneSize = Mathf.Max(0.1f, ShelterZoneSize);

            // 5. Native Cassie Speech-Speed Estimation Thresholds
            CassieNotifySpeed = Math.Min(1.5, Math.Max(0.3, CassieNotifySpeed));
            CassieDetonationSpeed = Math.Min(1.5, Math.Max(0.3, CassieDetonationSpeed));

            // 6. Unified Timing Audio Buffer Pipeline
            if (CassieTimingBuffer < 0f || CassieTimingBuffer > 5f)
            {
                LogHelper.Warning($"[OmegaWarhead Config] Extremal CassieTimingBuffer value detected ({CassieTimingBuffer}s). Reverting immediately to safe production default (0.65s).");
                CassieTimingBuffer = 0.65f;
            }

            // 7. Descending Sequence Order Verification for Countdown List
            if (NotifyTimes != null && NotifyTimes.Count > 1)
            {
                for (int i = 0; i < NotifyTimes.Count - 1; i++)
                {
                    if (NotifyTimes[i] < NotifyTimes[i + 1])
                    {
                        LogHelper.Warning("[OmegaWarhead Config] NotifyTimes list constraint violated (Not sorted in descending timeline order). Executing optimal array restructuring loop...");
                        NotifyTimes.Sort((a, b) => b.CompareTo(a));
                        break;
                    }
                }
            }

            // 8. Native Color Spectrum Pipeline Safeguards
            LightsColorR = Mathf.Clamp(LightsColorR, 0f, 1f);
            LightsColorG = Mathf.Clamp(LightsColorG, 0f, 1f);
            LightsColorB = Mathf.Clamp(LightsColorB, 0f, 1f);

            // 9. UI Localization Assets Security Validation Loop
            ValidateString(CassieMessageMiniWaveOnDmsCancel, nameof(CassieMessageMiniWaveOnDmsCancel));
            ValidateString(HelicopterIncomingMessage, nameof(HelicopterIncomingMessage));
            ValidateString(HelicopterEscapeMessage, nameof(HelicopterEscapeMessage));
            ValidateString(ActivatedMessage, nameof(ActivatedMessage));
            ValidateString(StoppingOmegaCassie, nameof(StoppingOmegaCassie));
            ValidateString(StartingOmegaCassie, nameof(StartingOmegaCassie));
            ValidateString(DetonatingOmegaCassie, nameof(DetonatingOmegaCassie));
            ValidateString(HeliIncomingCassie, nameof(HeliIncomingCassie));
            ValidateString(CheckpointUnlockCassie, nameof(CheckpointUnlockCassie));
            ValidateString(StoppingOmegaMessage, nameof(StoppingOmegaMessage));
            ValidateString(StartingOmegaMessage, nameof(StartingOmegaMessage));
            ValidateString(DetonatingOmegaMessage, nameof(DetonatingOmegaMessage));
            ValidateString(HeliIncomingMessage, nameof(HeliIncomingMessage));
            ValidateString(CheckpointUnlockMessage, nameof(CheckpointUnlockMessage));
            ValidateString(SurvivorMessage, nameof(SurvivorMessage));
            ValidateString(EvacuatedMessage, nameof(EvacuatedMessage));
            ValidateString(KilledMessage, nameof(KilledMessage));
            ValidateString(EndingBroadcast, nameof(EndingBroadcast));
            ValidateString(Permissions, nameof(Permissions));
        }

        /// <summary>
        /// White-space resilient configuration string tracking filter.
        /// </summary>
        private void ValidateString(string value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                LogHelper.Warning($"[OmegaWarhead Config Asset Disruption] Value mapping for property '{propertyName}' evaluates to null or blank space coordinates.");
            }
        }
        #endregion
    }
}