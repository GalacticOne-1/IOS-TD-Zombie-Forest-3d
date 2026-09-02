using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio
{
    public sealed class VoiceAudioData
    {
        public AudioClip[] DamageClips { get; }
        public AudioClip[] DeathClips { get; }
        public AudioClip[] AggroClips { get; }

        public float Volume { get; }
        public float PitchMin { get; }
        public float PitchMax { get; }

        public VoiceAudioData(
            AudioClip[] damageClips,
            AudioClip[] deathClips,
            AudioClip[] aggroClips,
            float volume,
            float pitchMin,
            float pitchMax)
        {
            DamageClips = damageClips;
            DeathClips = deathClips;
            AggroClips = aggroClips;
            Volume = volume;
            PitchMin = pitchMin;
            PitchMax = pitchMax;
        }
    }
}