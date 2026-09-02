using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.UI.Inventory;

namespace Galactic1.Code.Inventory.Services
{
    /// <summary>
    /// Порт ресурсов локации.
    ///
    /// Роль:
    /// Adapter для одного источника инвентаря
    /// (обычно инвентарь игрока или транспорт).
    /// </summary>
    public class LocationInventoryPort : IInventoryResourcesPort
    {
        private readonly IInventoryResourcesPort _source;

        public LocationInventoryPort(IInventoryResourcesPort source)
        {
            _source = source;
        }

        /// <summary>
        /// Делегирует получение количества ресурса.
        /// </summary>
        public int GetTotalAmount(RuntimeId itemKey)
        {
            return _source.GetTotalAmount(itemKey);
        }

        /// <summary>
        /// Делегирует списание ресурса.
        /// </summary>
        public bool TrySpend(RuntimeId itemId, int amount)
        {
            return _source.TrySpend(itemId, amount);
        }

        /// <summary>
        /// Делегирует добавление предмета.
        /// </summary>
        public AddItemResult TryAdd(InventorySlotRuntime slotRuntime)
        {
            return _source.TryAdd(slotRuntime);
        }

        /// <summary>
        /// Делегирует проверку добавления.
        /// </summary>
        public bool CanAdd(InventorySlotRuntime slotRuntime)
        {
            return _source.CanAdd(slotRuntime);
        }

        /// <summary>
        /// Делегирует проверку множественного добавления.
        /// </summary>
        public bool CanAddMultiple(IEnumerable<InventorySlotRuntime> slotsToAdd)
        {
            return _source.CanAddMultiple(slotsToAdd);
        }
    }
}