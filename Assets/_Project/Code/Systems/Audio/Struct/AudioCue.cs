using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio.Weapons
{
    [System.Serializable]
    public sealed class AudioCue
    {
        [Tooltip("One or more clips. A random clip is selected at playback time.")]
        public AudioClip[] clips;

        [Range(0f, 1f)]
        public float volume = 1f;

        [Range(0.8f, 1.2f)]
        public float pitchMin = 0.95f;

        [Range(0.8f, 1.2f)]
        public float pitchMax = 1.05f;

        public bool HasClips =>
            clips != null && clips.Length > 0;
        
        
        
        public AudioCueData ToData()
        {
            if (!HasClips)
                return null;

            float min = pitchMin;
            float max = pitchMax;

            if (min > max)
                (min, max) = (max, min);

            return new AudioCueData(
                clips,
                volume,
                min,
                max);
        }
    }
    
}