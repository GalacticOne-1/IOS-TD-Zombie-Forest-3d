using System.Collections.Generic;
using Galactic1.Code.Data.Combat;
using Galactic1.Code.Gameplay.Combat.Data;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Config
{
    /// <summary>
    /// Runtime lookup database for surface material configs.
    /// Populated at scene init from ScriptableObjects.
    /// Used by:
    /// - HitResolver
    /// - PenetrationResolver
    /// - RicochetResolver
    /// </summary>
    public sealed class SurfaceMaterialDatabase
    {
        private readonly Dictionary<SurfaceType, SurfaceMaterialConfig> _map = new();

        private SurfaceMaterialConfig _fallback;

        public SurfaceMaterialDatabase(IEnumerable<SurfaceMaterialConfig> configs)
        {
            foreach (var cfg in configs)
            {
                _map[cfg.Surface] = cfg;

                if (cfg.Surface == SurfaceType.Default)
                    _fallback = cfg;
            }

            if (_fallback == null)
                Debug.LogWarning("[SurfaceMaterialDatabase] No Default surface config found. " +
                                 "Add a SurfaceMaterialConfig with Surface = Default.");
        }

        /// <summary>
        /// Returns config for the given surface.
        /// Falls back to Default if the surface has no explicit config.
        /// </summary>
        public SurfaceMaterialConfig Get(SurfaceType type)
        {
            if (_map.TryGetValue(type, out var cfg))
                return cfg;

            return _fallback;
        }
    }
}