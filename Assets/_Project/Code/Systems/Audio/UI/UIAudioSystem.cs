using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Systems;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio
{
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
            AudioCueData data = e.Data;

            if (data == null || !data.HasClips)
                return;

            AudioClip clip = PickClip(data.Clips);

            if (clip == null)
                return;

            float pitch = PickPitch(
                data.PitchMin,
                data.PitchMax);

            AudioService.PlaySFX(
                clip,
                data.Volume,
                pitch);
        }

        private static AudioClip PickClip(AudioClip[] clips)
        {
            return clips.Length == 1
                ? clips[0]
                : clips[Random.Range(0, clips.Length)];
        }

        private static float PickPitch(float min, float max)
        {
            if (Mathf.Approximately(min, max))
                return min;

            return Random.Range(min, max);
        }
    }
}