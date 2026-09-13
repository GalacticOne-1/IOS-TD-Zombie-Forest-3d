using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>Live-состояние текущего пользовательского взаимодействия со слотом
    /// инвентаря — отдельно от ITutorialInventoryQuery (equip/storage — состояние данных,
    /// а не состояние UI-жеста). itemId == null в реализациях = "любой предмет".</summary>
    public interface ITutorialInventoryInteractionQuery
    {
        bool IsItemSelected(ItemId itemId);
        bool IsItemBeingDragged(ItemId itemId);
    }
}