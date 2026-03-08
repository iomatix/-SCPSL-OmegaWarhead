namespace OmegaWarhead
{
    using OmegaWarhead.Core.LoggingUtils;
    using OmegaWarhead.Core.PlayerUtils;
    using LabApi.Features.Wrappers;
    using MapGeneration;
    using PlayerRoles;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using static OmegaWarhead.Core.PlayerUtils.PlayerMethods;

    /// <summary>
    /// Handles caching of runtime data for the OmegaWarhead plugin,
    /// including shelter locations, players evacuated by helicopter, and disabled factions.
    /// </summary>
    public class CacheHandler
    {
        #region Fields

        private readonly Plugin _plugin;
        private HashSet<Vector3> _cachedShelterLocations;
        private HashSet<Player> _cachedHeliSurvivors;
        private HashSet<Faction> _cachedDisabledFactions;
        private Dictionary<Player, PlayerFate> _cachedPlayerFates;

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
            LogHelper.Debug("Scanning for EzShelter rooms and caching locations.");
            var shelterLocations = new HashSet<Vector3>();
            foreach (Room room in Room.List)
            {
                if (room.Name == RoomName.EzEvacShelter)
                {
                    shelterLocations.Add(room.Position);
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
                    _cachedShelterLocations = CacheShelterLocations();
                }
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
            CachedHeliSurvivors.Add(evacuatedPlayer);
            LogHelper.Debug($"Cached player {evacuatedPlayer.Nickname} as evacuated by helicopter.");
        }

        /// <summary>
        /// Gets the cached list of players who were successfully evacuated by helicopter.
        /// </summary>
        /// <returns>
        /// A <see cref="HashSet{Player}"/> of helicopter-evacuated players.
        /// </returns>
        public HashSet<Player> GetCachedPlayersEvacuatedByHelicopters() => CachedHeliSurvivors;

        /// <summary>
        /// Checks if a specific player is marked as having evacuated via helicopter.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> instance to check.</param>
        /// <returns><c>true</c> if the player is cached as evacuated; otherwise, <c>false</c>.</returns>
        public bool IsPlayerEvacuatedByHelicopters(Player player)
        {
            return CachedHeliSurvivors.Contains(player);
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
        public HashSet<Faction> GetCachedDisabledFactions() => CachedDisabledFactions;

        /// <summary>
        /// Checks if the specified faction has been cached as disabled.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> to check.</param>
        /// <returns><c>true</c> if the faction is disabled; otherwise, <c>false</c>.</returns>
        public bool IsFactionDisabled(Faction faction)
        {
            return CachedDisabledFactions.Contains(faction);
        }

        /// <summary>
        /// Adds a faction to the cache of disabled factions.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> to disable.</param>
        public void CacheDisabledFaction(Faction faction)
        {
            CachedDisabledFactions.Add(faction);
            LogHelper.Debug($"Cached faction {faction} as disabled.");
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
                return _cachedDisabledFactions;
            }
        }

        #endregion

        #region Player Fate Caching

        /// <summary>
        /// Gets the dictionary containing all cached player fates recorded during the current round.
        /// This provides full access to player-to-fate mappings for analysis, filtering, or display.
        /// </summary>
        /// <returns>
        /// A <see cref="Dictionary{Player, PlayerFate}"/> representing players and their associated fates.
        /// </returns>
        public Dictionary<Player, PlayerFate> GetCachedPlayerFates() => CachedPlayerFates;

        /// <summary>
        /// Caches the fate of a specific player within the current round context.
        /// If the player already has a cached fate, it will be overwritten.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> instance whose fate is being cached.</param>
        /// <param name="fate">The <see cref="PlayerFate"/> value representing the player's outcome.</param>
        public void CachePlayerFate(Player player, PlayerFate fate)
        {
            CachedPlayerFates[player] = fate;
            LogHelper.Debug($"Cached fate for {player.Nickname}: {fate}");
        }

        /// <summary>
        /// Retrieves the cached fate of a given player.
        /// If no fate is found in the cache, <see cref="PlayerFate.Unknown"/> is returned.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> instance to query.</param>
        /// <returns>
        /// The <see cref="PlayerFate"/> associated with the player, or <see cref="PlayerFate.Unknown"/> if not found.
        /// </returns>
        public PlayerFate GetCachedPlayerFate(Player player)
        {
            if (CachedPlayerFates.TryGetValue(player, out PlayerFate fate))
                return fate;

            return PlayerFate.Unknown;
        }

        /// <summary>
        /// Retrieves an enumeration of players who have a cached fate assigned during the current round.
        /// Avoiding HashSet reallocation for memory safety.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{Player}"/> containing all players with a known fate.
        /// </returns>
        public IEnumerable<Player> GetPlayersWithCachedFate()
        {
            return CachedPlayerFates.Keys;
        }

        private Dictionary<Player, PlayerFate> CachedPlayerFates
        {
            get
            {
                if (_cachedPlayerFates == null)
                {
                    LogHelper.Debug("Initializing CachedPlayerFates as empty dictionary.");
                    _cachedPlayerFates = new Dictionary<Player, PlayerFate>();
                }
                return _cachedPlayerFates;
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

            if (_cachedHeliSurvivors != null)
            {
                foreach (Player player in _cachedHeliSurvivors)
                {
                    if (player != null && player.IsReady)
                    {
                        try
                        {
                            player.IsGodModeEnabled = false;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Warning($"Could not disable godmode for {player.Nickname}: {ex.Message}");
                        }
                    }
                }
                _cachedHeliSurvivors.Clear();
                _cachedHeliSurvivors = null;
            }

            if (_cachedDisabledFactions != null)
            {
                _cachedDisabledFactions.Clear();
                _cachedDisabledFactions = null;
            }

            if (_cachedPlayerFates != null)
            {
                _cachedPlayerFates.Clear();
                _cachedPlayerFates = null;
            }
        }

        #endregion
    }
}