namespace OmegaWarhead
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using MapGeneration;
    using PlayerRoles;
    using LabApi.Features.Wrappers;
    using OmegaWarhead.Core.LoggingUtils;
    using static OmegaWarhead.Core.PlayerUtils.PlayerMethods;

    /// <summary>
    /// Handles deterministic caching of runtime entities and environmental coordinates for the OmegaWarhead system.
    /// Fully decoupled from global static state targets to guarantee memory isolation and absolute leak prevention.
    /// </summary>
    public class CacheHandler
    {
        #region Private Repositories
        private readonly Plugin _plugin;
        private HashSet<Vector3> _cachedShelterLocations;
        private HashSet<Player> _cachedHeliSurvivors;
        private HashSet<Faction> _cachedDisabledFactions;
        private Dictionary<Player, PlayerFate> _cachedPlayerFates;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheHandler"/> class linked to a parent lifecycle scope.
        /// </summary>
        public CacheHandler(Plugin plugin) => _plugin = plugin;
        #endregion

        #region Shelter Coordinates Cache
        /// <summary>
        /// Resolves the cached collection of validated EzEvacShelter world coordinates.
        /// </summary>
        public HashSet<Vector3> GetCachedShelterLocations() => CachedShelterLocations;

        /// <summary>
        /// Scans active spatial room assets and isolates absolute vectors designating structural emergency shelter containment points.
        /// </summary>
        public HashSet<Vector3> CacheShelterLocations()
        {
            LogHelper.Debug(nameof(CacheHandler), "Executing spatial map analytics scan for active shelter zone facilities...");

            var shelterLocations = new HashSet<Vector3>();

            foreach (Room room in Room.List)
            {
                if (room.Name == RoomName.EzEvacShelter)
                {
                    shelterLocations.Add(room.Position);
                }
            }

            LogHelper.Debug(nameof(CacheHandler), $"Structural scan complete. Registered {shelterLocations.Count} emergency shelter facilities inside cache matrix.");
            return shelterLocations;
        }

        private HashSet<Vector3> CachedShelterLocations => _cachedShelterLocations ??= CacheShelterLocations();
        #endregion

        #region Extraction Lifecycle Cache
        /// <summary>
        /// Registers an active player identity into the persistent air extraction survival collection.
        /// </summary>
        public void CachePlayerEvacuatedByHelicopter(Player evacuatedPlayer)
        {
            if (evacuatedPlayer == null) return;

            CachedHeliSurvivors.Add(evacuatedPlayer);
            LogHelper.Debug(nameof(CacheHandler), $"Player '{evacuatedPlayer.Nickname}' securely committed to helicopter airlift extraction logs.");
        }

        /// <summary>
        /// Resolves the collection of player entities tracked as evacuated via air extraction assets.
        /// </summary>
        public HashSet<Player> GetCachedPlayersEvacuatedByHelicopters() => CachedHeliSurvivors;

        /// <summary>
        /// Evaluates whether a target player entity matches an active airlift extraction signature.
        /// </summary>
        public bool IsPlayerEvacuatedByHelicopters(Player player) => player != null && CachedHeliSurvivors.Contains(player);

        private HashSet<Player> CachedHeliSurvivors => _cachedHeliSurvivors ??= new HashSet<Player>();
        #endregion

        #region Disabled Factions Diagnostics
        /// <summary>
        /// Resolves the tracked collection of combat or neutral factions locked from active engagement matrices.
        /// </summary>
        public HashSet<Faction> GetCachedDisabledFactions() => CachedDisabledFactions;

        /// <summary>
        /// Determines if a target faction signature is locked down inside the active exclusion matrix.
        /// </summary>
        public bool IsFactionDisabled(Faction faction) => CachedDisabledFactions.Contains(faction);

        /// <summary>
        /// Pushes a specified faction signature into the active operational exclusion collection.
        /// </summary>
        public void CacheDisabledFaction(Faction faction)
        {
            CachedDisabledFactions.Add(faction);
            LogHelper.Debug(nameof(CacheHandler), $"Faction authorization vector '{faction}' successfully committed to exclusion cache.");
        }

        private HashSet<Faction> CachedDisabledFactions => _cachedDisabledFactions ??= new HashSet<Faction>();
        #endregion

        #region Statistical Player Fate Engine
        /// <summary>
        /// Resolves the complete collection tracking player outcomes for dynamic endgame statistic compilations.
        /// </summary>
        public Dictionary<Player, PlayerFate> GetCachedPlayerFates() => CachedPlayerFates;

        /// <summary>
        /// Commits or overwrites a categorical historical outcome for a target player entry.
        /// </summary>
        public void CachePlayerFate(Player player, PlayerFate fate)
        {
            if (player == null) return;

            CachedPlayerFates[player] = fate;
            LogHelper.Debug(nameof(CacheHandler), $"Committed historical lifecycle outcome for player '{player.Nickname}': [{fate}]");
        }

        /// <summary>
        /// Resolves the assigned outcome for a target player context. Defaults to Unknown if signature tracking missing.
        /// </summary>
        public PlayerFate GetCachedPlayerFate(Player player)
        {
            if (player != null && CachedPlayerFates.TryGetValue(player, out PlayerFate fate))
                return fate;

            return PlayerFate.Unknown;
        }

        /// <summary>
        /// Exposes raw tracking keys to facilitate memory-safe linear enumeration without array reallocations.
        /// </summary>
        public IEnumerable<Player> GetPlayersWithCachedFate() => CachedPlayerFates.Keys;

        private Dictionary<Player, PlayerFate> CachedPlayerFates => _cachedPlayerFates ??= new Dictionary<Player, PlayerFate>();
        #endregion

        #region Defensive Pipeline Purge
        /// <summary>
        /// Executes a complete structural purge of all tracking graphs. Resolves invulnerability contexts on entities prior to memory release.
        /// </summary>
        public void ResetCache()
        {
            LogHelper.Debug(nameof(CacheHandler), "Global cache eviction cycle triggered. Dismantling memory graphs...");

            _cachedShelterLocations = null;

            if (_cachedHeliSurvivors != null)
            {
                foreach (Player player in _cachedHeliSurvivors)
                {
                    if (player?.IsReady == true)
                    {
                        try
                        {
                            player.IsGodModeEnabled = false;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Warning(nameof(CacheHandler), $"Failed to strip extraction invulnerability layer from '{player.Nickname}': {ex.Message}");
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

            LogHelper.Debug(nameof(CacheHandler), "All data repositories securely unlinked and cleared for Garbage Collection sweep.");
        }
        #endregion
    }
}