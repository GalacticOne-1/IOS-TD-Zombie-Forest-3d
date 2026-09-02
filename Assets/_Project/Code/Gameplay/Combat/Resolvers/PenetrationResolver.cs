using Galactic1.Code.Data.Combat;

namespace Galactic1.Code.Gameplay.Combat.Resolvers
{
    /// <summary>
    /// Modifies damage based on surface penetration properties.
    /// Used by HitResolver.
    /// </summary>
    public sealed class PenetrationResolver
    {
        /// <summary>
        /// Returns damage after applying surface penetration modifier.
        /// </summary>
        public float Resolve(float damage, SurfaceMaterialConfig cfg)
        {
            return damage * cfg.PenetrationModifier;
        }
    }
}