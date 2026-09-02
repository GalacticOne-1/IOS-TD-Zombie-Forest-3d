using Galactic1.Code.Gameplay.Audio.Grenades;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Systems;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio.Grenades
{
    /// <summary>
    /// Pure presentation system for grenade audio. Mirrors
    /// WeaponGunshotAudioSystem: subscribe on construction, unsubscribe on
    /// Dispose(), no gameplay logic, no voice-budget system (none exists
    /// in this project yet).
    /// </summary>
    public sealed class GrenadeAudioPlaybackSystem
    {
        private readonly EventBinding<AudioGrenadeThrowEvent> _throwBinding;
        private readonly EventBinding<AudioGrenadeExplosionEvent> _explosionBinding;

        public GrenadeAudioPlaybackSystem()
        {
            _throwBinding = new EventBinding<AudioGrenadeThrowEvent>(OnThrow);
            _explosionBinding = new EventBinding<AudioGrenadeExplosionEvent>(OnExplosion);

            EventBus<AudioGrenadeThrowEvent>.Register(_throwBinding);
            EventBus<AudioGrenadeExplosionEvent>.Register(_explosionBinding);
        }

        public void Dispose()
        {
            EventBus<AudioGrenadeThrowEvent>.Deregister(_throwBinding);
            EventBus<AudioGrenadeExplosionEvent>.Deregister(_explosionBinding);
        }

        private void OnThrow(AudioGrenadeThrowEvent e)
        {
            var audio = e.Audio;
            if (audio == null) return;

            AudioClip clip = PickClip(audio.ThrowClips);
            if (clip == null) return;

            float pitch = PickPitch(audio.ThrowPitchMin, audio.ThrowPitchMax);
            AudioService.PlaySFXAtPosition(clip, e.Position, audio.ThrowVolume, pitch);
        }

        private void OnExplosion(AudioGrenadeExplosionEvent e)
        {
            var audio = e.Audio;
            if (audio == null) return;

            AudioClip clip = PickClip(audio.ExplosionClips);
            if (clip == null) return;

            float pitch = PickPitch(audio.ExplosionPitchMin, audio.ExplosionPitchMax);
            AudioService.PlaySFXAtPosition(clip, e.Position, audio.ExplosionVolume, pitch);
        }

        private static AudioClip PickClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
                return null;

            return clips.Length == 1
                ? clips[0]
                : clips[Random.Range(0, clips.Length)];
        }

        private static float PickPitch(float min, float max)
        {
            return Mathf.Approximately(min, max) ? min : Random.Range(min, max);
        }
    }
}