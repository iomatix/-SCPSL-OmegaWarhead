namespace OmegaWarhead.Core.Audio
{
    using AudioManagerAPI.Defaults;
    using AudioManagerAPI.Features.Enums;
    using AudioManagerAPI.Features.Management;
    using OmegaWarhead.Core.AudioUtils;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Log = LabApi.Features.Console.Logger;

    public class OmegaAudioManager
    {
        private readonly Plugin _plugin;
        private static IAudioManager sharedAudioManager;

        // byte -> int (Session ID - API 2.0 change)
        private int _sirenSessionId;
        private int _endingMusicSessionId;

        private readonly Dictionary<OmegaWarheadAudio, (string key, string resourceName)> audioConfig = new Dictionary<OmegaWarheadAudio, (string key, string resourceName)>()
        {
            { OmegaWarheadAudio.Siren, ("siren", "OmegaWarhead.Shared.Audio.Files.omegawarhead.siren.wav") },
            { OmegaWarheadAudio.EndingMusic, ("endingmusic", "OmegaWarhead.Shared.Audio.Files.omegawarhead.endingmusic.wav") }
        };

        public OmegaAudioManager(Plugin plugin)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin), "Plugin instance cannot be null.");

            sharedAudioManager = DefaultAudioManager.Instance;
            RegisterAudioResources();
        }

        private void RegisterAudioResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var pair in audioConfig)
            {
                string resourceName = pair.Value.resourceName;
                var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null || stream.Length == 0)
                {
                    Log.Error($"[OmegaAudioManager][RegisterAudioResources] Failed to load audio resource: {resourceName}. Stream is null or empty.");
                }
                else
                {
                    Log.Debug($"[OmegaAudioManager][RegisterAudioResources] Loaded audio resource: {resourceName}, size: {stream.Length} bytes");
                }

                sharedAudioManager.RegisterAudio(pair.Value.key, () => assembly.GetManifestResourceStream(resourceName));
                Log.Debug($"[OmegaAudioManager][RegisterAudioResources] Registered audio key: {pair.Value.key}");
            }
        }

        public int PlayOmegaSiren()
        {
            if (_sirenSessionId != 0)
            {
                // API will clean up the session after FadeOut.
                sharedAudioManager.FadeOutAudio(_sirenSessionId, 2f);
                Log.Debug($"[OmegaAudioManager] Stopped existing siren with session ID {_sirenSessionId}.");
                _sirenSessionId = 0;
            }

            if (sharedAudioManager == null)
            {
                Log.Warn("[OmegaAudioManager] sharedAudioManager is null. Cannot play siren.");
                return 0;
            }

            var audioKey = OmegaWarheadAudio.Siren.GetAudioKey();
            if (string.IsNullOrEmpty(audioKey))
            {
                Log.Warn("[OmegaAudioManager] Audio key for siren is null or empty.");
                return 0;
            }

            try
            {
                _sirenSessionId = sharedAudioManager.PlayGlobalAudio(
                    key: audioKey,
                    loop: true,
                    volume: 0.8f,
                    priority: AudioPriority.High,
                    validPlayersFilter: null, // null means all valid players
                    queue: false,
                    fadeInDuration: 1.5f,
                    persistent: true,
                    lifespan: null, // null means it will play indefinitely until stopped
                    autoCleanup: false
                );
            }
            catch (Exception ex)
            {
                Log.Warn($"[OmegaAudioManager] Failed to play Omega siren audio: {ex.Message}");
                return 0;
            }

            if (_sirenSessionId == 0)
            {
                Log.Warn("[OmegaAudioManager] Failed to play Omega siren audio (invalid session ID).");
                return 0;
            }

            Log.Info($"[OmegaAudioManager] Playing looping Omega siren with session ID {_sirenSessionId}.");
            return _sirenSessionId;
        }

        public void StopOmegaSiren()
        {
            if (_sirenSessionId != 0)
            {
                // Direct use of fade out with cleanup (without invoking physical factory)
                sharedAudioManager.FadeOutAudio(_sirenSessionId, 2f);
                Log.Info($"[OmegaAudioManager] Fading out and stopping Omega siren with session ID {_sirenSessionId}.");
                _sirenSessionId = 0;
            }
        }

        public int PlayEndingMusic(float lifespan = 109f)
        {
            if (_endingMusicSessionId != 0)
            {
                sharedAudioManager.FadeOutAudio(_endingMusicSessionId, 2f);
                Log.Debug($"[OmegaAudioManager] Stopped existing ending music with session ID {_endingMusicSessionId}.");
                _endingMusicSessionId = 0;
            }

            if (sharedAudioManager == null)
            {
                Log.Warn("[OmegaAudioManager] sharedAudioManager is null. Cannot play ending music.");
                return 0;
            }

            var audioKey = OmegaWarheadAudio.EndingMusic.GetAudioKey();
            if (string.IsNullOrEmpty(audioKey))
            {
                Log.Warn("[OmegaAudioManager] Audio key for Ending Music is null or empty.");
                return 0;
            }

            try
            {
                _endingMusicSessionId = sharedAudioManager.PlayGlobalAudio(
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
                Log.Warn($"[OmegaAudioManager] Failed to play Omega ending music: {ex.Message}");
                return 0;
            }

            if (_endingMusicSessionId == 0)
            {
                Log.Warn("[OmegaAudioManager] Failed to play Omega ending music (invalid session ID).");
                return 0;
            }

            Log.Info($"[OmegaAudioManager] Playing Omega ending music with session ID {_endingMusicSessionId}.");
            return _endingMusicSessionId;
        }

        public void Cleanup()
        {
            StopOmegaSiren();
            if (_endingMusicSessionId != 0)
            {
                // For instant cleanup, you could use DestroySession if you don't want to wait for the 2-second fade out:
                // sharedAudioManager.DestroySession(_endingMusicSessionId);
                // Leaving FadeOut for a smooth cut-off at the end of the round.
                sharedAudioManager.FadeOutAudio(_endingMusicSessionId, 2f);
                Log.Debug($"[OmegaAudioManager] Cleaned up ending music with session ID {_endingMusicSessionId}.");
                _endingMusicSessionId = 0;
            }
        }
    }
}