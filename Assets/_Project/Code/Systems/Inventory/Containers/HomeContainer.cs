

using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// Использовать для объектов игрока
    /// </summary>
    public class HomeContainer : InventoryContainer
    {
        [SerializeField] protected BaseInventoryData inventory;
        
        public override BaseInventoryData Inventory => inventory;
        
        protected override void Awake()
        {
            // Создаём копию, чтобы у каждого игрока был свой экземпляр
            inventory = Instantiate(inventory);
            inventory.name = "CrateInventoryInstance";
            inventory.Initialize();
        }
    }
}