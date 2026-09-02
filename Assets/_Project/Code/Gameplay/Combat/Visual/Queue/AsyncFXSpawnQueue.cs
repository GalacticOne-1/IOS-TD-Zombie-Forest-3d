using System.Collections.Generic;
using Galactic1.PoolObject;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Frame-budgeted async FX spawning queue.
    ///
    /// All combat visual systems enqueue requests here.
    /// LateUpdateM() drains up to maxPerFrame requests per LateUpdate.
    /// Prevents frame spikes when many hits land simultaneously.
    ///
    /// FIXED vs initial Phase 4 draft:
    /// - Implements ILateUpdate (LateUpdateM) — matches MonoBehaviourMaster.lateUpdate
    ///   contract used elsewhere in the project, not IUpdate/UpdateM.
    ///
    /// Lifecycle:
    ///   Initialize() — register with MonoBehaviourMaster at raid start
    ///   Dispose()    — deregister at raid end
    /// </summary>
    public sealed class AsyncFXSpawnQueue : IUpdate
    {
        private readonly Queue<FXSpawnRequest> _queue = new();
        private readonly int _maxPerFrame;
        private readonly EffectRequestSystem _effectSystem;

        public AsyncFXSpawnQueue(int maxPerFrame, EffectRequestSystem effectSystem)
        {
            _maxPerFrame = maxPerFrame;
            _effectSystem = effectSystem;
        }

        public void Enqueue(FXSpawnRequest request)
            => _queue.Enqueue(request);

        public int PendingCount => _queue.Count;

        // ── ILateUpdate ───────────────────────────────────────────────

        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        public void UpdateM()
        {
            int count = Mathf.Min(_maxPerFrame, _queue.Count);

            for (int i = 0; i < count; i++)
                _queue.Dequeue().Execute(_effectSystem);
        }

        // ── Lifecycle ─────────────────────────────────────────────────

        public void Initialize()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
        }

        public void Dispose()
        {
            _queue.Clear();
            IUpdateClear();
        }
    }
}