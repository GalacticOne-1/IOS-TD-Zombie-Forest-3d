using Galactic1.Code.Gameplay.Combat.Events;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Listens to VisualImpactEvent and enqueues impact particle FX.
    ///
    /// LOD-gated: skips effects beyond threshold distance.
    /// Frame-budgeted: enqueues into AsyncFXSpawnQueue, not instant spawn.
    ///
    /// Lifecycle:
    ///   Created at raid init.
    ///   Disposed at raid end — unsubscribes from EventBus.
    /// </summary>
    public sealed class ImpactSurfaceResolver
    {
        private readonly CombatFXLODService _lod;
        private readonly AsyncFXSpawnQueue _queue;
        private readonly CombatSurfaceFXDatabase _db;

        private readonly EventBinding<VisualImpactEvent> _binding;

        public ImpactSurfaceResolver(
            CombatFXLODService lod,
            AsyncFXSpawnQueue queue,
            CombatSurfaceFXDatabase db)
        {
            _lod = lod;
            _queue = queue;
            _db = db;

            _binding = new EventBinding<VisualImpactEvent>(OnImpact);
            EventBus<VisualImpactEvent>.Register(_binding);
        }

        public void Dispose()
            => EventBus<VisualImpactEvent>.Deregister(_binding);

        // ── Handler ───────────────────────────────────────────────────

        private void OnImpact(VisualImpactEvent e)
        {
            if (!_lod.ShouldSpawnHeavyFX(e.Point))
                return;

            if (!_db.TryGet(e.Surface, out var cfg))
                return;

            if (cfg.ImpactFXId == null)
                return;

            _queue.Enqueue(new FXSpawnRequest(
                cfg.ImpactFXId,
                e.Point,
                Quaternion.LookRotation(-e.ShotDirection)));
        }
    }
}