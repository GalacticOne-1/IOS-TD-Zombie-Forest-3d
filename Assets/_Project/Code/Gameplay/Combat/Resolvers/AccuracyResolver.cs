using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Resolvers
{
    /// <summary>
    /// Rolls whether a shot hits based on accuracy value.
    /// Used by HitResolver.
    /// </summary>
    public sealed class AccuracyResolver
    {
        /// <summary>
        /// Returns true if the shot hits.
        /// </summary>
        /// <param name="accuracy">0..1 — probability of hit</param>
        public bool RollHit(float accuracy)
        {
            return Random.value <= accuracy;
        }
    }
}