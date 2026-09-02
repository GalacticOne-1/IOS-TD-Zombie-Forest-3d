
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// Слоты уже созданы в префабе и настроены в инспекторе.
    /// Пул не нужен — просто собираем children из slotRoot.
    /// Используется для слотов экипировки.
    /// </summary>
    public sealed class StaticSlotViewProvider : ISlotViewProvider
    {
        private readonly List<InventorySlotView> slots = new();

        public StaticSlotViewProvider(Transform[] slotRoots)
        {
            foreach (var root in slotRoots)
            {
                var l = root.childCount;
                for (int i = 0; i < l; i++)
                {
                    var view = root.GetChild(i).GetComponent<InventorySlotView>();
                    if (view != null)
                        slots.Add(view);
                }
            }
        }

        // count игнорируем — слоты фиксированы
        public IReadOnlyList<InventorySlotView> GetSlots(int count) => slots;
    }
}