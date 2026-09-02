using Galactic1.Code.Gameplay.Combat.Data;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Audio-only impact sound request.
    /// Raised by CombatEventRouter in response to GameplayHitEvent.
    ///
    /// Delay simulates realistic sound travel time
    /// (hear gunshot first, hear impact shortly after).
    ///
    /// Used by DelayedImpactAudioSystem (Phase 5).
    /// </summary>
    public readonly struct AudioImpactEvent : IEvent
    {
        public readonly Vector3 Position;
        public readonly SurfaceType Surface;

        /// <summary>Playback delay in seconds after the event is raised.</summary>
        public readonly float Delay;

        public AudioImpactEvent(Vector3 position, SurfaceType surface, float delay)
        {
            Position = position;
            Surface  = surface;
            Delay    = delay;
        }
    }
}