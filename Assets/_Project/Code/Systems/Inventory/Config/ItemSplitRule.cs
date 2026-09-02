using Galactic1.Code.UI.Inventory;
using UnityEngine;

namespace Galactic1.Configs.UI
{
    [CreateAssetMenu(menuName = "Game Configs/Rules/UI/Item Split Rule")]
    public class ItemSplitRule : ButtonRuleLogic
    {
        public override bool Evaluate(InventoryManagementWindow window)
        {
            var view = window.GetActiveView();
            if (view == null || view.selectedSlot == null)
                return false;

            var accessService = window.controller.AccessService;

            // ❌ Сплит запрещён в экипировке
            if (accessService._inventoryRules.IsEquipmentSource(view._source))
                return false;

            var slot = view.GetSlot(view.selectedSlot.SlotIndex);
            if (slot == null || slot.IsEmpty || slot.Amount <= 1)
                return false;

            // 🔍 Проверяем есть ли свободное место
            var slots = accessService.GetSlots(view._source);

            bool hasEmpty = false;
            var l = slots.Count;
            for (int i = 0; i < l; i++)
            {
                if (slots[i].IsEmpty)
                {
                    hasEmpty = true;
                    break;
                }
            }

            return hasEmpty;
        }
    }
}