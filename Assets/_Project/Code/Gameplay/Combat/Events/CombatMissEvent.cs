using Galactic1.Code.Systems.Raid;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Raised when a HitRequest resolves with no raycast hit (clean miss).
    ///
    /// Uses existing EventBus&lt;T&gt; infrastructure — no new bus class.
    /// </summary>
    public readonly struct CombatMissEvent : IEvent
    {
        public readonly IUnitSceneContext Attacker;
        public readonly Vector3 Origin;
        public readonly Vector3 Direction;
        public readonly WeaponType WeaponType;

        public CombatMissEvent(
            IUnitSceneContext attacker,
            Vector3 origin,
            Vector3 direction,
            WeaponType weaponType)
        {
            Attacker = attacker;
            Origin = origin;
            Direction = direction;
            WeaponType = weaponType;
        }
    }
}