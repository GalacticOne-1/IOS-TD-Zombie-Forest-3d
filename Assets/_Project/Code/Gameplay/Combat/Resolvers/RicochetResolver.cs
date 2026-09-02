using Galactic1.Code.Data.Combat;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Resolvers
{
    /// <summary>
    /// Resolves whether a projectile ricochets off a surface.
    /// Used by HitResolver.
    /// </summary>
    public sealed class RicochetResolver
    {
        /// <summary>
        /// Returns true if the projectile should ricochet.
        /// Probability is defined per surface in SurfaceMaterialConfig.
        /// </summary>
        public bool ShouldRicochet(SurfaceMaterialConfig cfg)
        {
            return Random.value <= cfg.RicochetChance;
        }
    }
}