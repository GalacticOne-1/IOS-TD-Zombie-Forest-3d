using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio.Grenades
{
    /// <summary>
    /// Runtime-safe immutable audio data for a grenade. Same role as
    /// WeaponAudioData — converted once from GrenadeAudioDefinition (SO),
    /// then travels through events without any ScriptableObject reference.
    /// </summary>
    public sealed class GrenadeAudioData
    {
        public readonly AudioClip[] ThrowClips;
        public readonly float ThrowPitchMin;
        public readonly float ThrowPitchMax;
        public readonly float ThrowVolume;
        public readonly int ThrowPriority;

        public readonly AudioClip[] ExplosionClips;
        public readonly float ExplosionPitchMin;
        public readonly float ExplosionPitchMax;
        public readonly float ExplosionVolume;
        public readonly int ExplosionPriority;

        public GrenadeAudioData(
            AudioClip[] throwClips,
            float throwPitchMin,
            float throwPitchMax,
            float throwVolume,
            int throwPriority,
            AudioClip[] explosionClips,
            float explosionPitchMin,
            float explosionPitchMax,
            float explosionVolume,
            int explosionPriority)
        {
            ThrowClips = throwClips;
            ThrowPitchMin = throwPitchMin;
            ThrowPitchMax = throwPitchMax;
            ThrowVolume = throwVolume;
            ThrowPriority = throwPriority;

            ExplosionClips = explosionClips;
            ExplosionPitchMin = explosionPitchMin;
            ExplosionPitchMax = explosionPitchMax;
            ExplosionVolume = explosionVolume;
            ExplosionPriority = explosionPriority;
        }
    }
}