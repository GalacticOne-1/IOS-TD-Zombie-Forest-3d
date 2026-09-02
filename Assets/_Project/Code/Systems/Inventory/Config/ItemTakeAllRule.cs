using Galactic1.Code.UI.Inventory;
using UnityEngine;

namespace Galactic1.Configs.UI
{

    [CreateAssetMenu(menuName = "Game Configs/Rules/UI/Item Take All Rule")]
    public class ItemTakeAllRule : ButtonRuleLogic
    {
        public override bool Evaluate(InventoryManagementWindow window)
        {
            var right = window.rightSide;
            if (right == null || right._source == null) return false;

            // Например, кнопка активна, если в правом инвентаре есть предметы
            foreach (var slot in right._source.GetSlots())
            {
                if (!slot.IsEmpty) return true;
            }

            return false;
        }
    }
}