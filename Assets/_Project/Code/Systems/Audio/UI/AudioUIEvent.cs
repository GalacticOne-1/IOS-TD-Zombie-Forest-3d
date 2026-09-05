using Galactic1.Code.Gameplay.Audio;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    public readonly struct AudioUIEvent : IEvent
    {
        public readonly SimpleAudioData Data;

        public AudioUIEvent(SimpleAudioData data)
        {
            Data = data;
        }
    }
}