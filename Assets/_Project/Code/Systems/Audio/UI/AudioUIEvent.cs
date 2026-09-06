using Galactic1.Code.Gameplay.Audio;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    public readonly struct AudioUIEvent : IEvent
    {
        public readonly AudioCueData Data;

        public AudioUIEvent(AudioCueData data)
        {
            Data = data;
        }
    }
}