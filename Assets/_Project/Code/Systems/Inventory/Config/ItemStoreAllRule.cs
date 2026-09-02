using Galactic1.Code.UI.Inventory;
using UnityEngine;

namespace Galactic1.Configs.UI
{
    [CreateAssetMenu(menuName = "Game Configs/Rules/UI/Item Store All Rule")]
    public class ItemStoreAllRule : ButtonRuleLogic
    {
        public override bool Evaluate(InventoryManagementWindow window)
        {
            var left = window.leftSide;
            if (left == null || left._source == null) return false;

            // Кнопка активна, если в левом инвентаре есть предметы
            foreach (var slot in left._source.GetSlots())
            {
                if (!slot.IsEmpty) return true;
            }

            return false;
        }
    }
}