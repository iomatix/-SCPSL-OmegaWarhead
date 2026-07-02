namespace OmegaWarhead
{
    using System;
    using LabApi.Loader.Features.Plugins;
    using MEC;
    using OmegaWarhead.Core.Audio;
    using OmegaWarhead.Core.LoggingUtils;
    using OmegaWarhead.Core.PlayerUtils;
    using OmegaWarhead.Core.RoundScenarioUtils;

    /// <summary>
    /// The main plugin class for the OmegaWarhead system, managing initialization, handlers, and lifecycle events inside LabAPI.
    /// </summary>
    public class Plugin : Plugin<Config>
    {
        #region Subsystem Providers
        private RoundController _roundController;
        private WarheadMethods _warheadMethods;
        private PlayerMethods _playerMethods;
        private EventHandler _eventHandler;
        private CacheHandler _cacheHandler;
        private OmegaWarheadManager _omegaManager;
        private OmegaAudioManager _omegaAudioManager;
        #endregion

        /// <summary>
        /// Gets the singleton instance of the <see cref="Plugin"/> class.
        /// </summary>
        public static Plugin Singleton { get; private set; }

        #region LabAPI Mandated Metadata
        public override string Author => "iomatix";
        public override string Name => "OmegaWarhead";
        public override string Description => "Advanced facility alternative detonation and escape sequence control engine.";
        public override Version Version => new Version(8, 0, 0);
        public override Version RequiredApiVersion => new Version(1, 0, 0);
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
        /// Native LabAPI configuration framework hook.
        /// </summary>
        public override void LoadConfigs()
        {
            base.LoadConfigs();
            Config.Validate();
        }

        /// <summary>
        /// LabAPI structural entry point. Allocates decentralized operational handlers and registers engine hooks.
        /// </summary>
        public override void Enable()
        {
            Singleton = this;

            LogHelper.Debug("Enabling OmegaWarhead plugin under LabAPI execution context.");
            try
            {
                Config.Validate();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Failed to validate config: {ex.Message}\nStackTrace: {ex.StackTrace}");
                LogHelper.Error("OmegaWarhead initialization aborted due to invalid configuration.");
                return;
            }

            #region Initialize Components
            LogHelper.Debug("Allocating decoupled internal handler domains.");
            RoundController = new RoundController(this);
            EventHandler = new EventHandler(this);
            CacheHandler = new CacheHandler(this);
            PlayerMethods = new PlayerMethods(this);
            WarheadMethods = new WarheadMethods(this);
            OmegaManager = new OmegaWarheadManager(this);
            AudioManager = new OmegaAudioManager(this);
            #endregion

            #region Register Events
            LogHelper.Debug("Binding event pipeline abstractions.");
            EventHandler.RegisterEvents();
            #endregion

            LogHelper.Info("OmegaWarhead plugin successfully enabled.");
            OmegaManager.Init();
        }

        /// <summary>
        /// LabAPI structural teardown layer. Deallocates subsystem contexts, terminates background processes, and releases hooks.
        /// </summary>
        public override void Disable()
        {
            LogHelper.Debug("Disabling OmegaWarhead plugin framework.");

            try
            {
                LogHelper.Debug("Deactivating OmegaWarheadManager lifecycle core.");
                OmegaManager?.Disable();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Error while disabling OmegaManager: {ex}");
            }

            try
            {
                LogHelper.Debug("Terminating active execution threads and coroutines.");
                foreach (string tag in Shared.CoroutineTags.AllStaticTags)
                {
                    Timing.KillCoroutines(tag);
                }
                LogHelper.Debug("Cleared coroutines via structural tags successfully.");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Error while killing coroutines: {ex}");
            }

            try
            {
                LogHelper.Debug("Detaching event handler structural bridges.");
                EventHandler?.UnregisterEvents();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Error while unregistering events: {ex}");
            }

            LogHelper.Debug("Nullifying instance references to permit clean GC sweep.");
            RoundController = null;
            EventHandler = null;
            PlayerMethods = null;
            WarheadMethods = null;
            OmegaManager = null;
            CacheHandler = null;
            AudioManager = null;
            Singleton = null;

            LogHelper.Debug("OmegaWarhead plugin successfully offline.");
        }
        #endregion
    }
}