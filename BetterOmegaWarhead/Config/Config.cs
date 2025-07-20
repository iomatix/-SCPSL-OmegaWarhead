namespace BetterOmegaWarhead
{
    using System.ComponentModel;
    using BetterOmegaWarhead.Core.LoggingUtils;
    using Exiled.API.Interfaces;

    /// <summary>
    /// Configuration settings for BetterOmegaWarhead.
    /// </summary>
    public sealed class Config : IConfig
    {
        #region Core

        [Description("Plugin enabled?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Chance that the Alpha Warhead will be replaced with Omega Warhead.")]
        public int ReplaceAlphaChance { get; set; } = 15;

        [Description("Number of engaged generatorgenerators that guarantees Omega Warhead launch.")]
        public int GeneratorsNumGuaranteeOmega { get; set; } = 2;

        [Description("Whether players can stop the Omega Warhead.")]
        public bool IsStopAllowed { get; set; } = false;

        [Description("Should Omega countdown be reset on stop?")]
        public bool ResetOmegaOnWarheadStop { get; set; } = false;

        #endregion

        #region Timing

        [Description("Time until Omega detonation (in seconds).")]
        public float TimeToDetonation { get; set; } = 320f;

        [Description("Delay before checkpoint doors open and lock (in seconds).")]
        public float OpenAndLockCheckpointDoorsDelay { get; set; } = 225f;

        [Description("Delay before the helicopter broadcast begins (in seconds).")]
        public float HelicopterBroadcastDelay { get; set; } = 250f;

        [Description("Delay before Omega sequence starts (in seconds).")]
        public float DelayBeforeOmegaSequence { get; set; } = 5f;

        #endregion

        #region Zones

        [Description("Size of the escape zone.")]
        public float EscapeZoneSize { get; set; } = 7.75f;

        [Description("Size of the breach shelter zone.")]
        public float ShelterZoneSize { get; set; } = 7.75f;

        #endregion

        #region Visuals

        [Description("Red channel of Omega room lighting (0.0 - 1.0).")]
        public float LightsColorR { get; set; } = 0.05f;

        [Description("Green channel of Omega room lighting (0.0 - 1.0).")]
        public float LightsColorG { get; set; } = 0.85f;

        [Description("Blue channel of Omega room lighting (0.0 - 1.0).")]
        public float LightsColorB { get; set; } = 0.35f;

        #endregion

        #region Messages
        [Description("Hint message displayed when the evacuation helicopter is inbound to the landing zone.")]
        public string HelicopterIncomingMessage { get; set; } = "Incoming evacuation helicopter!";

        [Description("Hint message shown when helicopter escape is successful.")]
        public string HelicopterEscape { get; set; } = "You escaped in the helicopter.";

        [Description("Hint message when Omega Warhead is activated.")]
        public string ActivatedMessage { get; set; } = "<b><color=red>OMEGA WARHEAD ACTIVATED</color></b>\nPLEASE EVACUATE IMMEDIATELY";

        [Description("Cassie message when Omega is stopped.")]
        public string StoppingOmegaCassie { get; set; } = "pitch_0.9 Omega Warhead detonation has been stopped";

        [Description("Cassie message when Omega is activated.")]
        public string StartingOmegaCassie { get; set; } = "pitch_0.2 .g3 .g3 .g3 pitch_0.9 attention . attention . activating omega warhead . Please evacuate in the . breach shelter or in the helicopter . please evacuate immediately .";

        [Description("Cassie message during detonation.")]
        public string DetonatingOmegaCassie { get; set; } = "pitch_0.65 Detonating pitch_0.5 Warhead";

        [Description("Cassie message announcing incoming helicopter.")]
        public string HeliIncomingCassie { get; set; } = "pitch_0.25 .g3 .g3 .g3 pitch_0.9 attention . attention . the helicopter is in coming . Please evacuate . Attention . the helicopter is in coming . Please evacuate immediately";

        [Description("Cassie message announcing checkpoint unlock.")]
        public string CheckpointUnlockCassie { get; set; } = "pitch_0.25 .g3 .g3 .g3 pitch_0.9 attention . attention . the checkpoint doors are open . Attention . the checkpoint doors are open . Please evacuate immediately";

        [Description("Clear Cassie queue before important messages?")]
        public bool CassieMessageClearBeforeImportant { get; set; } = true;

        [Description("Clear Cassie queue before warhead messages?")]
        public bool CassieMessageClearBeforeWarheadMessage { get; set; } = true;

        #endregion

        #region Permissions & Debug

        [Description("Permission string required to use Omega Warhead commands.")]
        public string Permissions { get; set; } = "omegawarhead";

        [Description("Enable debug logging.")]
        public bool Debug { get; set; } = false;

        #endregion

        #region Validation

        /// <summary>
        /// Validates numeric config values and logs warnings if any are out of expected range.
        /// </summary>
        public void Validate()
        {
            if (ReplaceAlphaChance < 0 || ReplaceAlphaChance > 100)
                LogHelper.Warning($"[Config] ReplaceAlphaChance should be between 0 and 100. Current: {ReplaceAlphaChance}");

            if (GeneratorsNumGuaranteeOmega < 0)
                LogHelper.Warning($"[Config] GeneratorsNumGuaranteeOmega cannot be negative. Current: {GeneratorsNumGuaranteeOmega}");

            if (TimeToDetonation < 10)
                LogHelper.Warning($"[Config] TimeToDetonation is too low. Current: {TimeToDetonation}s");

            if (OpenAndLockCheckpointDoorsDelay >= TimeToDetonation)
                LogHelper.Warning($"[Config] OpenAndLockCheckpointDoorsDelay should be less than TimeToDetonation. Current: {OpenAndLockCheckpointDoorsDelay}s");

            if (HelicopterBroadcastDelay >= TimeToDetonation)
                LogHelper.Warning($"[Config] HelicopterBroadcastDelay should be less than TimeToDetonation. Current: {HelicopterBroadcastDelay}s");

            if (LightsColorR < 0f || LightsColorR > 1f || LightsColorG < 0f || LightsColorG > 1f || LightsColorB < 0f || LightsColorB > 1f)
                LogHelper.Warning($"[Config] Light color RGB values should be between 0.0 and 1.0. Current: R={LightsColorR}, G={LightsColorG}, B={LightsColorB}");
        }

        #endregion
    }
}
