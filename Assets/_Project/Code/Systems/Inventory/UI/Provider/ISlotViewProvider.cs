using System.Collections.Generic;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// Стратегия получения слотов для InventoryView.
    /// Две реализации: статичные (экипировка) и динамические (инвентарь).
    /// </summary>
    public interface ISlotViewProvider
    {
        /// <summary>
        /// Возвращает актуальный список слотов под нужный размер.
        /// Для статичных — всегда возвращает одни и те же объекты.
        /// Для динамических — resize пула.
        /// </summary>
        IReadOnlyList<InventorySlotView> GetSlots(int count);
    }
}