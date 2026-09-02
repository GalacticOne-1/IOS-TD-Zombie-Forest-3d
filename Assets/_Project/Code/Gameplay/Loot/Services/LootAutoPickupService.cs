using System.Collections.Generic;
using Galactic1.RaidLoot.Events;
using Galactic1.RaidLoot.Runtime;
using UnityEngine;

namespace Galactic1.RaidLoot.Services
{
    /// <summary>
    /// Automatically transfers generated loot into RaidLootBuffer
    /// and emits a single ContainerLootCollectedEvent.
    /// </summary>
    public sealed class LootAutoPickupService
    {
        private readonly LootContainerRepository _repository;
        private readonly RaidLootBuffer _buffer;

        public LootAutoPickupService(
            LootContainerRepository repository,
            RaidLootBuffer buffer)
        {
            _repository = repository;
            _buffer = buffer;
        }

        public void OnLootGenerated(LootGeneratedEvent e)
        {
            if (!_repository.TryGet(e.RuntimeId, out var runtime))
            {
                Debug.LogWarning(
                    $"[LootAutoPickupService] Container not found: {e.RuntimeId}");
                return;
            }

            var collectedLoot = new List<LootGenerationRecord>(e.GeneratedItems.Count);

            foreach (var record in e.GeneratedItems)
            {
                if (!runtime.TryRemoveItem(record))
                    continue;

                _buffer.AddItem(record);

                collectedLoot.Add(record);
            }

            if (collectedLoot.Count == 0)
                return;

            EventBus<ContainerLootCollectedEvent>.Raise(
                new ContainerLootCollectedEvent(
                    e.RuntimeId,
                    collectedLoot));
        }
    }
}