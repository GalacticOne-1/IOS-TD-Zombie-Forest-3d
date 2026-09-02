using Galactic1.Code.UI.Inventory;
using UnityEngine;

namespace Galactic1.Configs.UI
{
    [CreateAssetMenu(menuName = "Game Configs/Rules/UI/Item Remove Rule")]
    public class ItemRemoveRule : ButtonRuleLogic
    {
        public override bool Evaluate(InventoryManagementWindow window)
        {
            var view = window.GetActiveView();
            if (view == null || view.selectedSlot == null) 
                return false;

            var slot = view.GetSlot(view.selectedSlot.SlotIndex);
            if (slot == null || slot.IsEmpty) 
                return false;

            var item = slot.Item;
            if (item == null) 
                return false;

            // -----------------------------------------------------------
            // 🎒 Если выбрана сумка, проверяем: можно ли её удалить?
            // -----------------------------------------------------------
            // if (item is BagItem)
            // {
            //     // Получаем экипировку игрока
            //     if (ui._source is CharacterEquipmentInventoryData equip)
            //     {
            //         // Проверяем, можно ли удалить сумку
            //         if (!equip.CanRemoveBag(window.leftSide._source as CharacterInventoryData, ui.selectedSlot.SlotIndex))
            //             return false;
            //     }
            // }

            return true;
        }
    }
}