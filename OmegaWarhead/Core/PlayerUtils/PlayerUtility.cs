namespace BetterOmegaWarhead.Core.PlayerUtils
{
    using LabApi.Features.Wrappers;
    using MapGeneration;
    using MEC;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Provides static helper methods related to player checks and escape conditions.
    /// </summary>
    #region PlayerUtility Class
    public static class PlayerUtility
    {
        #region Escape Eligibility Methods
        /// <summary>
        /// Checks whether a player is eligible to trigger an escape scenario.
        /// </summary>
        /// <param name="player">The player to evaluate.</param>
        /// <param name="escapeZone">The world-space position of the escape zone.</param>
        /// <param name="escapeZoneSize">The maximum distance from the zone for a player to qualify.</param>
        /// <returns><c>true</c> if the player is alive, not an SCP, and within range of the escape zone; otherwise, <c>false</c>.</returns>
        public static bool IsEligibleForEscape(Player player, Vector3 escapeZone, float escapeZoneSize)
        {
            #region Eligibility Checks
            if (player.IsSCP || !player.IsAlive)
                return false;

            float distance = Vector3.Distance(player.Position, escapeZone);
            return distance <= escapeZoneSize;
            #endregion
        }
        #endregion

        #region Shelter Detection Methods
        /// <summary>
        /// Determines whether the player is inside a valid shelter area, either by room name or proximity to cached positions.
        /// </summary>
        /// <param name="player">The player to evaluate.</param>
        /// <param name="shelterZoneSize">The radius to check for nearby shelters.</param>
        /// <param name="roomNames">Optional list of <see cref="RoomName"/>s to consider as valid shelters. Defaults to <see cref="RoomName.EzEvacShelter"/> if empty.</param>
        /// <returns><c>true</c> if the player is in a shelter room or within range of a cached shelter position; otherwise, <c>false</c>.</returns>
        public static bool IsInShelter(Player player, float shelterZoneSize, params RoomName[] roomNames)
        {
            #region Room Name Validation
            // Default to EzEvacShelter if no rooms specified  
            if (roomNames == null || roomNames.Length == 0)
            {
                roomNames = new[] { RoomName.EzEvacShelter };
            }

            // Check if player is in any of the specified shelter rooms  
            if (player.Room != null && roomNames.Contains(player.Room.Name))
                return true;
            #endregion

            #region Distance-Based Shelter Check
            // Check distance-based shelters  
            HashSet<Vector3> shelters = Plugin.Singleton.CacheHandler.GetCachedShelterLocations();
            foreach (Vector3 shelterPos in shelters)
            {
                if (Vector3.Distance(player.Position, shelterPos) <= shelterZoneSize)
                    return true;
            }
            #endregion

            return false;
        }
        #endregion
    }
    #endregion
}