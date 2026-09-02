using System.Collections.Generic;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Runtime;
using UnityEngine;

namespace Galactic1.RaidLoot.Services
{
    /// <summary>
    /// Central registry of all LootContainerRuntime instances in the current raid.
    /// Lives in LocationContext. All services use this instead of holding
    /// direct references to individual runtimes.
    /// </summary>
    public sealed class LootContainerRepository : IGameService
    {
        private readonly Dictionary<string, LootContainerRuntime> _containers = new();

        public void Register(LootContainerRuntime runtime)
        {
            var id = runtime.Id;
            if (_containers.ContainsKey(id))
            {
                Debug.LogError($"[LootContainerRepository] Duplicate: {id}");
                return;
            }
            _containers[id] = runtime;
        }

        public void Unregister(string id)
            => _containers.Remove(id);

        public LootContainerRuntime Get(string id)
        {
            if (_containers.TryGetValue(id, out var runtime))
                return runtime;
            Debug.LogError($"[LootContainerRepository] Not found: {id}");
            return null;
        }

        public bool TryGet(string id, out LootContainerRuntime runtime)
            => _containers.TryGetValue(id, out runtime);

        // ── Lifecycle ────────────────────────────────────────────────────────

        public void Clear() => _containers.Clear();
    }
}