namespace BetterOmegaWarhead.Core.DoorUtils
{
    using LabApi.Features.Enums;
    using LabApi.Features.Wrappers;
    using MEC;
    using System.Collections.Generic;

    /// <summary>
    /// Static helper methods related to Door logic.
    /// </summary>
    public class DoorUtility
    {
        /// <summary>
        /// Checks if a door is open.
        /// </summary>
        public static bool IsDoorOpen(Door door)
        {
            return door != null && door.IsOpened;
        }

        /// <summary>
        /// Toggles door state.
        /// </summary>
        public static void ToggleDoor(Door door)
        {
            if (door == null)
                return;

            if (door.IsOpened)
                door.IsOpened = false;
            else
                door.IsOpened = true;
        }
    }
}
