
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Systems;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio
{
    /// <summary>
    /// Presentation-only system for unit voice/reaction audio.
    ///
    /// Responsibilities:
    /// - subscribe to AudioVoiceEvent;
    /// - select the correct voice clip by event type;
    /// - safely handle missing audio data/clips;
    /// - randomize pitch;
    /// - forward playback to the existing AudioService.
    ///
    /// Does NOT contain gameplay logic.
    /// Does NOT know which gameplay system caused the event.
    /// </summary>
    public sealed class VoiceAudioSystem
    {
        private readonly EventBinding<AudioVoiceEvent> _binding;

        public VoiceAudioSystem()
        {
            _binding = new EventBinding<AudioVoiceEvent>(OnVoiceRequested);

            EventBus<AudioVoiceEvent>.Register(_binding);
        }

        /// <summary>
        /// Removes the event subscription.
        /// Must be called when the raid/audio runtime is destroyed.
        /// </summary>
        public void Dispose()
        {
            EventBus<AudioVoiceEvent>.Deregister(_binding);
        }

        // =========================================================
        // EVENT
        // =========================================================

        private void OnVoiceRequested(AudioVoiceEvent e)
        {
            if (e.Data == null)
                return;

            AudioClip clip = SelectClip(e.Data, e.Type);

            if (clip == null)
                return;

            float pitch = SelectPitch(
                e.Data.PitchMin,
                e.Data.PitchMax);

            AudioService.PlaySFXAtPosition(
                clip,
                e.Position,
                e.Data.Volume,
                pitch);
        }

        // =========================================================
        // CLIP
        // =========================================================

        private static AudioClip SelectClip(
            VoiceAudioData data,
            VoiceEventType type)
        {
            AudioClip[] clips = type switch
            {
                VoiceEventType.Damage => data.DamageClips,
                VoiceEventType.Death => data.DeathClips,
                VoiceEventType.Aggro => data.AggroClips,
                _ => null
            };

            if (clips == null || clips.Length == 0)
                return null;

            if (clips.Length == 1)
                return clips[0];

            return clips[Random.Range(0, clips.Length)];
        }

        // =========================================================
        // PITCH
        // =========================================================

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