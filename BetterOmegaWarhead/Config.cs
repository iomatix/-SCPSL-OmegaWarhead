namespace BetterOmegaWarhead
{
    using Exiled.API.Interfaces;
    using System.ComponentModel;

    public sealed class Config : IConfig
    {
        [Description("Plugin enabled?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Chance that the Alpha Warhead will be replaced with Omega Warhead")]
        public int ReplaceAlphaChance { get; set; } = 15;

        [Description("Indicates that the player can stop OmegaWarhead after activation.")]
        public bool isStopAllowed { get; set; } = false;

        [Description("Red channel of the lights color in the rooms during OmegaWarhead event")]
        public float LightsColorR { get; set; } = 0.05f;

        [Description("Green channel of the lights color in the rooms during OmegaWarhead event")]
        public float LightsColorG { get; set; } = 0.85f;

        [Description("Blue channel of the lights color in the rooms during OmegaWarhead event")]
        public float LightsColorB { get; set; } = 0.35f;

        [Description("Broadcast that will appear when the rescue helicopter is coming.")]
        public string HelicopterMessage { get; set; } = "<color=blue>RESCUE HELICOPTER COMING TO SURFACE IN:</color> ";

        [Description("Broadcast that will appear when the player escapes in the helicopter.")]
        public string HelicopterEscape { get; set; } = "You escaped in the helicopter.";

        [Description("Broadcast that will appear when the Omega Warhead is activated.")]
        public string ActivatedMessage { get; set; } = "<b><color=red>OMEGA WARHEAD ACTIVATED.</color></b>\nPLEASE EVACUATE IN BREACH SHELTER.";

        [Description("Cassie message when Omega Warhead is stopped")]
        public string StopCassie { get; set; } = "pitch_0.9 Omega Warhead detonation stopped";

        [Description("Cassie message of Omega Warhead (Not recommended to modify this)")]
        public string Cassie { get; set; } = "pitch_0.2 .g3 .g3 .g3 pitch_0.9 attention . attention . activating omega warhead . detonation in 3 minutes . please evacuate in the breach shelter or in the helicopter . please evacuate now . 170 seconds until destruction .g3 . .  .g3 . .  . .g3 . . .g3 . . .g3 . . . . .g3 . . . . .g3 . . . .g3 . . . .g3 . . . .g3 . . . .g3 . . . .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 120 seconds  .g3 . .  .g3 . .  .g3 . .  .g3 . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .  .g3 . . .   .g3 . . .   .g3 . .   .g3 . . .   .g3 . . .   .g3 . . .   .g3 . . .   .g3 . . .   .g3 . . .   .g3 . . .   .g3 . . .   .g3 . . .   .g3 . . .   .g3 . . .   1 minute .g3 .  . .g3 .  . .g3 .  . .g3 .  . .g3 .  . .g3 .  . .g3 .  . .g3 .  . .g3 .  . .g3 .  . .g3 .  . .g3 .  . .g3 .  . .g3 .  . 30 seconds . all checkpoint doors are open . please evacuate . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . pitch_0.7 0";

        
        
        [Description("Permissions of the plugin.")]
        public string Permissions { get; set; } = "omegawarhead";


        [Description("Debug enabled?")]
        public bool Debug { get; set; } = false;
    }
}