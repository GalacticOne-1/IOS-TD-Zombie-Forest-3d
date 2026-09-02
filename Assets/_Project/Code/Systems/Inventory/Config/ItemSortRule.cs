
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.UI.Inventory;
using UnityEngine;

namespace Galactic1.Configs.UI
{
    [CreateAssetMenu(menuName = "Game Configs/Rules/UI/Item Sort Rule")]
    public class ItemSortRule : ButtonRuleLogic
    {
        public override bool Evaluate(InventoryManagementWindow window)
        {
            var view = window.GetActiveView();
            var source = view._source;
            return view != null &&
                   source != null &&
                   window.controller.AccessService.HasItems(source) &&
                   source.Type != InventorySourceType.UnitEquipment && 
                   source.Type != InventorySourceType.TransportEquipment;
        }
    }
}