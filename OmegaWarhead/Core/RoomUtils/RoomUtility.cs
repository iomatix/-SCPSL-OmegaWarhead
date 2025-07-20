namespace BetterOmegaWarhead.Core.RoomUtils
{
    using LabApi.Features.Wrappers;
    using MapGeneration;
    using UnityEngine;

    /// <summary>
    /// Static helper methods related to Room logic.
    /// </summary>
    #region RoomUtility Class
    public static class RoomUtility
    {
        #region Room Checks
        /// <summary>
        /// Checks if player is inside any room of a specific type.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <param name="roomName">The room type to compare.</param>
        /// <returns>True if the player is in the specified room type.</returns>
        public static bool IsPlayerInRoomType(Player player, RoomName roomName)
        {
            return player.Room != null && player.Room.Name == roomName;
        }
        #endregion

        #region Distance Calculations
        /// <summary>
        /// Returns distance between player and room center.
        /// </summary>
        /// <param name="player">The player whose distance is being calculated.</param>
        /// <param name="room">The room to measure distance to.</param>
        /// <returns>The distance in units; float.MaxValue if room is null.</returns>
        public static float DistanceToRoom(Player player, Room room)
        {
            if (room == null)
                return float.MaxValue;

            return Vector3.Distance(player.Position, room.Position);
        }
        #endregion
    }
    #endregion
}