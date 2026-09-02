using Galactic1.Systems.Inventory;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    public class DragonInventoryContainer : InventoryContainer, IInventoryContainer
    {
        [SerializeField] private DragonInventoryData dragonInventory;

        public override BaseInventoryData Inventory => dragonInventory;

        protected override void Awake()
        {
            // Создаём копию, чтобы у каждого игрока был свой экземпляр
            dragonInventory = Instantiate(dragonInventory);
            dragonInventory.name = "DragonInventoryInstance";
            dragonInventory.Initialize();
            
            ServiceLocator.Current.Get<InventoryRepository>().RegisterDragon(this, null);
        }
    }
}