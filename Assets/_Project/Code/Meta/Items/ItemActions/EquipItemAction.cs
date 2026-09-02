using UnityEngine;

namespace Galactic1.Items
{
    [CreateAssetMenu(fileName = "EquipItemAction", menuName = "Game Configs/Inventory/Equip Item Action")]
    public class EquipItemAction : ItemActionConfig
    {
        public override void Execute(ItemContext ctx)
        {
            if (ctx.window == null || ctx.view == null)
            {
                Debug.LogError("EquipItemAction: context is missing UI or Window");
                return;
            }

            var source = ctx.view._source;
            int slotIndex = ctx.view.selectedSlot.SlotIndex;

            // (!!!) Твоя основная логика экипировки
            ctx.window.controller.HandleEquip(source, slotIndex);
        }
    }
}