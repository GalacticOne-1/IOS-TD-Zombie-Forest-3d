using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>Узкий query "сколько предмета X сейчас лежит в источнике типа Y" — нужен
    /// ТОЛЬКО для ретроактивной проверки ItemTransferredObjective (checkAlreadySatisfiedOnStart),
    /// не завязан на конкретный UI-инстанс IInventorySource (те транзиентны — пересоздаются
    /// на каждый InventoryManagementController.BuildSources, см. ItemTransferredEvent
    /// докстринг), читает "истинное" хранилище напрямую, как GetCampStorageAmount делает
    /// для BaseStorage.</summary>
    public interface ITutorialInventorySourceAmountQuery
    {
        int GetAmount(InventorySourceType sourceType, ItemId itemId);
    }
}