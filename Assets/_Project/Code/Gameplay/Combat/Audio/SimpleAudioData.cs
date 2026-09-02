using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio
{
    /// <summary>
    /// Runtime-данные простого одноразового звука.
    ///
    /// Не является ScriptableObject.
    /// Не содержит gameplay-логики.
    /// Используется для передачи audio-параметров
    /// из authoring layer в runtime/event layer.
    /// </summary>
    public sealed class SimpleAudioData
    {
        public AudioClip Clip { get; }
        public float Volume { get; }
        public float PitchMin { get; }
        public float PitchMax { get; }
        public int Priority { get; }

        public SimpleAudioData(
            AudioClip clip,
            float volume,
            float pitchMin,
            float pitchMax,
            int priority)
        {
            Clip = clip;
            Volume = volume;
            PitchMin = pitchMin;
            PitchMax = pitchMax;
            Priority = priority;
        }
    }
}