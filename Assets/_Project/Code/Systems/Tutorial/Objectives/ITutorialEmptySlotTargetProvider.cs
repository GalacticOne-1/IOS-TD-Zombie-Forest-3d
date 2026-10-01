
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Tutorial.Presentation;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public interface ITutorialEmptySlotTargetProvider
    {
        bool TryGetEmptySlotTarget(InventorySourceType sourceType, out ITutorialTarget target);
    }
}