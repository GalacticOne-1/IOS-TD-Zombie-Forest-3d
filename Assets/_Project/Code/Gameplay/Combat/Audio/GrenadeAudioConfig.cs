using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio.Grenades
{
    [CreateAssetMenu(
        fileName = "GrenadeAudio_",
        menuName = "Game Configs/Audio/Grenade Audio Config")]
    public sealed class GrenadeAudioConfig : ScriptableObject
    {
        [Header("Throw")] public AudioClip[] throwClips;

        [Range(0f, 1f)] public float throwVolume = 0.8f;

        [Range(0.8f, 1.2f)] public float throwPitchMin = 0.95f;

        [Range(0.8f, 1.2f)] public float throwPitchMax = 1.05f;

        [Header("Explosion")] public AudioClip[] explosionClips;

        [Range(0f, 1f)] public float explosionVolume = 1f;

        [Range(0.8f, 1.2f)] public float explosionPitchMin = 0.97f;

        [Range(0.8f, 1.2f)] public float explosionPitchMax = 1.03f;

        [Header("Audio Priority")] [Tooltip("Взрыв обычно важнее звука броска — приоритеты разделены.")] [Range(0, 100)]
        public int throwPriority = 40;

        [Range(0, 100)] public int explosionPriority = 90;

        // Кэш — тот же паттерн, что у WeaponAudioDefinition.
        private GrenadeAudioData _cached;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _cached = null;
        }
#endif

        public GrenadeAudioData ToData()
        {
            if (_cached != null)
                return _cached;

            float tMin = Mathf.Min(throwPitchMin, throwPitchMax);
            float tMax = Mathf.Max(throwPitchMin, throwPitchMax);

            float eMin = Mathf.Min(explosionPitchMin, explosionPitchMax);
            float eMax = Mathf.Max(explosionPitchMin, explosionPitchMax);

            _cached = new GrenadeAudioData(
                throwClips, tMin, tMax, throwVolume, throwPriority,
                explosionClips, eMin, eMax, explosionVolume, explosionPriority);

            return _cached;
        }

    }
}