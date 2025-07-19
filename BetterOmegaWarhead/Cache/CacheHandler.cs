namespace BetterOmegaWarhead
{
    using BetterOmegaWarhead.Core.LoggingUtils;
    using LabApi.Features.Wrappers;
    using MapGeneration;
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
                    LogHelper.Debug("Initializing CachedShelterLocations via CacheShelterLocations.");
                    _cachedShelterLocations = CacheShelterLocations();
                }
                LogHelper.Debug($"Returning CachedShelterLocations with {_cachedShelterLocations.Count} locations.");
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
                    LogHelper.Debug("Initializing CachedHeliSurvivors as empty set.");
                    _cachedHeliSurvivors = new HashSet<Player>();
                }
                LogHelper.Debug($"Returning CachedHeliSurvivors with {_cachedHeliSurvivors.Count} players.");
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
                    LogHelper.Debug("Initializing CachedDisabledFactions as empty set.");
                    _cachedDisabledFactions = new HashSet<Faction>();
                }
                LogHelper.Debug($"Returning CachedDisabledFactions with {_cachedDisabledFactions.Count} factions.");
                return _cachedDisabledFactions;
            }
        }

        public HashSet<Vector3> GetCachedShelterLocations()
        {
            LogHelper.Debug("GetCachedShelterLocations called.");
            return CachedShelterLocations;
        }

        public HashSet<Vector3> CacheShelterLocations()
        {
            LogHelper.Debug("CacheShelterLocations called, scanning for EzShelter rooms.");
            var shelterLocations = new HashSet<Vector3>();
            foreach (Room room in Room.List)
            {
                if (room.Name == RoomName.EzEvacShelter)
                {
                    shelterLocations.Add(room.Position);
                    LogHelper.Debug($"Added shelter location: {room.Position}");
                }
            }
            LogHelper.Debug($"Cached {shelterLocations.Count} shelter locations.");
            return shelterLocations;
        }

        public void CachePlayerEvacuatedByHelicopter(Player evacuatedPlayer)
        {
            LogHelper.Debug($"Caching player {evacuatedPlayer.Nickname} as evacuated by helicopter.");
            CachedHeliSurvivors.Add(evacuatedPlayer);
            LogHelper.Debug($"CachedHeliSurvivors now contains {CachedHeliSurvivors.Count} players.");
        }

        public HashSet<Player> GetCachedPlayersEvacuatedByHelicopters()
        {
            LogHelper.Debug($"GetCachedPlayersEvacuatedByHelicopters called, returning {CachedHeliSurvivors.Count} players.");
            return CachedHeliSurvivors;
        }

        public bool IsPlayerEvacuatedByHelicopters(Player player)
        {
            bool isEvacuated = CachedHeliSurvivors.Contains(player);
            LogHelper.Debug($"Checking if {player.Nickname} is evacuated by helicopter: {isEvacuated}");
            return isEvacuated;
        }

        public HashSet<Faction> GetCachedDisabledFactions()
        {
            LogHelper.Debug($"GetCachedDisabledFactions called, returning {CachedDisabledFactions.Count} factions.");
            return CachedDisabledFactions;
        }

        public bool IsFactionDisabled(Faction faction)
        {
            bool isDisabled = CachedDisabledFactions.Contains(faction);
            LogHelper.Debug($"Checking if faction {faction} is disabled: {isDisabled}");
            return isDisabled;
        }

        public void CacheDisabledFaction(Faction faction)
        {
            CachedDisabledFactions.Add(faction);
        }

        public void ResetCache()
        {
            LogHelper.Debug("ResetCache called, clearing all caches.");
            _cachedShelterLocations = null;
            LogHelper.Debug("Cleared _cachedShelterLocations.");

            if (_cachedHeliSurvivors != null)
            {
                LogHelper.Debug($"Disabling god mode for {_cachedHeliSurvivors.Count} evacuated players.");
                foreach (Player player in _cachedHeliSurvivors)
                {
                    player.IsGodModeEnabled = false;
                    LogHelper.Debug($"Disabled god mode for {player.Nickname}.");
                }
                _cachedHeliSurvivors = null;
                LogHelper.Debug("Cleared _cachedHeliSurvivors.");
            }

            if (_cachedDisabledFactions != null)
            {
                LogHelper.Debug($"Clearing {_cachedDisabledFactions.Count} disabled factions.");
                _cachedDisabledFactions = null;
                LogHelper.Debug("Cleared _cachedDisabledFactions.");
            }
        }
    }
}