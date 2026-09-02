using Galactic1.Code.Gameplay.Audio.Weapons;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    public readonly struct WeaponAudioEvent : IEvent
    {
        public readonly Vector3 Position;
        public readonly WeaponAudioData Audio;
        public readonly WeaponAudioEventType Type;
        public readonly int Priority;

        public WeaponAudioEvent(
            Vector3 position,
            WeaponAudioData audio,
            WeaponAudioEventType type)
        {
            Position = position;
            Audio = audio;
            Type = type;
            Priority = audio != null ? audio.Priority : 0;
        }
    }
}