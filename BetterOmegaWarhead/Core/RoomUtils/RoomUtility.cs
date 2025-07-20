
namespace BetterOmegaWarhead.Core.RoomUtils
{
    using LabApi.Features.Wrappers;
    using MapGeneration;
    using UnityEngine;

    /// <summary>
    /// Static helper methods related to Room logic.
    /// </summary>
    public class RoomUtility
    {


        /// <summary>
        /// Checks if player is inside any room of a specific type.
        /// </summary>
        public static bool IsPlayerInRoomType(Player player, RoomName roomName)
        {
            return player.Room != null && player.Room.Name == roomName;
        }

        /// <summary>
        /// Returns distance between player and room center.
        /// </summary>
        public static float DistanceToRoom(Player player, Room room)
        {
            if (room == null)
                return float.MaxValue;

            return Vector3.Distance(player.Position, room.Position);
        }
    }
}
