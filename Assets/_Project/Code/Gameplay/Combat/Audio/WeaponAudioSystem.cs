using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Systems;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Audio.Weapons
{
    /// <summary>
    /// Pure presentation system: converts AudioGunshotEvent into an actual
    /// playback call. Contains zero gameplay logic.
    ///
    /// Subscribes to EventBus&lt;AudioGunshotEvent&gt; on construction,
    /// unsubscribes in Dispose(). Same lifecycle pattern as CombatEventRouter —
    /// create alongside it at raid init (BuildCombatRuntime), dispose at raid end.
    ///
    /// CombatAudioPrioritySystem / WeaponTailSystem do not exist in this
    /// project yet, so no voice-budget capping is applied here. When one is
    /// introduced, it should filter/consume AudioGunshotEvent.Priority
    /// upstream of this system rather than this system growing a budget
    /// mechanism of its own.
    /// </summary>
    public sealed class WeaponAudioSystem
    {
        private readonly EventBinding<WeaponAudioEvent> _gunshotBinding;

        public WeaponAudioSystem()
        {
            _gunshotBinding = new EventBinding<WeaponAudioEvent>(OnWeaponAudio);
            
            EventBus<WeaponAudioEvent>.Register(_gunshotBinding);
        }

        public void Dispose()
        {
            EventBus<WeaponAudioEvent>.Deregister(_gunshotBinding);
        }

        private void OnWeaponAudio(WeaponAudioEvent e)
        {
            if (e.Audio == null)
                return;
            
            WeaponAudioCueData cue = GetCue(e.Audio, e.Type);

            if (cue == null || !cue.HasClips)
                return;

            AudioClip clip = PickClip(cue.Clips);

            if (clip == null)
                return;

            float pitch = PickPitch(
                cue.PitchMin,
                cue.PitchMax);

            AudioService.PlaySFXAtPosition(
                clip,
                e.Position,
                cue.Volume,
                pitch);
        }

        
        
        private static WeaponAudioCueData GetCue(
            WeaponAudioData audio,
            WeaponAudioEventType type)
        {
            return type switch
            {
                WeaponAudioEventType.Fire => audio.Fire,

                WeaponAudioEventType.ReloadStart => audio.ReloadStart,

                WeaponAudioEventType.ReloadComplete => audio.ReloadComplete,

                WeaponAudioEventType.Empty => audio.Empty,

                WeaponAudioEventType.Overheat => audio.Overheat,

                WeaponAudioEventType.Broken => audio.Broken,

                _ => null
            };
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
            // min/max are already normalized by WeaponAudioDefinition.ToData(),
            // but staying defensive here costs nothing.
            return Mathf.Approximately(min, max)
                ? min
                : Random.Range(min, max);
        }
    }
}