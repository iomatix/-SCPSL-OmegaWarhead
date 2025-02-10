namespace BetterOmegaWarhead
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using System.Collections.Generic;
    using UnityEngine;

    public class CacheHandlers
    {
        private readonly Plugin _plugin;
        public CacheHandlers(Plugin plugin) => _plugin = plugin;

        // Cached shelter locations computed once per round.
        private HashSet<Vector3> _cachedShelterLocations = null;
        private HashSet<Player> _cachedHeliSurvivors = null;

        public HashSet<Vector3> GetCachedShelterLocations()
        {
            if (_cachedShelterLocations == null)
            {
                CacheShelterLocations();
            }
            return _cachedShelterLocations;
        }
        public HashSet<Vector3> CacheShelterLocations()
        {
            _cachedShelterLocations = new HashSet<Vector3>();
            foreach (Room room in Room.List)
            {
                if (room.Type == RoomType.EzShelter)
                {
                    _cachedShelterLocations.Add(room.Position);
                }
            }
            return _cachedShelterLocations;
        }

        public void CachePlayerEvacuatedByHelicopter(Player evacuatedPlayer)
        {
            _cachedHeliSurvivors.Add(evacuatedPlayer);
        }

        public HashSet<Player> GetCachedPlayersEvacuatedByHelicopters()
        {
            return _cachedHeliSurvivors;
        }

        public bool IsPlayerEvacuatedByHelicopters(Player player)
        {
            return _cachedHeliSurvivors.Contains(player);
        }


        public void ResetCache()
        {
            _cachedShelterLocations = null;
            foreach(Player player in _cachedHeliSurvivors)
            {
                player.IsGodModeEnabled = false;
            }
            _cachedHeliSurvivors = null;
        }
    }
}
