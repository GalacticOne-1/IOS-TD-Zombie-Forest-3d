
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// Использовать для всех инвентарей в локациях
    /// </summary>
    public class OuterContainer : InventoryContainer
    {
        [SerializeField] protected BaseInventoryData inventory;
        
        public override BaseInventoryData Inventory => inventory;
        
        protected override void Awake()
        {
            // Создаём копию, чтобы у каждого игрока был свой экземпляр
            inventory = Instantiate(inventory);
            inventory.name = "OuterCrateInventoryInstance";
            inventory.Initialize();
        }
    }
}