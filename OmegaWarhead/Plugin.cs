using LabApi.Extensions;
using LabApi.Extensions.Plugin;
using LabApi.Extensions.RoundManagement;
using LabApi.Loader.Features.Plugins;
using OmegaWarhead.Core.Audio;
using OmegaWarhead.Core.PlayerUtils;
using OmegaWarhead.Core.RoundScenarioUtils;
using System;
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
        public override Version Version => new Version(10, 1, 2);
        public override Version RequiredApiVersion => new Version(1, 0, 0);
        #endregion

        #region Singleton Reference
        public static Plugin Singleton { get; private set; }
        #endregion

        #region Subsystem Infrastructure Properties
        internal RoundController RoundController { get; private set; }
        internal WarheadMethods WarheadMethods { get; private set; }
        internal PlayerMethods PlayerMethods { get; private set; }
        internal EventHandler EventHandler { get; private set; }
        internal CacheHandler CacheHandler { get; private set; }
        internal OmegaWarheadManager OmegaManager { get; private set; }
        public OmegaAudioManager AudioManager { get; private set; }
        #endregion

        #region Lifecycle Matrix Hooks
        public override void LoadConfigs()
        {
            base.LoadConfigs();
            Config?.Validate();
        }

        public override void Enable()
        {
            Singleton = this;

            Logger.Debug(Name, "Enabling OmegaWarhead via pristine inferred PluginBuilder pipelines.", Config?.Debug ?? false);

            try
            {
                Config?.Validate();
            }
            catch (Exception ex)
            {
                Logger.Error(Name, $"OmegaWarhead initialization aborted due to invalid configuration: {ex.Message}");
                return;
            }

            PluginBuilder.Create(this)
                .InitializeModule(() => RoundController = new RoundController())
                .InitializeModule(() => EventHandler = new EventHandler(this))
                .InitializeModule(() => CacheHandler = new CacheHandler(this))
                .InitializeModule(() => PlayerMethods = new PlayerMethods(this))
                .InitializeModule(() => WarheadMethods = new WarheadMethods(this))
                .InitializeModule(() => OmegaManager = new OmegaWarheadManager(this))
                .InitializeModule(() => AudioManager = new OmegaAudioManager(this));

            RoundController.RegisterScenario(new DetonationEndingScenario(RoundController));
            EventHandler.RegisterEvents();

            Logger.Info(Name, "OmegaWarhead plugin infrastructure successfully built and enabled.");
            OmegaManager?.Init();
        }

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