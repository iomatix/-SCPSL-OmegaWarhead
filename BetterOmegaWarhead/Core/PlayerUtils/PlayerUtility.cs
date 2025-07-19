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
    public class PlayerUtility
    {
        private readonly Plugin _plugin;

        public PlayerUtility(Plugin plugin)
        {
            _plugin = plugin;
        }

        /// <summary>
        /// Checks if the player is eligible for helicopter escape.
        /// </summary>
        public bool IsEligibleForEscape(Player player, Vector3 helicopterZone)
        {
            if (player.IsSCP || !player.IsAlive)
                return false;

            float distance = Vector3.Distance(player.Position, helicopterZone);
            return distance <= _plugin.Config.HelicopterZoneSize;
        }

        /// <summary>
        /// Determines if a player is inside a shelter.
        /// </summary>
        public bool IsInShelter(Player player)
        {
            if (player.Room != null && player.Room.Name == RoomName.EzEvacShelter)
                return true;

            var shelters = _plugin.CacheHandler.GetCachedShelterLocations();
            foreach (Vector3 shelterPos in shelters)
            {
                if (Vector3.Distance(player.Position, shelterPos) <= _plugin.Config.ShelterZoneSize)
                    return true;
            }

            return false;
        }

    }
}
