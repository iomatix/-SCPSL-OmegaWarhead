namespace OmegaWarhead.Core.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AudioManagerAPI.Defaults;
    using AudioManagerAPI.Features.Enums;
    using AudioManagerAPI.Features.Management;
    using OmegaWarhead.Core.AudioUtils;
    using OmegaWarhead.Core.LoggingUtils;

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

        // Simplified configuration matrix mapping utilizing C# 9.0 target-typed expressions
        private readonly Dictionary<OmegaWarheadAudio, (string key, string resourceName)> _audioConfig = new()
        {
            { OmegaWarheadAudio.Siren, ("siren", "OmegaWarhead.Shared.Audio.Files.omegawarhead.siren.wav") },
            { OmegaWarheadAudio.EndingMusic, ("endingmusic", "OmegaWarhead.Shared.Audio.Files.omegawarhead.endingmusic.wav") }
        };
        #endregion

        #region Initialization & Resource Registration
        /// <summary>
        /// Initializes a new instance of the <see cref="OmegaAudioManager"/> class bound to the plugin lifecycle context.
        /// </summary>
        public OmegaAudioManager(Plugin plugin)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin), "Structural runtime allocation failure: Parent plugin context evaluates to null.");

            // Established pure instance-bound allocation to guarantee absolute isolation from global static states
            _audioManager = DefaultAudioManager.Instance;

            RegisterAudioResources();
        }

        /// <summary>
        /// Scans the compiled assembly layer, validates embedded wave streams, and registers dynamic content delivery blocks.
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
                    if (testStream == null)
                    {
                        LogHelper.Error(nameof(OmegaAudioManager), $"CRITICAL ASSET FAREWELL: Manifest embedded path '{resourcePath}' could not be resolved. Verify compiler build action flags (Must be 'Embedded Resource').");
                        continue;
                    }

                    if (testStream.Length == 0)
                    {
                        LogHelper.Error(nameof(OmegaAudioManager), $"CRITICAL ASSET CORRUPTION: Manifest path '{resourcePath}' evaluates to a 0-byte hollow allocation block.");
                        continue;
                    }

                    LogHelper.Debug(nameof(OmegaAudioManager), $"Asset tracking footprint verified successfully: {resourcePath} ({testStream.Length} bytes).");
                }

                // Inject lazy-loaded audio factory stream provider directly into the AudioManagerAPI backend
                _audioManager.RegisterAudio(key, () =>
                {
                    var stream = assembly.GetManifestResourceStream(resourcePath);
                    if (stream == null)
                    {
                        LogHelper.Error(nameof(OmegaAudioManager), $"Lazy-load extraction pipeline broken for asset key '{key}' under path coordinates: {resourcePath}");
                    }
                    return stream;
                });

                LogHelper.Info(nameof(OmegaAudioManager), $"Audio pipeline provider successfully registered for key index: [{key}]");
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
                LogHelper.Debug(nameof(OmegaAudioManager), $"Terminated pre-existing active siren context under session token: {_sirenSessionId}.");
                _sirenSessionId = 0;
            }

            if (_audioManager == null)
            {
                LogHelper.Warn(nameof(OmegaAudioManager), "Audio dispatch canceled: Native IAudioManager endpoint context references null.");
                return 0;
            }

            var audioKey = OmegaWarheadAudio.Siren.GetAudioKey();
            if (string.IsNullOrEmpty(audioKey))
            {
                LogHelper.Warn(nameof(OmegaAudioManager), "Audio dispatch canceled: Resolved enum asset key dictionary mapping returns empty.");
                return 0;
            }

            try
            {
                _sirenSessionId = _audioManager.PlayGlobalAudio(
                    key: audioKey,
                    loop: true,
                    volume: 0.8f,
                    priority: AudioPriority.High,
                    validPlayersFilter: null, // Broadcast structural waves globally to all connected endpoints
                    queue: false,
                    fadeInDuration: 1.5f,
                    persistent: true,
                    lifespan: null,          // Persistent continuous looping execution block until explicit manual termination
                    autoCleanup: false
                );
            }
            catch (Exception ex)
            {
                LogHelper.Warn(nameof(OmegaAudioManager), $"Exception captured inside native audio engine framework binding: {ex.Message}");
                return 0;
            }

            if (_sirenSessionId == 0)
            {
                LogHelper.Warn(nameof(OmegaAudioManager), "Audio engine allocation rejected: Playback engine returns empty tracking token.");
                return 0;
            }

            LogHelper.Info(nameof(OmegaAudioManager), $"Looping facility alternative evacuation siren engaged. Active Session ID: {_sirenSessionId}");
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
                LogHelper.Info(nameof(OmegaAudioManager), $"Fading out alternative evacuation siren system layer. Retiring Session ID: {_sirenSessionId}");
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
                LogHelper.Debug(nameof(OmegaAudioManager), $"Terminated pre-existing active ending musical track under session token: {_endingMusicSessionId}.");
                _endingMusicSessionId = 0;
            }

            if (_audioManager == null)
            {
                LogHelper.Warn(nameof(OmegaAudioManager), "Audio dispatch canceled: Native IAudioManager endpoint context references null.");
                return 0;
            }

            var audioKey = OmegaWarheadAudio.EndingMusic.GetAudioKey();
            if (string.IsNullOrEmpty(audioKey))
            {
                LogHelper.Warn(nameof(OmegaAudioManager), "Audio dispatch canceled: Resolved enum asset key dictionary mapping returns empty.");
                return 0;
            }

            try
            {
                _endingMusicSessionId = _audioManager.PlayGlobalAudio(
                    key: audioKey,
                    loop: false,
                    volume: 0.9f,
                    priority: AudioPriority.High,
                    validPlayersFilter: null,
                    queue: false,
                    fadeInDuration: 2f,
                    persistent: false,
                    lifespan: lifespan,
                    autoCleanup: true
                );
            }
            catch (Exception ex)
            {
                LogHelper.Warn(nameof(OmegaAudioManager), $"Exception captured inside native ending music engine binding: {ex.Message}");
                return 0;
            }

            if (_endingMusicSessionId == 0)
            {
                LogHelper.Warn(nameof(OmegaAudioManager), "Audio engine allocation rejected: Playback engine returns empty tracking token.");
                return 0;
            }

            LogHelper.Info(nameof(OmegaAudioManager), $"Cinematic post-apocalyptic ending theme engaged successfully. Active Session ID: {_endingMusicSessionId}");
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
                // Smooth linear attenuation decay sweep executed at round boundaries
                _audioManager.FadeOutAudio(_endingMusicSessionId, 2f);
                LogHelper.Debug(nameof(OmegaAudioManager), $"Structural session teardown complete for ending music track. Evicting token: {_endingMusicSessionId}");
                _endingMusicSessionId = 0;
            }
        }
        #endregion
    }
}