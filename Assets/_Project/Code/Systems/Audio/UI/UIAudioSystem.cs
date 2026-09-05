using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Systems;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio
{
    /// <summary>
    /// Для UI во всей игре
    /// </summary>
    public sealed class UIAudioSystem
    {
        private readonly EventBinding<AudioUIEvent> _binding;

        public UIAudioSystem()
        {
            _binding = new EventBinding<AudioUIEvent>(OnAudioCue);
            EventBus<AudioUIEvent>.Register(_binding);
        }

        public void Dispose()
        {
            EventBus<AudioUIEvent>.Deregister(_binding);
        }

        private void OnAudioCue(AudioUIEvent e)
        {
            var data = e.Data;

            if (data == null)
                return;

            if (data.Clip == null)
                return;

            float pitch = SelectPitch(
                data.PitchMin,
                data.PitchMax);

            AudioService.PlaySFX(
                data.Clip,
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