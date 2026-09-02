using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Enums;
using Galactic1.RaidLoot.Events;
using UnityEngine;

namespace Galactic1.RaidLoot.Services
{
    /// <summary>
    /// Handles a container open request from ProximityTrigger.
    /// Responsibility: guard check + state transition + publish ContainerOpenedEvent.
    /// Does NOT know about loot generation or the buffer.
    /// </summary>
    public sealed class LootContainerOpenService
    {
        private readonly LootContainerRepository _repository;

        public LootContainerOpenService(LootContainerRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Called by ProximityTrigger when the open timer elapses.
        /// </summary>
        // public void RequestOpen(LootContainerId containerId)
        // {
        //     if (!_repository.TryGet(containerId, out var runtime))
        //     {
        //         Debug.LogWarning($"[LootContainerOpenService] Unknown container: {containerId}");
        //         return;
        //     }
        //
        //     // Guard — only open once
        //     if (runtime.IsOpened)
        //         return;
        //
        //     runtime.SetState(ContainerState.Open);
        //     EventBus<ContainerOpenedEvent>.Raise(new ContainerOpenedEvent(containerId));
        // }
        public void RequestOpen(string runtimeId)
        {
            if (!_repository.TryGet(runtimeId, out var runtime)) return;
            if (runtime.IsOpened) 
                return;              // guard

            runtime.SetState(ContainerState.Opening); // ← не Open
            EventBus<ContainerOpenedEvent>.Raise(new ContainerOpenedEvent(runtime.Id));
        }
    }
}