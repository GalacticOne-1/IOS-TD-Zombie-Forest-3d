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

        public ContainerOpenedEvent(string runtimeId)
        {
            RuntimeId = runtimeId;
        }
    }
}