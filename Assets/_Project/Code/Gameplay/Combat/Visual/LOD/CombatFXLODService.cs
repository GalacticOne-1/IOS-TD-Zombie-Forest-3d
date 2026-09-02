using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Distance-based combat FX throttling.
    ///
    /// All visual systems ask this service before spawning anything.
    /// Keeps mobile GPU budget under control by skipping
    /// effects that are too far from the camera to be meaningful.
    ///
    /// Thresholds are tunable — expose to AdaptiveCombatQualityService (Phase 6)
    /// by injecting multipliers.
    ///
    /// Used by:
    /// - ImpactAggregationSystem
    /// - DecalSystem
    /// - FakeBulletSystem
    /// - CameraShakeSystem
    /// </summary>
    public sealed class CombatFXLODService
    {
        private readonly Camera _camera;

        // Tuneable distance thresholds
        private float _heavyFXDistance = 20f;
        private float _decalDistance = 30f;
        private float _tracerDistance = 35f;

        public CombatFXLODService(Camera camera)
        {
            _camera = camera;
        }

        /// <summary>
        /// Call before spawning heavy particle effects (explosions, blood).
        /// </summary>
        public bool ShouldSpawnHeavyFX(Vector3 position) => true;
            //=> DistanceTo(position) < _heavyFXDistance;

        /// <summary>
        /// Call before spawning decals (bullet holes, blood stains).
        /// </summary>
        public bool ShouldSpawnDecal(Vector3 position)
            => DistanceTo(position) < _decalDistance;

        /// <summary>
        /// Call before spawning tracers.
        /// </summary>
        public bool ShouldSpawnTracer(Vector3 start)
            => DistanceTo(start) < _tracerDistance;

        /// <summary>
        /// Adjusts all thresholds by a quality multiplier (0..1).
        /// Called by AdaptiveCombatQualityService (Phase 6) on FPS drop.
        /// </summary>
        public void SetQualityMultiplier(float multiplier)
        {
            multiplier = Mathf.Clamp01(multiplier);
            _heavyFXDistance = 20f * multiplier;
            _decalDistance = 12f * multiplier;
            _tracerDistance = 35f * multiplier;
        }

        private float DistanceTo(Vector3 position)
            => Vector3.Distance(_camera.transform.position, position);
    }
}