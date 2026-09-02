using Galactic1.Code.Gameplay.Audio.Grenades;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Audio-only "grenade exploded" notification. Raised once, from
    /// GrenadeProjectile.Explode(), before AoE/VFX dispatch. Never raised
    /// if the grenade has no authored GrenadeAudioDefinition.
    /// </summary>
    public readonly struct AudioGrenadeExplosionEvent : IEvent
    {
        public readonly Vector3 Position;
        public readonly GrenadeAudioData Audio;

        public AudioGrenadeExplosionEvent(Vector3 position, GrenadeAudioData audio)
        {
            Position = position;
            Audio = audio;
        }
    }
}