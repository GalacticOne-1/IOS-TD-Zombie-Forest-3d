using ObservableCollections;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    [CreateAssetMenu(menuName = "Game Configs/Inventory/Crate Inventory Data")]
    public class HomeInventoryData : BaseInventoryData
    {


        public override void Initialize(Object data = null)
        {
            // ! заменить на реальное прокси !
            InventoryProxy = new(new ObservableList<InventorySlotProxy>());

            if (InventoryProxy.Slots.Count == 0)
            {
                for (int i = 0; i < baseCapacity; i++)
                    InventoryProxy.Slots
                        .Add(new InventorySlotProxy(new InventorySlotData(null, "", 0, 0, 0)));
            }
        }

    }
}