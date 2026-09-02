using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio.Voice
{
    [CreateAssetMenu(
        fileName = "VoiceAudio_",
        menuName = "Game Configs/Audio/Voice Audio Config")]
    public sealed class VoiceAudioConfig : ScriptableObject
    {
        [Header("Voice Clips")] [SerializeField]
        private AudioClip[] damageClips;

        [SerializeField] 
        private AudioClip[] deathClips;
        
        [SerializeField] 
        private AudioClip[] aggroClips;

        [Header("Playback")] [Range(0f, 1f)] [SerializeField]
        private float volume = 1f;

        [Range(0.8f, 1.2f)] [SerializeField] private float pitchMin = 0.95f;

        [Range(0.8f, 1.2f)] [SerializeField] private float pitchMax = 1.05f;




        public VoiceAudioData ToData()
        {
            float minPitch = pitchMin;
            float maxPitch = pitchMax;
            if (minPitch > maxPitch) 
                (minPitch, maxPitch) = (maxPitch, minPitch);
            
            return new VoiceAudioData(
                damageClips, 
                deathClips,
                aggroClips,
                volume,
                minPitch,
                maxPitch);
        }
    }
}