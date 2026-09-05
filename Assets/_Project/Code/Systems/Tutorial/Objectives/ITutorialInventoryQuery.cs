using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Core.Enums;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public interface ITutorialInventoryQuery
    {
        bool IsItemEquippedByAnyStrategicUnit(EquipSlotType slot, ItemId itemId = null);
        int GetCampStorageAmount(ItemId itemId);
    }
}
