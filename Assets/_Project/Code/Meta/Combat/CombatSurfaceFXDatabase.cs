using System.Collections.Generic;
using Galactic1.Code.Data.Combat;
using Galactic1.Code.Gameplay.Combat.Data;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Runtime lookup: SurfaceType → CombatSurfaceFXConfig.
    /// Populated at scene init from ScriptableObjects.
    ///
    /// Used by:
    /// - ImpactAggregationSystem
    /// - DecalSystem
    /// - AudioSurfaceDatabase (Phase 5)
    /// </summary>
    public sealed class CombatSurfaceFXDatabase
    {
        private readonly Dictionary<SurfaceType, CombatSurfaceFXConfig> _map = new();
        private CombatSurfaceFXConfig _fallback;

        public CombatSurfaceFXDatabase(Dictionary<string, ScriptableObject> rawConfigs)
        {
            foreach (var pair in rawConfigs)
            {
                if(pair.Value is CombatSurfaceFXConfig config)
                {
                    _map[config.Surface] = config;

                    if (config.Surface == SurfaceType.Default)
                        _fallback = config;
                }
            }

            if (_fallback == null)
                Debug.LogWarning("[CombatSurfaceFXDatabase] No Default surface FX config found.");
        }

        /// <summary>
        /// Returns config for the given surface.
        /// Falls back to Default if no explicit config exists.
        /// </summary>
        public CombatSurfaceFXConfig Get(SurfaceType type)
        {
            if (_map.TryGetValue(type, out var cfg))
                return cfg;

            return _fallback;
        }

        public bool TryGet(SurfaceType type, out CombatSurfaceFXConfig cfg)
            => _map.TryGetValue(type, out cfg) || (cfg = _fallback) != null;
    }
}