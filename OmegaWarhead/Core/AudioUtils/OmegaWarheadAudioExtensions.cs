namespace OmegaWarhead.Core.AudioUtils
{
    using System;

    /// <summary>
    /// Enum representing registered audio keys for the OmegaWarhead plugin.
    /// </summary>
    public enum OmegaWarheadAudio
    {
        EndingMusic,
        Siren
    }

    /// <summary>
    /// Extension methods for OmegaWarheadAudio enum.
    /// </summary>
    public static class OmegaWarheadAudioExtensions
    {
        /// <summary>
        /// Gets the corresponding string key used in AudioManager for a given enum value.
        /// </summary>
        public static string GetAudioKey(this OmegaWarheadAudio audio)
        {
            switch (audio)
            {
                case OmegaWarheadAudio.EndingMusic:
                    return "endingmusic";
                case OmegaWarheadAudio.Siren:
                    return "siren";
                default:
                    throw new ArgumentOutOfRangeException(nameof(audio), audio, "Unknown audio key.");
            }
        }
    }
}
