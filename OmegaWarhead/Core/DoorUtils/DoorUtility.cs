namespace OmegaWarhead.Core.DoorUtils
{
    using LabApi.Features.Enums;
    using LabApi.Features.Wrappers;
    using MEC;
    using System.Collections.Generic;

    /// <summary>
    /// Provides static utility methods for interacting with doors in the game world.
    /// </summary>
    #region DoorUtility Class
    public static class DoorUtility
    {
        #region Door State Methods
        /// <summary>
        /// Determines whether a given door is currently open.
        /// </summary>
        /// <param name="door">The door to check.</param>
        /// <returns><c>true</c> if the door is open; otherwise, <c>false</c>.</returns>
        public static bool IsDoorOpen(Door door)
        {
            return door != null && door.IsOpened;
        }

        /// <summary>
        /// Toggles the state of the specified door between open and closed.
        /// </summary>
        /// <param name="door">The door to toggle. If <c>null</c>, the method does nothing.</param>
        public static void ToggleDoor(Door door)
        {
            if (door == null)
                return;

            door.IsOpened = !door.IsOpened;
        }
        #endregion
    }
    #endregion
}