
using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.RaidLoot.Authoring;

namespace Galactic1.RaidLoot.Events
{
    /// <summary>
    /// Published by LootContainerOpenService when a container transitions
    /// from Closed → Opening. Triggers LootGenerationService.
    /// </summary>
    public sealed class ContainerOpenedEvent : IEvent
    {
        public string RuntimeId { get; }
        public LootContainerTagId ContainerTagId { get; } 

        public ContainerOpenedEvent(string runtimeId)
        {
            RuntimeId = runtimeId;
            ContainerTagId = GameIdProvider.LootContainerTag;
        }
    }
}