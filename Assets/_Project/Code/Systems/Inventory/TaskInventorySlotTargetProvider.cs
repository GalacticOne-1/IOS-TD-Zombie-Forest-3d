using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Tutorial.Presentation;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// Реализация ITutorialItemSlotTargetProvider поверх реальных InventoryView.
    /// Живёт в Inventory UI namespace (а не в Tutorial), т.к. только она знает про
    /// InventoryView/_access/_source — тутор про эти типы не знает вообще ничего
    /// (см. TutorialInventoryViewRegistry, которая хранит только базовый MonoBehaviour-
    /// компонент, без раскрытия его внутреннего API наружу тутора).
    /// </summary>
    public sealed class TaskInventorySlotTargetProvider : ITutorialItemSlotTargetProvider
    {
        private readonly TutorialInventoryViewRegistry _registry;
        private InventoryAccessService _inventoryAccess;

        public TaskInventorySlotTargetProvider(TutorialInventoryViewRegistry registry)
        {
            _registry = registry;
        }

        public bool TryGetSlotTarget(ItemId itemId, out ITutorialTarget target)
        {
            _inventoryAccess ??= ServiceLocator.Current.Get<InventoryManagementWindow>().controller.AccessService;
            
            foreach (var view in _registry.Active)
            {
                if (view != null 
                    && _inventoryAccess.TryFindSlotRectByItem(view._source, view, itemId, out var slotRect))
                {
                    target = new DynamicRectTutorialTarget(slotRect);
                    return true;
                }
            }

            target = null;
            return false;
        }
    }
}
