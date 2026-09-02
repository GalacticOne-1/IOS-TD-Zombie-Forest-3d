
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// Слоты создаются динамически из префаба.
    /// Живёт на MonoBehaviour — создаётся один раз, переиспользуется при каждом Bind.
    /// </summary>
    public sealed class DynamicSlotViewProvider : ISlotViewProvider
    {
        private readonly GameObject slotPrefab;
        private readonly ExternalScrollbarBinder binder;
        private readonly ScrollRect scrollRect;
        private readonly RectTransform container;
        private readonly List<InventorySlotView> pool = new();

        private const int MinVisibleSlots = 25;
        private int previousCount;

        public DynamicSlotViewProvider(
            ExternalScrollbarBinder binder,
            GameObject slotPrefab,
            ScrollRect scrollRect)
        {
            this.binder = binder;
            this.slotPrefab = slotPrefab;
            this.scrollRect = scrollRect;
            container = scrollRect.content;
        }

        public IReadOnlyList<InventorySlotView> GetSlots(int count)
        {
            if (count < MinVisibleSlots)
                count = MinVisibleSlots;

            // Расширяем пул только если реально не хватает
            while (pool.Count < count)
            {
                var go = Object.Instantiate(slotPrefab, container);
                var slot = go.GetComponent<InventorySlotView>();
                pool.Add(slot);
            }

            for (int i = 0; i < pool.Count; i++)
                pool[i].gameObject.SetActive(i < count);

            
            scrollRect.SetSizeContentGridLayoutGroup(true, false, false, true);

            if (count > MinVisibleSlots)
            {
                binder.Scrollbar.gameObject.SetActive(true);
                // ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait1(
                //     () =>
                //     {
                
                // !! убрал условие т.к был баг со скоролом если у машины меньше слотов !!
                //if(previousCount < count) // сброс скрола если предыдущий источник имел меньше слотов (разные источники)
                {
                    binder.Scrollbar.GetComponent<Scrollbar>().value = 1;
                    scrollRect.ScrollRectResetV();
                }
                    //});
            }
            else
            {
                binder.Scrollbar.gameObject.SetActive(false);
                scrollRect.ScrollRectResetV();
            }

            previousCount = count;
            return pool.GetRange(0, count);
        }

    }
}