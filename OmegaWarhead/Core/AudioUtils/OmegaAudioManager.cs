using AudioManagerAPI.Defaults;
using AudioManagerAPI.Features.Enums;
using AudioManagerAPI.Features.Management;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = LabApi.Extensions.Misc.iLogger;

namespace OmegaWarhead.Core.Audio
{
    /// <summary>
    /// Defines the internal audio keys specific to the OmegaWarhead subsystem.
    /// </summary>
    public enum OmegaAudioKey
    {
        Siren,
        EndingMusic
    }

    /// <summary>
    /// Coordinates embedded audio resource deployment pipelines and manages dynamic playback sessions 
    /// via the external SCPSL-AudioManagerAPI ecosystem.
    /// </summary>
    public class OmegaAudioManager
    {
        #region Private Repositories & Session Handlers
        private readonly Plugin _plugin;
        private readonly IAudioManager _audioManager;

        private int _sirenSessionId;
        private int _endingMusicSessionId;

        // Target-typed dictionary mapping configuration rules securely using the local enum context
        private readonly Dictionary<OmegaAudioKey, (string key, string resourceName)> _audioConfig = new()
        {
            { OmegaAudioKey.Siren, ("siren", "OmegaWarhead.Shared.Audio.Files.omegawarhead.siren.wav") },
            { OmegaAudioKey.EndingMusic, ("endingmusic", "OmegaWarhead.Shared.Audio.Files.omegawarhead.endingmusic.wav") }
        };
        #endregion

        #region Initialization & Resource Registration
        /// <summary>
        /// Initializes a new instance of the <see cref="OmegaAudioManager"/> class bound to the plugin lifecycle context.
        /// </summary>
        public OmegaAudioManager(Plugin plugin)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin), "Structural allocation failure: Parent plugin context evaluates to null.");
            _audioManager = DefaultAudioManager.Instance;

            RegisterAudioResources();
        }

        /// <summary>
        /// Scans the compiled assembly layer, validates embedded wave streams, and registers content delivery blocks.
        /// </summary>
        private void RegisterAudioResources()
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var pair in _audioConfig)
            {
                string key = pair.Value.key;
                string resourcePath = pair.Value.resourceName;

                // Pre-flight defensive validation block ensuring embedded physical resource maps correctly
                using (var testStream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (testStream is null)
                    {
                        Logger.Error(nameof(OmegaAudioManager), $"Asset Extraction Defect: Manifest embedded path '{resourcePath}' could not be resolved. Verify build action fields are set to 'Embedded Resource'.");
                        continue;
                    }

                    if (testStream.Length == 0)
                    {
                        Logger.Error(nameof(OmegaAudioManager), $"Asset Extraction Defect: Manifest path resource target '{resourcePath}' evaluates to a 0-byte unallocated file allocation.");
                        continue;
                    }

                    Logger.Debug(nameof(OmegaAudioManager), $"Asset tracking footprint verified successfully: {resourcePath} ({testStream.Length} bytes).", _plugin.Config.Debug);
                }

                // Inject lazy-loaded audio stream factory straight into the AudioManagerAPI backend registry
                _audioManager.RegisterAudio(key, () =>
                {
                    var stream = assembly.GetManifestResourceStream(resourcePath);
                    if (stream is null)
                    {
                        Logger.Error(nameof(OmegaAudioManager), $"Lazy-load extraction pipeline broken for asset key token '{key}' under path coordinates: {resourcePath}");
                    }
                    return stream;
                });

                Logger.Info(nameof(OmegaAudioManager), $"Audio pipeline content provider successfully registered for key identifier: [{key}]");
            }
        }
        #endregion

        #region Operational Playback Controllers
        /// <summary>
        /// Dispatches a looping global audio playback tracking sequence simulating secondary facility alert siren systems.
        /// </summary>
        public int PlayOmegaSiren()
        {
            if (_sirenSessionId != 0)
            {
                _audioManager.FadeOutAudio(_sirenSessionId, 2f);
                Logger.Debug(nameof(OmegaAudioManager), $"Terminated pre-existing active siren context under session token: {_sirenSessionId}.", _plugin.Config.Debug);
                _sirenSessionId = 0;
            }

            if (_audioManager is null)
            {
                Logger.Warn(nameof(OmegaAudioManager), "Audio dispatch canceled: Native IAudioManager endpoint context references null.");
                return 0;
            }

            // Extract the standardized string identifier directly from the internal config map
            string audioKey = _audioConfig[OmegaAudioKey.Siren].key;

            try
            {
                // Architectural Upgrade: Execute via the generic pipeline using a cached static filter delegate
                _sirenSessionId = _audioManager.PlayGlobalAudio<object>(
                    key: audioKey,
                    state: null,
                    validPlayersFilter: (player, _) => player != null && player.IsReady,
                    loop: true,
                    volume: 0.8f,
                    priority: AudioPriority.High,
                    queue: false,
                    fadeInDuration: 1.5f,
                    persistent: true,
                    lifespan: null,          // Persistent continuous looping block until explicit manual termination
                    autoCleanup: false
                );
            }
            catch (Exception ex)
            {
                Logger.Warn(nameof(OmegaAudioManager), $"Exception captured inside native audio engine framework binding sequence: {ex.Message}");
                return 0;
            }

            if (_sirenSessionId == 0)
            {
                Logger.Warn(nameof(OmegaAudioManager), "Audio engine allocation rejected: Playback engine returns empty structural tracking token.");
                return 0;
            }

            Logger.Info(nameof(OmegaAudioManager), $"Looping facility alternative evacuation siren engaged. Active Session ID: {_sirenSessionId}");
            return _sirenSessionId;
        }

        /// <summary>
        /// Fades out and releases the active evacuation siren tracking handle.
        /// </summary>
        public void StopOmegaSiren()
        {
            if (_sirenSessionId != 0)
            {
                _audioManager.FadeOutAudio(_sirenSessionId, 2f);
                Logger.Info(nameof(OmegaAudioManager), $"Fading out alternative evacuation siren system layer. Retiring Session ID: {_sirenSessionId}");
                _sirenSessionId = 0;
            }
        }

        /// <summary>
        /// Dispatches the final atmospheric closing audio arrangement track across global server audio outputs.
        /// </summary>
        public int PlayEndingMusic(float lifespan = 109f)
        {
            if (_endingMusicSessionId != 0)
            {
                _audioManager.FadeOutAudio(_endingMusicSessionId, 2f);
                Logger.Debug(nameof(OmegaAudioManager), $"Terminated pre-existing active ending musical track under session token: {_endingMusicSessionId}.", _plugin.Config.Debug);
                _endingMusicSessionId = 0;
            }

            if (_audioManager is null)
            {
                Logger.Warn(nameof(OmegaAudioManager), "Audio dispatch canceled: Native IAudioManager endpoint context references null.");
                return 0;
            }

            // Extract the standardized string identifier directly from the internal config map
            string audioKey = _audioConfig[OmegaAudioKey.EndingMusic].key;

            try
            {
                // Architectural Upgrade: Execute via the generic pipeline using a cached static filter delegate
                _endingMusicSessionId = _audioManager.PlayGlobalAudio<object>(
                    key: audioKey,
                    state: null,
                    validPlayersFilter: (player, _) => player != null && player.IsReady,
                    loop: false,
                    volume: 0.9f,
                    priority: AudioPriority.High,
                    queue: false,
                    fadeInDuration: 2f,
                    persistent: false,
                    lifespan: lifespan,
                    autoCleanup: true
                );
            }
            catch (Exception ex)
            {
                Logger.Warn(nameof(OmegaAudioManager), $"Exception captured inside native ending music engine binding sequence: {ex.Message}");
                return 0;
            }

            if (_endingMusicSessionId == 0)
            {
                Logger.Warn(nameof(OmegaAudioManager), "Audio engine allocation rejected: Playback engine returns empty structural tracking token.");
                return 0;
            }

            Logger.Info(nameof(OmegaAudioManager), $"Cinematic post-apocalyptic ending theme engaged successfully. Active Session ID: {_endingMusicSessionId}");
            return _endingMusicSessionId;
        }
        #endregion

        #region Defensive Resource Purge
        /// <summary>
        /// Systematically cuts active playback contexts and resets internal tracker values.
        /// </summary>
        public void Cleanup()
        {
            StopOmegaSiren();

            if (_endingMusicSessionId != 0)
            {
                _audioManager.FadeOutAudio(_endingMusicSessionId, 2f);
                Logger.Debug(nameof(OmegaAudioManager), $"Structural session teardown complete for ending music track. Evicting token reference: {_endingMusicSessionId}", _plugin.Config.Debug);
                _endingMusicSessionId = 0;
            }
        }
        #endregion
    }
}