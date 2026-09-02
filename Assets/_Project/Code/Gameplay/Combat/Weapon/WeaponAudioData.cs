using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio.Weapons
{
    public sealed class WeaponAudioData
    {
        public readonly WeaponAudioCueData Fire;
        public readonly WeaponAudioCueData ReloadStart;
        public readonly WeaponAudioCueData ReloadComplete;
        public readonly WeaponAudioCueData Empty;
        public readonly WeaponAudioCueData Overheat;
        public readonly WeaponAudioCueData Broken;

        public readonly int Priority;

        public WeaponAudioData(
            WeaponAudioCueData fire,
            WeaponAudioCueData reloadStart,
            WeaponAudioCueData reloadComplete,
            WeaponAudioCueData empty,
            WeaponAudioCueData overheat,
            WeaponAudioCueData broken,
            int priority)
        {
            Fire = fire;
            ReloadStart = reloadStart;
            ReloadComplete = reloadComplete;
            Empty = empty;
            Overheat = overheat;
            Broken = broken;

            Priority = priority;
        }
    }
    
    /// <summary>
    /// Runtime-safe audio data for one weapon audio cue.
    ///
    /// Contains no Unity authoring configuration logic.
    /// Created from WeaponAudioCue during WeaponAudioConfig.ToData().
    /// </summary>
    public sealed class WeaponAudioCueData
    {
        public readonly AudioClip[] Clips;

        public readonly float Volume;
        public readonly float PitchMin;
        public readonly float PitchMax;

        public WeaponAudioCueData(
            AudioClip[] clips,
            float volume,
            float pitchMin,
            float pitchMax)
        {
            Clips = clips;
            Volume = volume;

            if (pitchMin > pitchMax)
            {
                (pitchMin, pitchMax) = (pitchMax, pitchMin);
            }

            PitchMin = pitchMin;
            PitchMax = pitchMax;
        }

        public bool HasClips =>
            Clips != null && Clips.Length > 0;
    }
}