using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio.Weapons
{
    public enum WeaponAudioEventType
    {
        Fire = 0,

        ReloadStart = 10,
        ReloadComplete = 11,

        Empty = 20,

        Overheat = 30,

        Broken = 40
    }

    [CreateAssetMenu(
        fileName = "WeaponAudio_",
        menuName = "Game Configs/Audio/Weapon Audio Config")]
    public sealed class WeaponAudioConfig : ScriptableObject
    {
        [Header("Fire")]
        public AudioCue fire = new();

        [Header("Reload")]
        public AudioCue reloadStart = new();
        public AudioCue reloadComplete = new();

        [Header("Dry Fire")]
        public AudioCue empty = new();

        [Header("Overheat")]
        public AudioCue overheat = new();

        [Header("Broken")]
        public AudioCue broken = new();

        [Header("Audio Priority")]
        [Range(0, 100)]
        public int priority = 50;

        private WeaponAudioData _cached;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _cached = null;
        }
#endif

        public WeaponAudioData ToData()
        {
            if (_cached != null)
                return _cached;

            _cached = new WeaponAudioData(
                CreateCueData(fire),
                CreateCueData(reloadStart),
                CreateCueData(reloadComplete),
                CreateCueData(empty),
                CreateCueData(overheat),
                CreateCueData(broken),
                priority);

            return _cached;
        }

        private static AudioCueData CreateCueData(
            AudioCue cue)
        {
            if (cue == null)
                return null;

            float pitchMin = cue.pitchMin;
            float pitchMax = cue.pitchMax;

            if (pitchMin > pitchMax)
            {
                (pitchMin, pitchMax) =
                    (pitchMax, pitchMin);
            }

            return new AudioCueData(
                cue.clips,
                cue.volume,
                pitchMin,
                pitchMax);
        }
    }
}