using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Systems;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio
{
    /// <summary>
    /// Presentation-only system for simple one-shot audio cues.
    ///
    /// Converts AudioCueEvent into actual audio playback.
    /// Does not contain gameplay logic.
    /// Does not know whether the source was a medkit, bandage,
    /// grenade, door, pickup, etc.
    /// </summary>
    public sealed class AudioCueSystem
    {
        private readonly EventBinding<AudioCueEvent> _binding;

        public AudioCueSystem()
        {
            _binding = new EventBinding<AudioCueEvent>(OnAudioCue);
            EventBus<AudioCueEvent>.Register(_binding);
        }

        public void Dispose()
        {
            EventBus<AudioCueEvent>.Deregister(_binding);
        }

        private void OnAudioCue(AudioCueEvent e)
        {
            var data = e.Data;

            if (data == null)
                return;

            if (data.Clip == null)
                return;

            float pitch = SelectPitch(
                data.PitchMin,
                data.PitchMax);

            AudioService.PlaySFXAtPosition(
                data.Clip,
                e.Position,
                data.Volume,
                pitch);
        }

        private static float SelectPitch(float min, float max)
        {
            if (min > max)
                (min, max) = (max, min);

            if (Mathf.Approximately(min, max))
                return min;

            return Random.Range(min, max);
        }
    }
}