namespace OmegaWarhead.Core.PlayerUtils
{
    using LabApi.Features.Wrappers;
    using MapGeneration;
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
        public static bool IsEligibleForEscape(Player player, Vector3 escapeZone, float escapeZoneSize)
        {
            if (player == null || player.IsSCP || !player.IsAlive)
                return false;

            float sqrDistance = (player.Position - escapeZone).sqrMagnitude;
            return sqrDistance <= (escapeZoneSize * escapeZoneSize);
        }
        #endregion

        #region Shelter Detection Methods
        /// <summary>
        /// Determines whether the player is inside a valid shelter area.
        /// </summary>
        public static bool IsInShelter(Player player, float shelterZoneSize, params RoomName[] roomNames)
        {
            if (player == null) return false;

            #region Room Name Validation
            // Fast-path: no params provided, just check the default EzEvacShelter
            if (roomNames == null || roomNames.Length == 0)
            {
                if (player.Room != null && player.Room.Name == RoomName.EzEvacShelter)
                    return true;
            }
            else
            {
                // Slow-path: custom rooms provided
                if (player.Room != null && roomNames.Contains(player.Room.Name))
                    return true;
            }
            #endregion

            #region Distance-Based Shelter Check
            HashSet<Vector3> shelters = Plugin.Singleton.CacheHandler.GetCachedShelterLocations();
            float sqrZoneSize = shelterZoneSize * shelterZoneSize;

            foreach (Vector3 shelterPos in shelters)
            {
                if ((player.Position - shelterPos).sqrMagnitude <= sqrZoneSize)
                    return true;
            }
            #endregion

            return false;
        }
        #endregion
    }
    #endregion
}