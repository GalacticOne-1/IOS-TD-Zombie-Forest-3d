using Galactic1.Code.Gameplay.Combat.Data;
using UnityEngine;
namespace Galactic1.Code.Gameplay.Combat.Resolvers
{
    /// <summary>
    /// Resolves physical surface material from a collider.
    /// Reads Unity tags — tags must be defined in Project Settings.
    /// Used by HitResolver.
    /// </summary>
    public sealed class SurfaceResolver
    {
        public SurfaceType Resolve(Collider collider)
        {
            return collider.tag switch
            {
                "Metal"    => SurfaceType.Metal,
                "Concrete" => SurfaceType.Concrete,
                "Wood"     => SurfaceType.Wood,
                "Organic"  => SurfaceType.Organic,
                "Glass"    => SurfaceType.Glass,
                _          => SurfaceType.Default
            };
        }
    }
}