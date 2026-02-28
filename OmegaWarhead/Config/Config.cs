namespace OmegaWarhead
{
    using Exiled.API.Interfaces;
    using OmegaWarhead.Core.LoggingUtils;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Configuration settings for OmegaWarhead.
    /// </summary>
    #region Config Class
    public sealed class Config : IConfig
    {
        #region Core
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

        #region Timing
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
        /// Estimation multiplier used to calculate how long Cassie countdown notifications play.
        /// This does not change the actual Cassie voice speed. It helps synchronize message scheduling.
        /// Tune this value to match how Cassie behaves on your server.
        /// Recommended range: 0.45 to 1.25.
        /// </summary>
        [Description("Multiplier for calculating estimated Cassie duration during countdown notifications (does not control actual speech speed).")]
        public double CassieNotifySpeed { get; set; } = 0.75;

        /// <summary>
        /// Estimation multiplier for the final Omega Warhead detonation Cassie message.
        /// Used to calculate when the message finishes. Tune this to match Cassie's real delivery.
        /// Recommended range: 0.25 to 0.65.
        /// </summary>
        [Description("Multiplier for estimating Cassie duration of final detonation message (does not affect actual voice speed).")]
        public double CassieDetonationSpeed { get; set; } = 0.35;

        /// <summary>
        /// Extra buffer time (in seconds) added to each Cassie announcement to prevent overlapping or skipped messages.
        /// This ensures smoother playback—especially during fast countdown intervals (e.g. 5s, 4s, 3s...).
        /// Recommended value: 0.45 to 1.5 seconds.
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

        #region Zones
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

        #region Visuals
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

        #region Messages

        /// <summary>
        /// Priority level for Cassie’s messages. Higher values will skip more of the message queue before playing these announcements.
        /// </summary>
        [Description("Priority for Cassie messages.")]
        public float CassieMessagePriority { get; set; } = 7.51f;

        /// <summary>
        /// Priority level for Cassie’s important messages. Higher values will skip more of the message queue before playing these announcements.
        /// </summary>
        [Description("Priority for important Cassie messages.")]
        public float CassieMessageImportantPriority { get; set; } = 10.1f;

        /// <summary>
        /// Gets or sets the hint message displayed when the evacuation helicopter is inbound.
        /// </summary>
        [Description("Hint message displayed when the evacuation helicopter is inbound to the landing zone.")]
        public string HelicopterIncomingMessage { get; set; } = "Incoming evacuation helicopter!";

        /// <summary>
        /// Gets or sets the hint message shown when helicopter escape is successful.
        /// </summary>
        [Description("Hint message shown when helicopter escape is successful.")]
        public string HelicopterEscapeMessage { get; set; } = "You escaped in the helicopter.";

        /// <summary>  
        /// Gets or sets the message displayed to players who survive the Omega Warhead detonation.  
        /// </summary>  
        [Description("Message shown to players who successfully survive Omega Warhead detonation.")]
        public string SurvivorMessage { get; set; } = "The facility may be gone, but hope lives on through survivors like you. A new dawn awaits.\n<b><color=#00FA9A>Your story of survival will inspire generations.</color></b>";

        /// <summary>  
        /// Gets or sets the message displayed to players who evacuated the Omega Warhead detonation.  
        /// </summary>  
        [Description("Message shown to players who successfully evacuate during Omega Warhead detonation.")]
        public string EvacuatedMessage { get; set; } = "Your escape was swift, but the scars will linger.\n<b><color=#00FA9A>You didn’t just run — you made it through hell to tell the tale.</color></b>";


        /// <summary>  
        /// Gets or sets the message displayed to players who been killed by the Omega Warhead detonation.  
        /// </summary>  
        [Description("Message shown to players who die during Omega Warhead detonation.")]
        public string KilledMessage { get; set; } = "You became part of the blast that erased a legacy.\n<b><color=#6C1133>The facility consumed you... but whispers of your courage echo in the ruins.</color></b>";

        /// <summary>
        /// Global ending broadcast message displaying survival statistics after the nuclear event,
        /// wrapped in a post-apocalyptic hopeful theme.
        /// </summary>
        /// <remarks>
        /// This string supports these placeholders, which will be replaced at runtime:
        /// <list type="bullet">
        ///   <item><description><c>{survived}</c> — Number of souls who survived and sheltered safely.</description></item>
        ///   <item><description><c>{escaped}</c> — Number of heroes evacuated and airlifted to safety.</description></item>
        ///   <item><description><c>{dead}</c> — Number of casualties absorbed by the blast.</description></item>
        ///   <item><description><c>{code}</c> — Automated system log code identifier.</description></item>
        /// </list>
        /// The message includes stylized colors and sizes for emphasis, suited for in-game UI rendering.
        /// </remarks>
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



        /// <summary>
        /// Gets or sets the hint message displayed when Omega Warhead is activated.
        /// </summary>
        [Description("Hint message when Omega Warhead is activated.")]
        public string ActivatedMessage { get; set; } = "<b><color=#ff0040>OMEGA WARHEAD ACTIVATED</color></b>\nPLEASE EVACUATE IMMEDIATELY";

        /// <summary>
        /// Gets or sets the Cassie message when Omega Warhead is stopped.
        /// </summary>
        [Description("Cassie message when Omega is stopped.")]
        public string StoppingOmegaCassie { get; set; } = "pitch_0.9 Omega Warhead detonation has been stopped";

        /// <summary>
        /// Gets or sets the Cassie message when Omega Warhead is activated.
        /// </summary>
        [Description("Cassie message when Omega is activated.")]
        public string StartingOmegaCassie { get; set; } = "pitch_0.2 .g3 .g3 .g3 pitch_0.9 attention . attention . activating omega warhead . Please evacuate in the . breach shelter or in the helicopter . please evacuate immediately .";

        /// <summary>
        /// Gets or sets the Cassie message during Omega Warhead detonation.
        /// </summary>
        [Description("Cassie message during detonation.")]
        public string DetonatingOmegaCassie { get; set; } = "pitch_0.65 Detonating pitch_0.5 Warhead";

        /// <summary>
        /// Gets or sets the Cassie message announcing the incoming helicopter.
        /// </summary>
        [Description("Cassie message announcing incoming helicopter.")]
        public string HeliIncomingCassie { get; set; } = "pitch_0.25 .g3 .g3 .g3 pitch_0.9 attention . attention . the helicopter is in coming . Please evacuate . Attention . the helicopter is in coming . Please evacuate immediately";

        /// <summary>
        /// Gets or sets the Cassie message announcing checkpoint unlock.
        /// </summary>
        [Description("Cassie message announcing checkpoint unlock.")]
        public string CheckpointUnlockCassie { get; set; } = "pitch_0.25 .g3 .g3 .g3 pitch_0.9 attention . attention . the checkpoint doors are open . Attention . the checkpoint doors are open . Please evacuate immediately";

        /// <summary>
        /// Gets or sets the message when Omega Warhead is stopped.
        /// </summary>
        [Description("Message when Omega is stopped.")]
        public string StoppingOmegaMessage { get; set; } = "Omega Warhead detonation has been successfully aborted";

        /// <summary>
        /// Gets or sets the message when Omega Warhead is activated.
        /// </summary>
        [Description("Message when Omega is activated.")]
        public string StartingOmegaMessage { get; set; } = "Attention: Omega Warhead activation sequence initiated. All personnel must evacuate through secure shelters or extraction zones";

        /// <summary>
        /// Gets or sets the message during Omega Warhead detonation.
        /// </summary>
        [Description("Message during detonation.")]
        public string DetonatingOmegaMessage { get; set; } = "Omega Warhead detonation in progress. Evacuation is no longer possible";

        /// <summary>
        /// Gets or sets the message announcing the incoming helicopter.
        /// </summary>
        [Description("Message announcing incoming helicopter.")]
        public string HeliIncomingMessage { get; set; } = "Alert: Extraction helicopter approaching the facility. Proceed to the surface zone immediately";

        /// <summary>
        /// Gets or sets the message announcing checkpoint unlock.
        /// </summary>
        [Description("Message announcing checkpoint unlock.")]
        public string CheckpointUnlockMessage { get; set; } = "Facility update: Checkpoint doors are now accessible. Proceed with caution";

        /// <summary>
        /// Gets or sets a value indicating whether Cassie string messages (e.g., subtitles or on-screen text) should be disabled for important announcements.
        /// </summary>
        [Description("Disable Cassie string messages during message broadcasts?")]
        public bool DisableCassieMessages { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to clear the Cassie queue before important messages.
        /// </summary>
        [Description("Clear Cassie queue before important messages?")]
        public bool CassieMessageClearBeforeImportant { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to clear the Cassie queue before warhead messages.
        /// </summary>
        [Description("Clear Cassie queue before warhead messages?")]
        public bool CassieMessageClearBeforeWarheadMessage { get; set; } = false;
        #endregion

        #region Permissions & Debug
        /// <summary>
        /// Gets or sets the permission string required to use Omega Warhead commands.
        /// </summary>
        [Description("Permission string required to use Omega Warhead commands.")]
        public string Permissions { get; set; } = "omegawarhead";

        /// <summary>
        /// Gets or sets a value indicating whether debug logging is enabled.
        /// </summary>
        [Description("Enable debug logging.")]
        public bool Debug { get; set; } = false;
        #endregion

        #region Validation
        /// <summary>
        /// Validates configuration values and logs warnings if any are out of expected ranges or invalid.
        /// </summary>
        public void Validate()
        {
            // Core
            if (ReplaceAlphaChance < 0 || ReplaceAlphaChance > 100)
                LogHelper.Warning($"[Config] ReplaceAlphaChance should be between 0 and 100. Current: {ReplaceAlphaChance}");

            if (GeneratorsNumGuaranteeOmega < 0 || GeneratorsNumGuaranteeOmega > 5)
                LogHelper.Warning($"[Config] GeneratorsNumGuaranteeOmega should be between 0 and 5. Current: {GeneratorsNumGuaranteeOmega}");

            if (GeneratorsIncreaseChanceBy < 0)
                LogHelper.Warning($"[Config] GeneratorsIncreaseChanceBy shouldn't be negative. Current: {GeneratorsIncreaseChanceBy}");

            if (!IsEnabled)
                LogHelper.Warning("[Config] IsEnabled is set to false. Plugin will not function.");

            // Timing
            if (TimeToDetonation < 10)
                LogHelper.Warning($"[Config] TimeToDetonation should be at least 10 seconds to allow evacuation. Current: {TimeToDetonation}s");

            // Cassie Timing Adjustments
            if (CassieNotifySpeed < 0.3 || CassieNotifySpeed > 1.5)
                LogHelper.Warning($"[Config] CassieNotifySpeed ({CassieNotifySpeed}) is outside safe estimation range. Recommended: 0.65 to 1.25.");

            if (CassieDetonationSpeed < 0.3 || CassieDetonationSpeed > 1.5)
                LogHelper.Warning($"[Config] CassieDetonationSpeed ({CassieDetonationSpeed}) may lead to incorrect detonation timing. Recommended: 0.35 to 0.65.");

            // Unified Buffer Validation
            if (CassieTimingBuffer < 0f || CassieTimingBuffer > 5f)
            {
                LogHelper.Warning($"[Config] CassieTimingBuffer ({CassieTimingBuffer}s) is extreme. Hard limit is 0 to 5 seconds. Resetting to default (0.65s).");
                CassieTimingBuffer = 0.65f; // Automatyczna korekta wartości krytycznej
            }
            else if (CassieTimingBuffer < 0.45f || CassieTimingBuffer > 1.5f)
            {
                LogHelper.Warning($"[Config] CassieTimingBuffer ({CassieTimingBuffer}s) is valid but outside optimal range. Recommended: 0.45 to 1.5 seconds.");
            }

            // Notify Times Sorting Validation
            if (NotifyTimes != null && NotifyTimes.Count > 1)
            {
                bool isSorted = true;
                for (int i = 0; i < NotifyTimes.Count - 1; i++)
                {
                    if (NotifyTimes[i] < NotifyTimes[i + 1])
                    {
                        isSorted = false;
                        break;
                    }
                }

                if (!isSorted)
                {
                    LogHelper.Warning("[Config] NotifyTimes list is not sorted in descending order. Sorting automatically.");
                    NotifyTimes.Sort((a, b) => b.CompareTo(a)); // Make descending sort
                }
            }

            // Sequence Delays
            if (OpenAndLockCheckpointDoorsDelay < 0 || OpenAndLockCheckpointDoorsDelay >= TimeToDetonation)
                LogHelper.Warning($"[Config] OpenAndLockCheckpointDoorsDelay must be positive and less than TimeToDetonation. Current: {OpenAndLockCheckpointDoorsDelay}s");

            if (HelicopterBroadcastDelay < 0 || HelicopterBroadcastDelay >= TimeToDetonation)
                LogHelper.Warning($"[Config] HelicopterBroadcastDelay must be positive and less than TimeToDetonation. Current: {HelicopterBroadcastDelay}s");

            if (DelayBeforeOmegaSequence < 0 || DelayBeforeOmegaSequence >= TimeToDetonation)
                LogHelper.Warning($"[Config] DelayBeforeOmegaSequence must be positive and less than TimeToDetonation. Current: {DelayBeforeOmegaSequence}s");

            // Zones
            if (EscapeZoneSize <= 0)
                LogHelper.Warning($"[Config] EscapeZoneSize should be positive. Current: {EscapeZoneSize}");

            if (ShelterZoneSize <= 0)
                LogHelper.Warning($"[Config] ShelterZoneSize should be positive. Current: {ShelterZoneSize}");

            // Visuals
            if (LightsColorR < 0f || LightsColorR > 1f || LightsColorG < 0f || LightsColorG > 1f || LightsColorB < 0f || LightsColorB > 1f)
                LogHelper.Warning($"[Config] Light color RGB values should be between 0.0 and 1.0. Current: R={LightsColorR}, G={LightsColorG}, B={LightsColorB}");

            // Messages & Strings (DRY optimization)
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
        /// Helper method to validate string properties.
        /// </summary>
        private void ValidateString(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
                LogHelper.Warning($"[Config] {propertyName} is empty or null.");
        }
        #endregion
    }
    #endregion
}
