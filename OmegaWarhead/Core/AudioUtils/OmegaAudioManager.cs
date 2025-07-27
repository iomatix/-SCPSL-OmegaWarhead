namespace OmegaWarhead.Core.Audio
{

    using AudioManagerAPI.Defaults;
    using AudioManagerAPI.Features.Enums;
    using AudioManagerAPI.Features.Management;
    using AudioManagerAPI.Features.Static;
    using OmegaWarhead.Core.AudioUtils;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Log = LabApi.Features.Console.Logger;

    public class OmegaAudioManager
    {
        private readonly Plugin _plugin;
        private static IAudioManager sharedAudioManager;
        private byte _sirenControllerId;
        private byte _endingMusicControllerId;

        private readonly Dictionary<OmegaWarheadAudio, (string key, string resourceName)> audioConfig = new Dictionary<OmegaWarheadAudio, (string key, string resourceName)>()
        {
            { OmegaWarheadAudio.Siren, ("siren", "OmegaWarhead.Shared.Audio.Files.omegawarhead.siren.wav") },
            { OmegaWarheadAudio.EndingMusic, ("endingmusic", "OmegaWarhead.Shared.Audio.Files.omegawarhead.endingmusic.wav") }
        };

        public OmegaAudioManager(Plugin plugin)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin), "Plugin instance cannot be null.");
            if (DefaultAudioManager.Instance == null)
            {
                Log.Debug("[OmegaAudioManager] Initializing DefaultAudioManager.Instance.");
                DefaultAudioManager.RegisterDefaults(cacheSize: 120);
                sharedAudioManager = DefaultAudioManager.Instance;
                Log.Debug($"[OmegaAudioManager] sharedAudioManager initialized: {sharedAudioManager}");
                RegisterAudioResources();
            }
            else
            {
                Log.Debug("[OmegaAudioManager] DefaultAudioManager.Instance already initialized, using existing instance.");
                sharedAudioManager = DefaultAudioManager.Instance;
                RegisterAudioResources();
            }
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

        public byte PlayOmegaSiren()
        {
            if (_sirenControllerId != 0)
            {
                sharedAudioManager.FadeOutAudio(_sirenControllerId, 2f);
                StaticSpeakerFactory.RemoveSpeaker(_sirenControllerId);
                Log.Debug($"[OmegaAudioManager] Stopped existing siren with controller ID {_sirenControllerId}.");
            }

            try
            {
                _sirenControllerId = sharedAudioManager.PlayGlobalAudioWithFilter(
                    key: OmegaWarheadAudio.Siren.GetAudioKey(),
                    loop: true,
                    volume: 0.8f,
                    priority: AudioPriority.High,
                    configureSpeaker: null,
                    queue: false,
                    fadeInDuration: 1.5f,
                    persistent: true,
                    lifespan: 0f,
                    autoCleanup: false
                );
            }
            catch (Exception ex)
            {
                Log.Warn($"[OmegaAudioManager] Failed to play Omega siren audio: {ex.Message}");
                return 0;
            }

            if (_sirenControllerId == 0)
            {
                Log.Warn("[OmegaAudioManager] Failed to play Omega siren audio (invalid controller ID).");
                return 0;
            }

            Log.Info($"[OmegaAudioManager] Playing looping Omega siren with controller ID {_sirenControllerId}.");
            return _sirenControllerId;
        }

        public void StopOmegaSiren()
        {
            if (_sirenControllerId != 0)
            {
                sharedAudioManager.FadeOutAudio(_sirenControllerId, 2f);
                StaticSpeakerFactory.RemoveSpeaker(_sirenControllerId);
                Log.Info($"[OmegaAudioManager] Stopped Omega siren with controller ID {_sirenControllerId}.");
                _sirenControllerId = 0;
            }
        }

        public byte PlayEndingMusic(float lifespan = 109f)
        {
            if (_endingMusicControllerId != 0)
            {
                sharedAudioManager.FadeOutAudio(_endingMusicControllerId, 2f);
                StaticSpeakerFactory.RemoveSpeaker(_endingMusicControllerId);
                Log.Debug($"[OmegaAudioManager] Stopped existing ending music with controller ID {_endingMusicControllerId}.");
            }

            try
            {
                _endingMusicControllerId = sharedAudioManager.PlayGlobalAudioWithFilter(
                    key: OmegaWarheadAudio.EndingMusic.GetAudioKey(),
                    loop: false,
                    volume: 0.9f,
                    priority: AudioPriority.High,
                    configureSpeaker: null,
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

            if (_endingMusicControllerId == 0)
            {
                Log.Warn("[OmegaAudioManager] Failed to play Omega ending music (invalid controller ID).");
                return 0;
            }

            Log.Info($"[OmegaAudioManager] Playing Omega ending music with controller ID {_endingMusicControllerId}.");
            return _endingMusicControllerId;
        }

        public void Cleanup()
        {
            StopOmegaSiren();
            if (_endingMusicControllerId != 0)
            {
                sharedAudioManager.FadeOutAudio(_endingMusicControllerId, 2f);
                StaticSpeakerFactory.RemoveSpeaker(_endingMusicControllerId);
                Log.Debug($"[OmegaAudioManager] Cleaned up ending music with controller ID {_endingMusicControllerId}.");
                _endingMusicControllerId = 0;
            }
            sharedAudioManager.CleanupAllSpeakers();
            Log.Debug("[OmegaAudioManager] Cleaned up all audio speakers.");
        }
    }
}