namespace OmegaWarhead.Shared
{
    public static class CoroutineTags
    {
        // Core tags for static processes (not directly tied to a player)
        public const string Core = "Omega-Core";
        public const string Detonation = "Omega-Detonation";
        public const string Countdown = "Omega-Countdown";
        public const string Helicopter = "Omega-Heli";
        public const string Checkpoints = "Omega-Checkpoints";
        public const string HeliEvacuation = "Omega-HeliEvacuation";
        public const string Escape = "Omega-Escape";
        public const string Scenario = "Omega-Scenario";

        // Prefixes for dynamic processes

        /// <summary>
        /// This is used for player-specific processes. The actual tag will be "OmegaSavePlayer-{PlayerID}" to ensure uniqueness per player.
        /// </summary>
        public const string SavePlayerPrefix = "Omega-SavePlayer-";

        /// <summary>
        /// Tag for generic temporary processes that don't fit into the other categories. This can be used for one-off tasks or processes that are created dynamically and don't need a specific prefix e.g. MEC CallDelayed coroutines.
        /// </summary>
        public const string Temp = "Omega-Temp";

        /// <summary>
        /// This array is useful for cleaning up all static processes at once, such as when the warhead is defused or the round ends.
        /// </summary>
        public static readonly string[] AllStaticTags =
        {
           Core, Detonation, Countdown, Helicopter, Checkpoints, HeliEvacuation, Escape, Scenario, Temp
        };
    }
}