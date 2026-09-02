using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.RaidLoot.Runtime
{
    /// <summary>
    /// Temporary buffer that accumulates all loot picked up during a raid.
    /// Lives only while RaidRuntime is active.
    /// Created by RaidRuntime. Destroyed (cleared) after CalculateResult().
    ///
    /// Access is intentionally narrow:
    ///   Write  → LootAutoPickupService
    ///   Read   → LootResultMapper (via RaidRuntime.CalculateResult)
    /// </summary>
    public sealed class RaidLootBuffer
    {
        private readonly List<BufferedLootEntry> _entries = new();

        public IReadOnlyList<BufferedLootEntry> Entries => _entries;

        public event Action<BufferedLootEntry> OnItemAdded;

        // ── Write ────────────────────────────────────────────────────────────

        public void AddItem(LootGenerationRecord record)
        {
            var entry = new BufferedLootEntry(record, Time.time);
            _entries.Add(entry);
            OnItemAdded?.Invoke(entry);
        }

        // ── Read ─────────────────────────────────────────────────────────────

        public IReadOnlyList<BufferedLootEntry> GetAll() => _entries;

        public int TotalItemCount()
        {
            var total = 0;
            foreach (var e in _entries) total += e.Amount;
            return total;
        }

        // ── Lifecycle ────────────────────────────────────────────────────────

        /// <summary>Called by RaidRuntime after CalculateResult() completes.</summary>
        public void Clear() => _entries.Clear();
    }
}