using System;
using LabApi.Loader.Features.Plugins;
using LabApi.Extensions;
using LabApi.Extensions.Misc;
using MEC;
using OmegaWarhead.Core.Audio;
using OmegaWarhead.Core.PlayerUtils;
using OmegaWarhead.Core.RoundScenarioUtils;

using Logger = LabApi.Extensions.Misc.iLogger;

namespace OmegaWarhead
{
    /// <summary>
    /// The main plugin class for the OmegaWarhead system, managing initialization, handlers, and lifecycle events inside LabAPI.
    /// </summary>
    public class Plugin : Plugin<Config>
    {
        #region Metadata Providers
        public override string Author => "iomatix";
        public override string Name => "OmegaWarhead";
        public override string Description => "Advanced facility alternative detonation and escape sequence control engine.";
        public override Version Version => new Version(10, 0, 0);
        public override Version RequiredApiVersion => new Version(1, 0, 0);
        #endregion

        #region Singleton Reference
        /// <summary>
        /// Gets the singleton instance of the <see cref="Plugin"/> class.
        /// </summary>
        public static Plugin Singleton { get; private set; }
        #endregion

        #region Subsystem Infrastructure Properties
        /// <summary>
        /// Gets the round controller methods for managing round ending.
        /// </summary>
        internal RoundController RoundController { get; private set; }

        /// <summary>
        /// Gets the warhead methods handler for managing Omega Warhead sequences.
        /// </summary>
        internal WarheadMethods WarheadMethods { get; private set; }

        /// <summary>
        /// Gets the player methods handler for managing player-related logic.
        /// </summary>
        internal PlayerMethods PlayerMethods { get; private set; }

        /// <summary>
        /// Gets the event handler for managing server and player events.
        /// </summary>
        internal EventHandler EventHandler { get; private set; }

        /// <summary>
        /// Gets the cache handler for managing cached data.
        /// </summary>
        internal CacheHandler CacheHandler { get; private set; }

        /// <summary>
        /// Gets the Omega Warhead manager for handling warhead activation and detonation.
        /// </summary>
        internal OmegaWarheadManager OmegaManager { get; private set; }

        /// <summary>
        /// Gets the Omega Audio for audio management provided by AudioManagerAPI.
        /// </summary>
        public OmegaAudioManager AudioManager { get; private set; }
        #endregion

        #region Lifecycle Matrix Hooks
        /// <summary>
        /// Native LabAPI configuration framework hook.
        /// </summary>
        public override void LoadConfigs()
        {
            base.LoadConfigs();
            Config?.Validate();
        }

        /// <summary>
        /// LabAPI structural entry point. Allocates decentralized operational handlers and registers engine hooks.
        /// </summary>
        public override void Enable()
        {
            Singleton = this;

            Logger.Debug(Name, "Enabling OmegaWarhead plugin under LabAPI execution context.", Config?.Debug ?? false);

            try
            {
                Config?.Validate();
            }
            catch (Exception ex)
            {
                Logger.Error(Name, $"OmegaWarhead initialization aborted due to invalid configuration: {ex.Message}");
                return;
            }

            // Allocation Layer: Constructing pristine operational domain managers
            RoundController = new RoundController(this);
            EventHandler = new EventHandler(this);
            CacheHandler = new CacheHandler(this);
            PlayerMethods = new PlayerMethods(this);
            WarheadMethods = new WarheadMethods(this);
            OmegaManager = new OmegaWarheadManager(this);
            AudioManager = new OmegaAudioManager(this);

            // Event Registration Pipeline
            EventHandler.RegisterEvents();

            Logger.Info(Name, "OmegaWarhead plugin infrastructure successfully enabled.");
            OmegaManager?.Init();
        }

        /// <summary>
        /// LabAPI structural teardown layer. Deallocates subsystem contexts, terminates background processes, and releases hooks.
        /// </summary>
        public override void Disable()
        {
            Logger.Debug(Name, "Disabling OmegaWarhead plugin framework.", Config?.Debug ?? false);

            try
            {
                OmegaManager?.Disable();
            }
            catch (Exception ex)
            {
                Logger.Error(Name, $"Failed to deactivate OmegaWarheadManager lifecycle core smoothly: {ex.Message}");
            }

            try
            {
                // Fluent API Alignment: Direct collection extension utilization to evict trailing routines atomically
                Shared.CoroutineTags.AllStaticTags.KillCoroutines();
                Logger.Debug(Name, "Cleared active execution threads and coroutines via structural tags successfully.", Config?.Debug ?? false);
            }
            catch (Exception ex)
            {
                Logger.Error(Name, $"Anomalous coroutine thread eviction interruption: {ex.Message}");
            }

            try
            {
                EventHandler?.UnregisterEvents();
            }
            catch (Exception ex)
            {
                Logger.Error(Name, $"Failed to detach event handler structural bridges securely: {ex.Message}");
            }

            // Nullify instance references to permit clean garbage collection sweeps instantly
            RoundController = null;
            EventHandler = null;
            PlayerMethods = null;
            WarheadMethods = null;
            OmegaManager = null;
            CacheHandler = null;
            AudioManager = null;
            Singleton = null;

            Logger.Info(Name, "OmegaWarhead plugin lifecycle successfully forced offline.");
        }
        #endregion
    }
}