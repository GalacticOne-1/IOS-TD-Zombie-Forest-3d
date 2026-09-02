using Galactic1.Code.UI.Inventory;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Configs.UI
{
    [CreateAssetMenu(menuName = "Game Configs/Rules/UI/Item Is Equipable Rule")]
    public class ItemIsEquipableRule : ButtonRuleLogic
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
            if (item == null ||
                !item.HasModule<ActionModule>() ||
                item.Action.Actions.Count == 0 ||
                !window.modeController.SquadMode()) // <= нужен режим юнитов
                return false;

            var controller = window.controller;
            if (controller == null)
                return false;

            var leftSource = controller.LeftSource;
            var rightSource = controller.RightSource;
            var accessService = controller.AccessService;
            
            // -----------------------------------------------------
            // 🚫 Если обе стороны — не экипировка → нельзя
            // -----------------------------------------------------
            if (leftSource == null || rightSource == null ||
                !accessService._inventoryRules.IsEquipmentSource(leftSource) &&
                !accessService._inventoryRules.IsEquipmentSource(rightSource))
                return false;

            // -----------------------------------------------------
            // Проверка типа персонажа (юнит/техника)
            // -----------------------------------------------------
            if (!accessService._equipmentValidation.CheckSource(rightSource, item))
                return false;
            
            bool isRightSide = view._source == rightSource;
            bool isLeftSide  = view._source == leftSource;
            
            var leftInv  = leftSource?.InventoryData;
            var rightInv = rightSource?.InventoryData;


            // =====================================================
            // 🔥 1) ПРАВАЯ СТОРОНА — ЭКИПИРОВАНИЕ
            // =====================================================
            if (isRightSide)
            {
                // расходник → Use
                if (item.HasModule<UseModule>() && item.Use.ConsumeOnUse)
                {
                    window.SetDynamicButtonText("Use");
                    return true;
                }
            
                // всё остальное → false
                return false;
            }

            // =====================================================
            // 🔥 2) ЛЕВАЯ СТОРОНА — ИНВЕНТАРЬ
            // =====================================================
            if (isLeftSide)
            {
                // -----------------------------------------------------
                // 🧳 Проверка сумки (как в MoveSlot)
                // -----------------------------------------------------
                // if (item.Category == ItemCategory.Stuff && 
                //     item.Config.EquipSlotType == ItemEquipSlotType.Bag)
                // {
                //     var toEquip = rightInv as CharacterEquipmentInventoryData;
                //     if (toEquip == null)
                //         return false;
                //
                //     // находим индекс слота экипировки, куда попадёт сумка
                //     // (в UI ты обычно знаешь index → но тут безопасно достать через window)
                //     int? equipIndex = InventoryRules.FindBestBagSlot(toEquip, item);
                //     if (!equipIndex.HasValue || !toEquip.slotTypes.TryGetValue(equipIndex.Value, out var equipSlotType))
                //         return false;
                //
                //     if (!InventoryRules.IsBagSlot(equipSlotType))
                //         return false;
                //
                //     var oldSlot = new InventorySlot(toEquip.InventoryProxy.Slots[equipIndex.Value].Item.Value,
                //         toEquip.InventoryProxy.Slots[equipIndex.Value].Amount.Value,
                //         toEquip.InventoryProxy.Slots[equipIndex.Value].Durability.Value);
                //     var newSlot = new InventorySlot(item, slot.amount, slot.durability);
                //
                //     var leftInventory = leftInv as CharacterInventoryData;
                //
                //     
                //     if (!toEquip.CanChangeBag(
                //             view.selectedSlot.SlotIndex,      // fromIndex
                //             rightInv,                       // toInventory
                //             equipIndex.Value,               // toIndex
                //             oldSlot,
                //             newSlot,
                //             leftInventory))
                //     {
                //         // Нельзя надеть сумку
                //         return false;
                //     }
                //
                //     // сумку надеть можно = кнопка активна
                //     window.SetDynamicButtonText("Equip");
                //     return true;
                // }


                // -----------------------------------------------------
                // 🧰 Обычные предметы
                // -----------------------------------------------------
                 if (item.HasModule<UseModule>() && item.Use.ConsumeOnUse)
                 {
                     window.SetDynamicButtonText("Use");
                     return true;
                 }
                 else
                 {
                     window.SetDynamicButtonText("Equip");
                     return true;
                 }
            }

            return false;
        }
    }
}
