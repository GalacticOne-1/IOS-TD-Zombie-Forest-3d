using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio
{
    [CreateAssetMenu(
        fileName = "AudioCue_",
        menuName = "Game Configs/Audio/Simple Audio Cue")]
    public sealed class SimpleAudioConfig : 
        ScriptableObject, 
        IUIStyleConfig,
        IUIAudioConfig
    {
        [field: SerializeField] public string ConfigId { get; private set; }

        public string Id
        {
            get => ConfigId;
            set => ConfigId = value;
        }
        
        
        [SerializeField] private AudioClip clip;

        [Range(0f, 1f)] [SerializeField] private float volume = 1f;

        [Range(0.8f, 1.2f)] [SerializeField] private float pitchMin = 1f;

        [Range(0.8f, 1.2f)] [SerializeField] private float pitchMax = 1f;

        [Range(0, 100)] [SerializeField] private int priority = 50;


        public AudioCueData ToData()
        {
            float min = pitchMin;
            float max = pitchMax;

            if (min > max)
                (min, max) = (max, min);

            return new AudioCueData(
                new[] { clip },
                volume,
                min,
                max);
        }
    }
}