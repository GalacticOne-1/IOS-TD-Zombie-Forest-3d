using UnityEngine;

namespace Galactic1.Systems
{
    [CreateAssetMenu(
        fileName = "AudioSettings",
        menuName = "Game Configs/Audio/Audio Settings")]
    public class AudioSettings : ScriptableObject
    {
        [Header("Default Mixer Levels")]
        [Tooltip("Default Music level in AudioMixer, in decibels.")]
        [SerializeField] private float musicVolumeDb = -30f;

        [Tooltip("Default SFX level in AudioMixer, in decibels.")]
        [SerializeField] private float sfxVolumeDb = -3f;

        public float MusicVolumeDb => musicVolumeDb;
        public float SFXVolumeDb => sfxVolumeDb;
    }
}