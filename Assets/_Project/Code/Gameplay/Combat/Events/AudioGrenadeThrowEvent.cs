using Galactic1.Code.Gameplay.Audio.Grenades;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    public readonly struct AudioGrenadeThrowEvent : IEvent
    {
        public readonly Vector3 Position;
        public readonly GrenadeAudioData Audio;

        public AudioGrenadeThrowEvent(Vector3 position, GrenadeAudioData audio)
        {
            Position = position;
            Audio = audio;
        }
    }
}