using Galactic1.Code.Gameplay.Combat.Data;
using Galactic1.Code.Gameplay.Damage;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Resolvers
{
    /// <summary>
    /// Resolves which body zone was hit using the existing HitboxProxy.
    /// Falls back to Torso if the collider has no HitboxProxy.
    /// Used by HitResolver.
    /// </summary>
    public sealed class BodyPartResolver
    {
        public BodyPartType Resolve(Collider collider)
        {
            if (collider.TryGetComponent(out HitboxProxy proxy))
                return proxy.BodyPart;

            return BodyPartType.Torso;
        }
    }
}