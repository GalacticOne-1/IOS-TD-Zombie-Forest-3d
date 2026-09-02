using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Visual-only tracer request.
    /// Raised by CombatEventRouter per confirmed hit.
    ///
    /// Used by FakeBulletSystem (Phase 4).
    /// NOT authoritative — tracers are cosmetic only.
    /// </summary>
    public readonly struct VisualTracerEvent : IEvent
    {
        public readonly Vector3 Start;
        public readonly Vector3 End;
        public readonly WeaponType WeaponType;

        public VisualTracerEvent(Vector3 start, Vector3 end, WeaponType weaponType)
        {
            Start = start;
            End = end;
            WeaponType = weaponType;
        }
    }
}