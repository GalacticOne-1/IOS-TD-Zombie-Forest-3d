using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.UI.Inventory;

namespace Galactic1.Code.Inventory.Services
{
    /// <summary>
    /// Агрегированный порт ресурсов лагеря.
    ///
    /// Объединяет несколько источников ресурсов:
    /// - склады
    /// - транспорт
    /// - хранилища
    ///
    /// Роль:
    /// Composite Inventory Provider.
    /// Позволяет экономическим системам работать
    /// с несколькими инвентарями как с одним.
    /// </summary>
    public class CampInventoryPort : IInventoryResourcesPort
    {
        private readonly List<IInventoryResourcesPort> _sources;

        public CampInventoryPort(List<IInventoryResourcesPort> sources)
        {
            _sources = sources;
        }

        /// <summary>
        /// Возвращает суммарное количество ресурса
        /// во всех источниках лагеря.
        /// </summary>
        public int GetTotalAmount(RuntimeId itemKey)
        {
            int total = 0;

            foreach (var source in _sources)
                total += source.GetTotalAmount(itemKey);

            return total;
        }

        /// <summary>
        /// Пытается списать ресурс из нескольких источников.
        /// Списание происходит последовательно.
        /// </summary>
        public bool TrySpend(RuntimeId itemId, int amount)
        {
            if (amount <= 0)
                return true;

            int total = GetTotalAmount(itemId);
            if (total < amount)
                return false;

            int remaining = amount;

            foreach (var source in _sources)
            {
                if (remaining <= 0)
                    break;

                int available = source.GetTotalAmount(itemId);
                if (available <= 0)
                    continue;

                int toConsume = available >= remaining ? remaining : available;

                source.TrySpend(itemId, toConsume);
                remaining -= toConsume;
            }

            return true;
        }

        /// <summary>
        /// Пытается добавить предмет в любой источник,
        /// где есть место.
        /// </summary>
        public AddItemResult TryAdd(InventorySlotRuntime slotRuntime)
        {
            int initialAmount = slotRuntime.Amount;

            foreach (var source in _sources)
            {
                if (slotRuntime.Amount <= 0)
                    break;

                var result = source.TryAdd(slotRuntime);
                slotRuntime.Amount = result.Remaining;
            }

            return new AddItemResult(initialAmount - slotRuntime.Amount, slotRuntime.Amount);
        }

        /// <summary>
        /// Проверяет можно ли добавить предмет
        /// хотя бы в один источник.
        /// </summary>
        public bool CanAdd(InventorySlotRuntime slotRuntime)
        {
            foreach (var source in _sources)
            {
                if (source.CanAdd(slotRuntime))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет можно ли добавить несколько предметов.
        /// Проверка делается на каждом источнике.
        /// </summary>
        public bool CanAddMultiple(IEnumerable<InventorySlotRuntime> slotsToAdd)
        {
            foreach (var source in _sources)
            {
                if (source.CanAddMultiple(slotsToAdd))
                    return true;
            }

            return false;
        }
    }
}