using Galactic1.Code.Gameplay.Combat.Data;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Hit
{
    /// <summary>
    /// Final deterministic result of a single hit resolution.
    /// Produced by HitResolver.
    /// Consumed by WeaponFireService → DamagePipeline → gameplay events.
    /// </summary>
    public readonly struct HitResult
    {
        public readonly bool Hit;

        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly Vector3 ShotDirection;

        public readonly SurfaceType Surface;
        public readonly BodyPartType BodyPart;

        /// <summary>
        /// Target unit hit, if any. Null for environment-only hits or misses.
        /// </summary>
        public readonly IUnitSceneContext Target;

        public readonly float Damage;
        public readonly float ArmorPenetration;

        public HitResult(
            bool hit,
            Vector3 point,
            Vector3 normal, 
            Vector3 shotDirection,
            SurfaceType surface,
            BodyPartType bodyPart,
            IUnitSceneContext target,
            float damage,
            float armorPenetration)
        {
            Hit = hit;
            Point = point;
            Normal = normal;
            ShotDirection = shotDirection;
            Surface = surface;
            BodyPart = bodyPart;
            Target = target;
            Damage = damage;
            ArmorPenetration = armorPenetration;
        }
    }
}