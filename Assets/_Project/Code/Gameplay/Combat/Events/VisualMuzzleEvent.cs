using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Visual-only muzzle flash request.
    /// Raised by WeaponFireService when a burst begins.
    ///
    /// Used by weapon FX systems (Phase 4).
    /// </summary>
    public readonly struct VisualMuzzleEvent : IEvent
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public VisualMuzzleEvent(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}