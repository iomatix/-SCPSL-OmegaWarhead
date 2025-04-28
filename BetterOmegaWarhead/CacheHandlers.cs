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

        private HashSet<Vector3> _cachedShelterLocations;
        private HashSet<Player> _cachedHeliSurvivors;
        // Lazy initialization of shelter locations using a property getter.
        private HashSet<Vector3> CachedShelterLocations
        {
            get
            {
                if (_cachedShelterLocations == null)
                {
                    _cachedShelterLocations = CacheShelterLocations();
                }
                return _cachedShelterLocations;
            }
        }

        // Lazy initialization of heli survivors using a property getter.
        private HashSet<Player> CachedHeliSurvivors
        {
            get
            {
                if (_cachedHeliSurvivors == null)
                {
                    _cachedHeliSurvivors = new HashSet<Player>();
                }
                return _cachedHeliSurvivors;
            }
        }

        public HashSet<Vector3> GetCachedShelterLocations() => CachedShelterLocations;

        public HashSet<Vector3> CacheShelterLocations()
        {
            var shelterLocations = new HashSet<Vector3>();
            foreach (Room room in Room.List)
            {
                if (room.Type == RoomType.EzShelter)
                {
                    shelterLocations.Add(room.Position);
                }
            }
            return shelterLocations;
        }

        public void CachePlayerEvacuatedByHelicopter(Player evacuatedPlayer)
        {
            CachedHeliSurvivors.Add(evacuatedPlayer);
        }

        public HashSet<Player> GetCachedPlayersEvacuatedByHelicopters() =>
            CachedHeliSurvivors;

        public bool IsPlayerEvacuatedByHelicopters(Player player) =>
            CachedHeliSurvivors.Contains(player);


        public void ResetCache()
        {
            _cachedShelterLocations = null;
            if (_cachedHeliSurvivors != null)
            {
                foreach (Player player in _cachedHeliSurvivors)
                {
                    player.IsGodModeEnabled = false;
                }
                _cachedHeliSurvivors = null;
            }
        }
    }
}
