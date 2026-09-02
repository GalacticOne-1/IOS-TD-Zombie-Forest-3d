using Galactic1.Systems.Inventory;
using Galactic1.UI;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    public class PlayerInventoryContainer : InventoryContainer, IInventoryContainer
    {
        [SerializeField] private PlayerInventoryData playerInventory;

        public override BaseInventoryData Inventory => playerInventory;

        protected override void Awake()
        {
            // Создаём копию, чтобы у каждого игрока был свой экземпляр
            playerInventory = Instantiate(playerInventory);
            playerInventory.name = "PlayerInventoryInstance";
            playerInventory.Initialize();
            
            ServiceLocator.Current.Get<InventoryRepository>().RegisterPlayer(this, null);
        }
    }
}