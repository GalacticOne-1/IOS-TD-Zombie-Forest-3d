using Galactic1.Code.Gameplay.Combat.Cover;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Resolvers
{
    /// <summary>
    /// Resolves whether a shot is blocked by tactical cover.
    /// Used by HitResolver.
    /// </summary>
    public sealed class CoverResolver
    {
        /// <summary>
        /// Returns true if the shot is blocked by cover.
        /// Half cover blocks ~35%, full cover blocks ~75%.
        /// </summary>
        public bool IsBlocked(UnitCoverState cover)
        {
            float chance = cover.CoverType switch
            {
                CoverType.Half => 0.35f,
                CoverType.Full => 0.75f,
                _ => 0f
            };

            return Random.value <= chance;
        }
    }
}