using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Gameplay.Weapons.View;
using Galactic1.PoolObject;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Visual-only tracer projectile renderer.
    ///
    /// Subscribes to VisualTracerEvent and spawns a pooled TracerProjectile
    /// for each event. That is its ONLY job.
    ///
    /// CHANGE (Phase 3):
    /// All cadence / WeaponType / pellet-fraction logic removed — that is
    /// now TracerCadenceSystem's responsibility. FakeBulletSystem receives
    /// a VisualTracerEvent only when TracerCadenceSystem (or CombatEventRouter
    /// for misses) has already decided a tracer is needed.
    ///
    /// TracerProjectile.ProcessHit() does NOT apply damage — it just
    /// returns to pool on trigger. BaseProjectile.ProcessHit() does apply
    /// damage via DamageResolver, but TracerProjectile overrides that.
    ///
    /// LOD gating: tracers beyond CombatFXLODService._tracerDistance
    /// are skipped to stay within GPU budget.
    ///
    /// Lifecycle:
    /// Created at raid init (BuildCombatRuntime / CombatVisualRuntime).
    /// Disposed at raid end — unsubscribes from EventBus.
    /// </summary>
    public sealed class FakeBulletSystem
    {
        private readonly CombatFXLODService _lod;
        private readonly CombatTracerDatabase _tracerDb;
        private readonly EventBinding<VisualTracerEvent> _binding;

        public FakeBulletSystem(CombatFXLODService lod, CombatTracerDatabase tracerDb)
        {
            _lod = lod;
            _tracerDb = tracerDb;
            _binding = new EventBinding<VisualTracerEvent>(OnTracer);
            EventBus<VisualTracerEvent>.Register(_binding);
        }

        public void Dispose() => EventBus<VisualTracerEvent>.Deregister(_binding);

        private void OnTracer(VisualTracerEvent e)
        {
            if (!_lod.ShouldSpawnTracer(e.Start)) return;

            var ammo = _tracerDb.Get(e.WeaponType);
            if (ammo == null) return;

            Vector3 direction = (e.End - e.Start).normalized;
            var pool = ServiceLocator.Current.Get<PoolManager>();
            var bullet = pool.Get<BaseProjectile>(ammo)?.GetComponent<BaseProjectile>();

            if (bullet == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[FakeBulletSystem] No TracerProjectile in pool for {e.WeaponType}.");
#endif
                return;
            }

            
            bullet.transform.SetPositionAndRotation(e.Start, Quaternion.LookRotation(direction));
            bullet.Launch(null, direction, 0f, 0f);
        }
    }
}