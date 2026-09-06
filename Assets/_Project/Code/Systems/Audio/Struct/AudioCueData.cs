using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio
{
    /// <summary>
    /// Runtime-safe audio data for one weapon audio cue.
    ///
    /// Contains no Unity authoring configuration logic.
    /// Created from WeaponAudioCue during WeaponAudioConfig.ToData().
    /// </summary>
    public sealed class AudioCueData
    {
        public readonly AudioClip[] Clips;

        public readonly float Volume;
        public readonly float PitchMin;
        public readonly float PitchMax;

        public AudioCueData(
            AudioClip[] clips,
            float volume,
            float pitchMin,
            float pitchMax)
        {
            Clips = clips;
            Volume = volume;
            PitchMin = pitchMin;
            PitchMax = pitchMax;
        }

        public bool HasClips =>
            Clips != null && Clips.Length > 0;
    }
}