using Galactic1.Code.Gameplay.Combat.Data;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Visual-only impact request.
    /// Raised by CombatEventRouter in response to GameplayHitEvent.
    ///
    /// Used by:
    /// - ImpactAggregationSystem (Phase 4)
    /// - DecalSystem (Phase 4)
    /// - CameraShakeSystem (Phase 4)
    ///
    /// IMPORTANT: Contains NO gameplay data.
    /// Visual systems must never make gameplay decisions from this event.
    /// </summary>
    public readonly struct VisualImpactEvent : IEvent
    {
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly Vector3 ShotDirection;
        public readonly SurfaceType Surface;

        public VisualImpactEvent(
            Vector3 point, 
            Vector3 normal,
            Vector3 shotDirection,
            SurfaceType surface)
        {
            Point = point;
            Normal = normal;
            ShotDirection = shotDirection;
            Surface = surface;
        }
    }
}