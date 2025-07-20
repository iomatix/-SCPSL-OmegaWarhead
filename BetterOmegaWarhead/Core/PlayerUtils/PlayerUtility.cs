namespace BetterOmegaWarhead.Core.PlayerUtils
{
    using LabApi.Features.Wrappers;
    using MapGeneration;
    using MEC;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Static helper methods related to Player checks and conditions.
    /// </summary>
    public static class PlayerUtility
    {
        /// <summary>
        /// Checks if the player is eligible for escape scenario.
        /// </summary>
        public static bool IsEligibleForEscape(Player player, Vector3 escapeZone, float escapeZoneSize)
        {
            if (player.IsSCP || !player.IsAlive)
                return false;

            float distance = Vector3.Distance(player.Position, escapeZone);
            return distance <= escapeZoneSize;
        }

        /// <summary>
        /// Determines if a player is inside a shelter.
        /// </summary>
        public static bool IsInShelter(Player player, float shelterZoneSize, params RoomName[] roomNames)
        {
            // Default to EzEvacShelter if no rooms specified  
            if (roomNames == null || roomNames.Length == 0)
            {
                roomNames = new[] { RoomName.EzEvacShelter };
            }

            // Check if player is in any of the specified shelter rooms  
            if (player.Room != null && roomNames.Contains(player.Room.Name))
                return true;

            // Check distance-based shelters  
            HashSet<Vector3> shelters = Plugin.Singleton.CacheHandler.GetCachedShelterLocations();
            foreach (Vector3 shelterPos in shelters)
            {
                if (Vector3.Distance(player.Position, shelterPos) <= shelterZoneSize)
                    return true;
            }

            return false;
        }

    }
}
