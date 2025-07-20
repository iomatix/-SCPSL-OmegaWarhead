namespace BetterOmegaWarhead
{
    using BetterOmegaWarhead.Core.LoggingUtils;
    using LabApi.Features.Wrappers;
    using MapGeneration;
    using PlayerRoles;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Handles caching of runtime data for the BetterOmegaWarhead plugin,
    /// including shelter locations, players evacuated by helicopter, and disabled factions.
    /// </summary>
    public class CacheHandler
    {
        #region Fields

        private readonly Plugin _plugin;
        private HashSet<Vector3> _cachedShelterLocations;
        private HashSet<Player> _cachedHeliSurvivors;
        private HashSet<Faction> _cachedDisabledFactions;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheHandler"/> class.
        /// </summary>
        /// <param name="plugin">The plugin instance that owns this cache handler.</param>
        public CacheHandler(Plugin plugin) => _plugin = plugin;

        #endregion

        #region Shelter Caching

        /// <summary>
        /// Gets the cached list of EzEvacShelter locations.
        /// If the cache has not been initialized, it will be created on first access.
        /// </summary>
        /// <returns>
        /// A <see cref="HashSet{Vector3}"/> containing the positions of all EzEvacShelter rooms.
        /// </returns>
        public HashSet<Vector3> GetCachedShelterLocations() => CachedShelterLocations;

        /// <summary>
        /// Scans the map and collects positions of all rooms with the <see cref="RoomName.EzEvacShelter"/> designation.
        /// </summary>
        /// <returns>
        /// A <see cref="HashSet{Vector3}"/> of shelter room positions.
        /// </returns>
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

        #endregion

        #region Helicopter Evacuation Caching

        /// <summary>
        /// Adds a player to the list of helicopter-evacuated survivors.
        /// This also implicitly initializes the evacuation cache if necessary.
        /// </summary>
        /// <param name="evacuatedPlayer">The <see cref="Player"/> instance to cache.</param>
        public void CachePlayerEvacuatedByHelicopter(Player evacuatedPlayer)
        {
            LogHelper.Debug($"Caching player {evacuatedPlayer.Nickname} as evacuated by helicopter.");
            CachedHeliSurvivors.Add(evacuatedPlayer);
            LogHelper.Debug($"CachedHeliSurvivors now contains {CachedHeliSurvivors.Count} players.");
        }

        /// <summary>
        /// Gets the cached list of players who were successfully evacuated by helicopter.
        /// </summary>
        /// <returns>
        /// A <see cref="HashSet{Player}"/> of helicopter-evacuated players.
        /// </returns>
        public HashSet<Player> GetCachedPlayersEvacuatedByHelicopters()
        {
            LogHelper.Debug($"GetCachedPlayersEvacuatedByHelicopters called, returning {CachedHeliSurvivors.Count} players.");
            return CachedHeliSurvivors;
        }

        /// <summary>
        /// Checks if a specific player is marked as having evacuated via helicopter.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> instance to check.</param>
        /// <returns><c>true</c> if the player is cached as evacuated; otherwise, <c>false</c>.</returns>
        public bool IsPlayerEvacuatedByHelicopters(Player player)
        {
            bool isEvacuated = CachedHeliSurvivors.Contains(player);
            LogHelper.Debug($"Checking if {player.Nickname} is evacuated by helicopter: {isEvacuated}");
            return isEvacuated;
        }

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

        #endregion

        #region Disabled Factions Caching

        /// <summary>
        /// Gets the cached list of factions that have been marked as disabled.
        /// </summary>
        /// <returns>
        /// A <see cref="HashSet{Faction}"/> representing disabled factions.
        /// </returns>
        public HashSet<Faction> GetCachedDisabledFactions()
        {
            LogHelper.Debug($"GetCachedDisabledFactions called, returning {CachedDisabledFactions.Count} factions.");
            return CachedDisabledFactions;
        }

        /// <summary>
        /// Checks if the specified faction has been cached as disabled.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> to check.</param>
        /// <returns><c>true</c> if the faction is disabled; otherwise, <c>false</c>.</returns>
        public bool IsFactionDisabled(Faction faction)
        {
            bool isDisabled = CachedDisabledFactions.Contains(faction);
            LogHelper.Debug($"Checking if faction {faction} is disabled: {isDisabled}");
            return isDisabled;
        }

        /// <summary>
        /// Adds a faction to the cache of disabled factions.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> to disable.</param>
        public void CacheDisabledFaction(Faction faction)
        {
            CachedDisabledFactions.Add(faction);
        }

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

        #endregion

        #region Cache Reset

        /// <summary>
        /// Clears all cached data, including shelter locations, evacuated players, and disabled factions.
        /// Also disables god mode on all helicopter-evacuated players.
        /// </summary>
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

        #endregion
    }
}
