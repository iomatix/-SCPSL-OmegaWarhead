namespace OmegaWarhead
{
    using AudioManagerAPI.Defaults;
    using Exiled.API.Features;
    using MEC;
    using OmegaWarhead.Core.Audio;
    using OmegaWarhead.Core.AudioUtils;
    using OmegaWarhead.Core.LoggingUtils;
    using OmegaWarhead.Core.PlayerUtils;
    using OmegaWarhead.Core.RoundScenarioUtils;
    using System;
    using DoorUtility = OmegaWarhead.Core.DoorUtils;
    using EscapeUtility = OmegaWarhead.Core.PlayerUtils;
    using PlayerUtility = OmegaWarhead.Core.PlayerUtils;
    using RoomUtility = OmegaWarhead.Core.RoomUtils;
    using RoundUtility = OmegaWarhead.Core.RoundScenarioUtils;


    /// <summary>
    /// The main plugin class for the OmegaWarhead system, managing initialization, handlers, and lifecycle events.
    /// </summary>
    #region Plugin Class
    public class Plugin : Plugin<Config>
    {
        #region Fields
        /// <summary>
        /// Gets the singleton instance of the <see cref="Plugin"/> class.
        /// </summary>
        public static Plugin Singleton;

        private RoundController _roundController;
        private WarheadMethods _warheadMethods;
        private PlayerMethods _playerMethods;
        private EventHandler _eventHandler;
        private CacheHandler _cacheHandler;
        private OmegaWarheadManager _omegaManager;
        private OmegaAudioManager _omegaAudioManager;

        #endregion

        #region Plugin Metadata
        /// <summary>
        /// Gets the author of the plugin.
        /// </summary>
        public override string Author { get; } = "iomatix";

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public override string Name { get; } = "OmegaWarhead";

        /// <summary>
        /// Gets the prefix used for the plugin's configuration and logging.
        /// </summary>
        public override string Prefix { get; } = "OmegaWarhead";

        /// <summary>
        /// Gets the version of the plugin.
        /// </summary>
        public override Version Version { get; } = new Version(7, 3, 0);

        /// <summary>
        /// Gets the minimum required version of Exiled for the plugin.
        /// </summary>
        public override Version RequiredExiledVersion { get; } = new Version(9, 6, 0);
        #endregion

        #region Handler Properties
        /// <summary>
        /// Gets the round controller methods for managing round ending.
        /// </summary>
        internal RoundController RoundController { get => _roundController; private set => _roundController = value; }

        /// <summary>
        /// Gets the warhead methods handler for managing Omega Warhead sequences.
        /// </summary>
        internal WarheadMethods WarheadMethods { get => _warheadMethods; private set => _warheadMethods = value; }

        /// <summary>
        /// Gets the player methods handler for managing player-related logic.
        /// </summary>
        internal PlayerMethods PlayerMethods { get => _playerMethods; private set => _playerMethods = value; }

        /// <summary>
        /// Gets the event handler for managing server and player events.
        /// </summary>
        internal EventHandler EventHandler { get => _eventHandler; private set => _eventHandler = value; }

        /// <summary>
        /// Gets the cache handler for managing cached data.
        /// </summary>
        internal CacheHandler CacheHandler { get => _cacheHandler; private set => _cacheHandler = value; }

        /// <summary>
        /// Gets the Omega Warhead manager for handling warhead activation and detonation.
        /// </summary>
        internal OmegaWarheadManager OmegaManager { get => _omegaManager; private set => _omegaManager = value; }
        /// <summary>
        /// Gets the Omega Audio for audio management provided by AudioManagerAPI.
        /// </summary>
        public OmegaAudioManager AudioManager { get => _omegaAudioManager; private set => _omegaAudioManager = value; }
        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// Called when the plugin is enabled, initializing handlers and registering events.
        /// </summary>
        public override void OnEnabled()
        {
            Singleton = this;

            LogHelper.Debug("Enabling OmegaWarhead plugin.");
            try
            {
                Config.Validate();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Failed to validate config: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }


            #region Initialize Components
            LogHelper.Debug("Initializing handlers.");
            RoundController = new RoundController(this);
            EventHandler = new EventHandler(this);
            CacheHandler = new CacheHandler(this);
            PlayerMethods = new PlayerMethods(this);
            WarheadMethods = new WarheadMethods(this);
            OmegaManager = new OmegaWarheadManager(this);
            AudioManager = new OmegaAudioManager(this);
            #endregion


            #region Register Events
            LogHelper.Debug("Registering events.");
            EventHandler.RegisterEvents();
            OmegaManager.Init();
            #endregion

            base.OnEnabled();
            LogHelper.Info("OmegaWarhead plugin enabled.");
            OmegaManager.Init();
        }

        /// <summary>
        /// Called when the plugin is disabled, cleaning up resources and unregistering events.
        /// </summary>
        public override void OnDisabled()
        {
            LogHelper.Debug("Disabling OmegaWarhead plugin.");

            #region Disable Omega Manager
            LogHelper.Debug("Disabling OmegaWarheadManager.");
            OmegaManager?.Disable();
            #endregion

            #region Cleanup Coroutines
            LogHelper.Debug("Killing coroutines.");
            foreach (CoroutineHandle handle in EventHandler.Coroutines)
            {
                Timing.KillCoroutines(handle);
                LogHelper.Debug($"Killed coroutine: {handle}");
            }
            EventHandler.Coroutines.Clear();
            LogHelper.Debug("Cleared coroutine list.");
            #endregion

            #region Unregister Events
            LogHelper.Debug("Unregistering events.");
            EventHandler.UnregisterEvents();
            #endregion

            #region Nullify Handlers
            LogHelper.Debug("Nullifying handlers and singleton.");
            RoundController = null;
            Singleton = null;
            EventHandler = null;
            PlayerMethods = null;
            WarheadMethods = null;
            OmegaManager = null;
            #endregion

            LogHelper.Debug("OmegaWarhead plugin disabled.");
            base.OnDisabled();
        }
        #endregion
    }
    #endregion
}