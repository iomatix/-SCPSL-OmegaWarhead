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
        /// Represents different escape scenarios and their configurations.  
        /// </summary>  
        public class EscapeScenario
        {
            public string Name { get; set; }
            public Vector3 EscapeZone { get; set; }
            public Vector3 TeleportPosition { get; set; }
            public string InitialMessage { get; set; }
            public string EscapeMessage { get; set; }
            public float InitialDelay { get; set; }
            public float ProcessingDelay { get; set; }
            public float FinalDelay { get; set; }
            public Action OnEscapeTriggered { get; set; }
        }

        /// <summary>
        /// Checks if the player is eligible for escape scenario.
        /// </summary>
        public bool IsEligibleForEscape(Player player, Vector3 escapeZone)
        {
            if (player.IsSCP || !player.IsAlive)
                return false;

            float distance = Vector3.Distance(player.Position, escapeZone);
            return distance <= _plugin.Config.EscapeZoneSize;
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
