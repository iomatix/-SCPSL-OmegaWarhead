using LabApi.Extensions;
using LabApi.Loader.Features.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Logger = LabApi.Extensions.Misc.iLogger;

namespace OmegaWarhead
{
    /// <summary>
    /// Configuration settings for OmegaWarhead inside the LabAPI execution environment.
    /// </summary>
    public sealed class Config : LabApiConfig
    {
        #region Core Settings
        [Description("Plugin enabled?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Chance that the Alpha Warhead will be replaced with Omega Warhead.")]
        public int ReplaceAlphaChance { get; set; } = 15;

        [Description("Number of engaged generators that guarantees Omega Warhead launch.")]
        public int GeneratorsNumGuaranteeOmega { get; set; } = 3;

        [Description("How much the chance is increased per each activated generator.")]
        public float GeneratorsIncreaseChanceBy { get; set; } = 15;

        [Description("Whether players can stop the Omega Warhead.")]
        public bool IsStopAllowed { get; set; } = false;

        [Description("Should Omega countdown be reset on stop?")]
        public bool ResetOmegaOnWarheadStop { get; set; } = false;
        #endregion

        #region Timing Settings (T-Minus Rework)
        [Description("Total time until Omega detonation (in seconds) when initiated by default game sequences.")]
        public float TimeToDetonation { get; set; } = 345f;

        [Description("Notification times (in seconds), descending order (e.g. 300,240,...,1).")]
        public List<int> NotifyTimes { get; set; } = new()
        {
            300, 240, 180, 120, 60, 25, 15, 10, 5, 4, 3, 2, 1
        };

        [Description("Multiplier for calculating estimated Cassie duration during countdown notifications.")]
        public double CassieNotifySpeed { get; set; } = 0.75;

        [Description("Multiplier for estimating Cassie duration of final detonation message.")]
        public double CassieDetonationSpeed { get; set; } = 0.35;

        [Description("Buffer time (in seconds) added to each Cassie message to avoid skips during countdown.")]
        public float CassieTimingBuffer { get; set; } = 0.55f;

        [Description("T-Minus threshold (seconds REMAINING until detonation) when checkpoint doors open and permanently lock.")]
        public float OpenAndLockCheckpointDoorsTMinus { get; set; } = 120f;

        [Description("T-Minus threshold (seconds REMAINING until detonation) when the evacuation helicopter broadcast begins and arrival logic ticks.")]
        public float HelicopterBroadcastTMinus { get; set; } = 95f;

        [Description("Delay buffer (in seconds) executed from button press before the Omega sequence begins its cycle.")]
        public float DelayBeforeOmegaSequence { get; set; } = 0.15f;
        #endregion

        #region Zones Settings
        [Description("Size of the escape zone.")]
        public float EscapeZoneSize { get; set; } = 7.75f;

        [Description("Size of the breach shelter zone.")]
        public float ShelterZoneSize { get; set; } = 7.75f;

        [Description("The exact world coordinates for the helicopter escape zone trigger.")]
        public Vector3 HelicopterEscapeZone { get; set; } = new Vector3(127f, 295.5f, -43f);

        [Description("The target position where players are teleported upon successful helicopter evacuation.")]
        public Vector3 HelicopterTeleportPosition { get; set; } = new Vector3(39f, 1015f, 32f);
        #endregion

        #region Visuals Settings
        [Description("Red channel of Omega room lighting (0.0 - 1.0).")]
        public float LightsColorR { get; set; } = 0.05f;

        [Description("Green channel of Omega room lighting (0.0 - 1.0).")]
        public float LightsColorG { get; set; } = 0.85f;

        [Description("Blue channel of Omega room lighting (0.0 - 1.0).")]
        public float LightsColorB { get; set; } = 0.35f;
        #endregion

        #region UI and Audio Messages
        [Description("Priority for Cassie messages.")]
        public float CassieMessagePriority { get; set; } = 1.0f;

        [Description("Priority for important Cassie messages.")]
        public float CassieMessageImportantPriority { get; set; } = 10.1f;

        [Description("Hint message displayed when the evacuation helicopter is inbound to the landing zone.")]
        public string HelicopterIncomingMessage { get; set; } = "Evacuation helicopter is incoming!";

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
        public string StoppingOmegaCassie { get; set; } = "$pitch_0.95 Omega Warhead  $pitch_0.91 detonation $pitch_0.98 has been stopped";

        [Description("Cassie message when Omega is activated.")]
        public string StartingOmegaCassie { get; set; } = "$pitch_0.2 .g3 .g3 .g3 $pitch_0.95 attention . $pitch_0.93 attention . $pitch_0.97 activating $pitch_0.92 omega $pitch_0.95 warhead . $pitch_0.98 Please evacuate in the . breach shelter or in the helicopter . please evacuate $pitch_0.93 immediately .";

        [Description("Cassie message during detonation.")]
        public string DetonatingOmegaCassie { get; set; } = "$pitch_0.8 Detonating $pitch_0.65 OMEGA $pitch_0.5 Warhead";

        [Description("Cassie message announcing incoming helicopter.")]
        public string HeliIncomingCassie { get; set; } = "$pitch_0.25 .g3 .g3 .g3 $pitch_0.95 attention . $pitch_0.93 attention . $pitch_0.97 the $pitch_0.9 helicopter $pitch_0.95 is in coming . $pitch_0.95 Please evacuate . $pitch_0.9 Attention . $pitch_0.95 the helicopter is in coming . Please evacuate $pitch_0.85 immediately";

        [Description("Cassie message announcing checkpoint unlock.")]
        public string CheckpointUnlockCassie { get; set; } = "$pitch_0.25 .g3 .g3 .g3 $pitch_0.95 attention . $pitch_0.91 attention . $pitch_0.95 the $pitch_0.9 checkpoint doors $pitch_0.95 are open . $pitch_0.9 Attention . $pitch_0.95 the checkpoint doors are open . Please evacuate $pitch_0.9 immediately";

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
                Logger.Warn(nameof(Config), "Core Detonation Engine is disabled. The plugin assembly will remain passive.");
                return;
            }

            // 1. Core Randomization and Limits Clamping via Fluent API Primitives
            ReplaceAlphaChance = ReplaceAlphaChance.Clamp(0, 100);
            GeneratorsNumGuaranteeOmega = GeneratorsNumGuaranteeOmega.Clamp(0, 5);
            GeneratorsIncreaseChanceBy = GeneratorsIncreaseChanceBy.LimitMin(0f);
            TokensAddedOnDmsCancel = TokensAddedOnDmsCancel.LimitMin(0);

            // 2. Main Timeline Execution Baselines
            if (TimeToDetonation < 10f)
            {
                Logger.Warn(nameof(Config), $"Critical TimeToDetonation ({TimeToDetonation}s) evaluates below extraction minimum curves. Normalizing onto baseline limits (10s).");
                TimeToDetonation = 10f;
            }

            // 3. Relational T-Minus Structural Guards
            OpenAndLockCheckpointDoorsTMinus = OpenAndLockCheckpointDoorsTMinus.Clamp(1f, TimeToDetonation);
            HelicopterBroadcastTMinus = HelicopterBroadcastTMinus.Clamp(1f, TimeToDetonation);
            DelayBeforeOmegaSequence = DelayBeforeOmegaSequence.LimitMin(0f);

            // 4. Spatial Zone Grid Multipliers
            EscapeZoneSize = EscapeZoneSize.LimitMin(0.1f);
            ShelterZoneSize = ShelterZoneSize.LimitMin(0.1f);

            // 5. Native Cassie Speech-Speed Estimation Thresholds
            CassieNotifySpeed = CassieNotifySpeed.Clamp(0.3, 1.5);
            CassieDetonationSpeed = CassieDetonationSpeed.Clamp(0.3, 1.5);

            // 6. Unified Timing Audio Buffer Pipeline Checks
            if (CassieTimingBuffer < 0f || CassieTimingBuffer > 5f)
            {
                Logger.Warn(nameof(Config), $"Extremal CassieTimingBuffer variance detected ({CassieTimingBuffer}s). Reverting immediately to safe production fallback metrics (0.65s).");
                CassieTimingBuffer = 0.65f;
            }

            // 7. Optimized Chronological Array Order Verification via LINQ Declarative Pipes
            if (NotifyTimes is { Count: > 1 } && NotifyTimes.Zip(NotifyTimes.Skip(1), (current, next) => current < next).Any(isInvalid => isInvalid))
            {
                Logger.Warn(nameof(Config), "NotifyTimes chronological array matrix alignment constraint violated. Re-sorting array descending...");
                NotifyTimes.Sort((a, b) => b.CompareTo(a));
            }

            // 8. Native Color Spectrum Pipeline Safeguards
            LightsColorR = LightsColorR.Clamp(0f, 1f);
            LightsColorG = LightsColorG.Clamp(0f, 1f);
            LightsColorB = LightsColorB.Clamp(0f, 1f);

            // 9. Reflection-Driven Automated UI and Localization Assets Security Validation Loop (DRY Compliant)
            var stringProperties = typeof(Config)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(string));

            foreach (var property in stringProperties)
            {
                if (string.IsNullOrWhiteSpace((string)property.GetValue(this)))
                {
                    Logger.Warn(nameof(Config), $"[Asset Disruption] Value mapping for property asset string metadata '{property.Name}' evaluates to null or empty coordinates.");
                }
            }
        }
        #endregion
    }
}