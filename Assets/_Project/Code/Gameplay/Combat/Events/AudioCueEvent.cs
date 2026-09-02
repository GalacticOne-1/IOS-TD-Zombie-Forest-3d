using Galactic1.Code.Gameplay.Audio;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    public readonly struct AudioCueEvent : IEvent
    {
        public readonly Vector3 Position;
        public readonly SimpleAudioData Data;

        public AudioCueEvent(
            Vector3 position,
            SimpleAudioData data)
        {
            Position = position;
            Data = data;
        }
    }
}