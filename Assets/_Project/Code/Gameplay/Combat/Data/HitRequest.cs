using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Data
{
    /// <summary>
    /// Immutable single hit attempt.
    /// One FireRequest expands into N HitRequests via BurstFireResolver
    /// (N = shotgun pellets, burst count, minigun spray, etc).
    ///
    /// Created by BurstFireResolver.
    /// Consumed by HitResolver.
    /// </summary>
    public readonly struct HitRequest
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Direction;

        public readonly float Damage;
        public readonly float ArmorPenetration;

        public readonly IUnitSceneContext Attacker;

        public HitRequest(
            Vector3 origin,
            Vector3 direction,
            float damage,
            float armorPenetration,
            IUnitSceneContext attacker)
        {
            Origin = origin;
            Direction = direction;
            Damage = damage;
            ArmorPenetration = armorPenetration;
            Attacker = attacker;
        }
    }
}