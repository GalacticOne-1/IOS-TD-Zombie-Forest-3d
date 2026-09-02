using Galactic1.Code.Gameplay.Audio;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Audio-only voice/reaction request.
    /// Raised when a unit takes damage or dies.
    ///
    /// Used by VoiceBudgetingSystem (Phase 5)
    /// to prevent simultaneous voice spam.
    /// </summary>
    /// <summary>
    /// Audio-only voice/reaction request.
    ///
    /// Raised by CombatEventRouter when a unit takes damage or dies.
    ///
    /// The event contains runtime audio data only.
    /// It does not reference gameplay unit objects.
    /// </summary>
    public readonly struct AudioVoiceEvent : IEvent
    {
        public readonly Vector3 Position;
        public readonly VoiceAudioData Data;
        public readonly VoiceEventType Type;
        public readonly int Priority;

        public AudioVoiceEvent(
            Vector3 position,
            VoiceAudioData data,
            VoiceEventType type,
            int priority)
        {
            Position = position;
            Data = data;
            Type = type;
            Priority = priority;
        }
    }
    
    public enum VoiceEventType
    {
        Damage,
        Death,
        Aggro
    }
}