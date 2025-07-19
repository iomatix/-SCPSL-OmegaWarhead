namespace BetterOmegaWarhead
{
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using PlayerRoles;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class CacheHandler
    {
        private readonly Plugin _plugin;
        public CacheHandler(Plugin plugin) => _plugin = plugin;

        private HashSet<Vector3> _cachedShelterLocations;
        private HashSet<Player> _cachedHeliSurvivors;
        private HashSet<Faction> _cachedDisabledFactions;

        // Lazy initialization of shelter locations using a property getter.
        private HashSet<Vector3> CachedShelterLocations
        {
            get
            {
                if (_cachedShelterLocations == null)
                {
                    Log.Debug("Initializing CachedShelterLocations via CacheShelterLocations.");
                    _cachedShelterLocations = CacheShelterLocations();
                }
                Log.Debug($"Returning CachedShelterLocations with {_cachedShelterLocations.Count} locations.");
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
                    Log.Debug("Initializing CachedHeliSurvivors as empty set.");
                    _cachedHeliSurvivors = new HashSet<Player>();
                }
                Log.Debug($"Returning CachedHeliSurvivors with {_cachedHeliSurvivors.Count} players.");
                return _cachedHeliSurvivors;
            }
        }

        // Lazy initialization of disabled factions.
        private HashSet<Faction> CachedDisabledFactions
        {
            get
            {
                if (_cachedDisabledFactions == null)
                {
                    Log.Debug("Initializing CachedDisabledFactions as empty set.");
                    _cachedDisabledFactions = new HashSet<Faction>();
                }
                Log.Debug($"Returning CachedDisabledFactions with {_cachedDisabledFactions.Count} factions.");
                return _cachedDisabledFactions;
            }
        }

        public HashSet<Vector3> GetCachedShelterLocations()
        {
            Log.Debug("GetCachedShelterLocations called.");
            return CachedShelterLocations;
        }

        public HashSet<Vector3> CacheShelterLocations()
        {
            Log.Debug("CacheShelterLocations called, scanning for EzShelter rooms.");
            var shelterLocations = new HashSet<Vector3>();
            foreach (Room room in Room.List)
            {
                if (room.Type == RoomType.EzShelter)
                {
                    shelterLocations.Add(room.Position);
                    Log.Debug($"Added shelter location: {room.Position}");
                }
            }
            Log.Debug($"Cached {shelterLocations.Count} shelter locations.");
            return shelterLocations;
        }

        public void CachePlayerEvacuatedByHelicopter(Player evacuatedPlayer)
        {
            Log.Debug($"Caching player {evacuatedPlayer.Nickname} as evacuated by helicopter.");
            CachedHeliSurvivors.Add(evacuatedPlayer);
            Log.Debug($"CachedHeliSurvivors now contains {CachedHeliSurvivors.Count} players.");
        }

        public HashSet<Player> GetCachedPlayersEvacuatedByHelicopters()
        {
            Log.Debug($"GetCachedPlayersEvacuatedByHelicopters called, returning {CachedHeliSurvivors.Count} players.");
            return CachedHeliSurvivors;
        }

        public bool IsPlayerEvacuatedByHelicopters(Player player)
        {
            bool isEvacuated = CachedHeliSurvivors.Contains(player);
            Log.Debug($"Checking if {player.Nickname} is evacuated by helicopter: {isEvacuated}");
            return isEvacuated;
        }

        public HashSet<Faction> GetCachedDisabledFactions()
        {
            Log.Debug($"GetCachedDisabledFactions called, returning {CachedDisabledFactions.Count} factions.");
            return CachedDisabledFactions;
        }

        public bool IsFactionDisabled(Faction faction)
        {
            bool isDisabled = CachedDisabledFactions.Contains(faction);
            Log.Debug($"Checking if faction {faction} is disabled: {isDisabled}");
            return isDisabled;
        }

        public void CacheDisabledFaction(Faction faction)
        {
            CachedDisabledFactions.Add(faction);
        }

        public void ResetCache()
        {
            Log.Debug("ResetCache called, clearing all caches.");
            _cachedShelterLocations = null;
            Log.Debug("Cleared _cachedShelterLocations.");

            if (_cachedHeliSurvivors != null)
            {
                Log.Debug($"Disabling god mode for {_cachedHeliSurvivors.Count} evacuated players.");
                foreach (Player player in _cachedHeliSurvivors)
                {
                    player.IsGodModeEnabled = false;
                    Log.Debug($"Disabled god mode for {player.Nickname}.");
                }
                _cachedHeliSurvivors = null;
                Log.Debug("Cleared _cachedHeliSurvivors.");
            }

            if (_cachedDisabledFactions != null)
            {
                Log.Debug($"Clearing {_cachedDisabledFactions.Count} disabled factions.");
                _cachedDisabledFactions = null;
                Log.Debug("Cleared _cachedDisabledFactions.");
            }
        }
    }
}