using System.Collections.Generic;
using Galactic1.Code.UI.Inventory;

namespace Galactic1.Systems.Inventory
{
    /// <summary>
    /// Центральное хранилище всех инвентарей и контейнеров
    /// </summary>
    public class InventoryRepository : IGameService
    {
        public PlayerInventoryContainer PlayerInventory { get; private set; }
        public PlayerEquipmentContainer PlayerEquipment { get; private set; }

        public DragonInventoryContainer DragonInventory { get; private set; }
        public DragonEquipmentContainer DragonEquipment { get; private set; }

        private List<IInventoryContainer> worldContainers = new(); // заменить на словарь с ключами по guid





        public void RegisterPlayer(PlayerInventoryContainer inv, PlayerEquipmentContainer equip)
        {
            PlayerInventory = inv ?? PlayerInventory;
            PlayerEquipment = equip ?? PlayerEquipment;
        }

        public void RegisterDragon(DragonInventoryContainer inv, DragonEquipmentContainer equip)
        {
            DragonInventory = inv ?? DragonInventory;
            DragonEquipment = equip ?? DragonEquipment;
        }

        public void RegisterWorldContainer(IInventoryContainer container)
        {
            if (!worldContainers.Contains(container))
                worldContainers.Add(container);
        }

        public IEnumerable<IInventoryContainer> GetWorldContainers() => worldContainers;



        public (PlayerInventoryContainer, PlayerEquipmentContainer) GetPlayerInventory()
            => (PlayerInventory, PlayerEquipment);

        public (DragonInventoryContainer, DragonEquipmentContainer) GetDragonInventory()
            => (DragonInventory, DragonEquipment);
    }
}