
using Galactic1.Core.Systems.PlayerCreation;
using Galactic1.Gameplay.Player;
using Galactic1.Systems.Inventory;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    public class DragonEquipmentContainer : EquipmentContainer_old
    {
        [SerializeField] private DragonEquipmentInventoryData equipment;
        public override BaseInventoryData Inventory => equipment;

        private DragonWeaponBuilder _weaponBuilder;

        private void Awake()
        {
            // Создаём копию, чтобы у каждого игрока был свой экземпляр
            equipment = Instantiate(equipment);
            equipment.name = "DragonEquipmentInstance";
            equipment.Initialize(_equipmentContainerConfig);

            ServiceLocator.Current.Get<InventoryRepository>().RegisterDragon(null, this);
            
            _weaponBuilder = new DragonWeaponBuilder(GetComponent<IPlayerController>());
        }

        protected override (IInventoryContainer inventory, IInventoryContainer equipment) GetInventory()
            => (ServiceLocator.Current.Get<InventoryRepository>().DragonInventory, this);
        
        protected override WeaponBuilderBase GetWeaponBuilder() => _weaponBuilder;
    }
}