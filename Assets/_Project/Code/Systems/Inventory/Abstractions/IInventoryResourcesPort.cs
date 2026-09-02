using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.UI.Inventory;

namespace Galactic1.Code.Inventory.Abstractions
{
    /// <summary>
    /// Минимальный контракт взаимодействия производства с инвентарём.
    /// Только доменно-значимые операции.
    /// </summary>
    public interface IInventoryResourcesPort
    {
        /// <summary>
        /// Возвращает общее количество предмета.
        /// </summary>
        int GetTotalAmount(RuntimeId itemId);

        /// <summary>
        /// Пытается списать указанное количество предмета.
        /// Возвращает false если ресурсов недостаточно.
        /// </summary>
        bool TrySpend(RuntimeId itemId, int amount);

        /// <summary>
        /// Пытается добавить предмет в инвентарь.
        /// Возвращает false если нет места.
        /// </summary>
        AddItemResult TryAdd(InventorySlotRuntime slotRuntime);

        /// <summary>
        /// Проверка добавления одного слота
        /// <br/>Без мутации состояния
        /// </summary>
        bool CanAdd(InventorySlotRuntime slotRuntime);

        /// <summary>
        /// Проверка добавления списка
        /// <br/>Без мутации состояния и с учётом суммарного эффекта всех добавлений
        /// </summary>
        bool CanAddMultiple(IEnumerable<InventorySlotRuntime> slotsToAdd);
    }
}