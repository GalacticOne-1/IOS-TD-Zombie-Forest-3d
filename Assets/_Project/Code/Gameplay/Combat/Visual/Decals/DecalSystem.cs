using Galactic1.Code.Gameplay.Combat.Events;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Listens to VisualImpactEvent and enqueues decal (bullet hole / blood) FX.
    ///
    /// Tighter LOD distance than ImpactAggregationSystem —
    /// decals are only visible up close.
    ///
    /// Skips surfaces with no DecalId configured.
    ///
    /// Lifecycle:
    ///   Created at raid init.
    ///   Disposed at raid end — unsubscribes from EventBus.
    /// </summary>
    public sealed class DecalSystem
    {
        private readonly CombatFXLODService _lod;
        private readonly AsyncFXSpawnQueue _queue;
        private readonly CombatSurfaceFXDatabase _db;

        private readonly EventBinding<VisualImpactEvent> _binding;

        public DecalSystem(
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
            if (!_lod.ShouldSpawnDecal(e.Point))
                return;

            if (!_db.TryGet(e.Surface, out var cfg))
                return;

            if (cfg.DecalId == null)
                return;

            _queue.Enqueue(new FXSpawnRequest(
                cfg.DecalId,
                e.Point,
                Quaternion.LookRotation(e.Normal)));
        }
    }
}