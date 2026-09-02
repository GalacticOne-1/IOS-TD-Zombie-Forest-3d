using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio
{
    [CreateAssetMenu(
        fileName = "AudioCue_",
        menuName = "Game Configs/Audio/Simple Audio Cue")]
    public sealed class SimpleAudioDefinition : ScriptableObject
    {
        [SerializeField] private AudioClip clip;

        [Range(0f, 1f)] [SerializeField] private float volume = 1f;

        [Range(0.8f, 1.2f)] [SerializeField] private float pitchMin = 1f;

        [Range(0.8f, 1.2f)] [SerializeField] private float pitchMax = 1f;

        [Range(0, 100)] [SerializeField] private int priority = 50;

        public AudioClip Clip => clip;
        public float Volume => volume;
        public float PitchMin => pitchMin;
        public float PitchMax => pitchMax;
        public int Priority => priority;

        public SimpleAudioData ToData()
        {
            float min = pitchMin;
            float max = pitchMax;

            if (min > max)
                (min, max) = (max, min);

            return new SimpleAudioData(
                clip,
                volume,
                min,
                max,
                priority);
        }
    }
}