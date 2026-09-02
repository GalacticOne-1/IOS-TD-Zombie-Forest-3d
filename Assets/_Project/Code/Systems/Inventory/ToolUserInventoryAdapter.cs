
using Galactic1.Core.Enums;
using Galactic1.Items;
using Galactic1.Code.UI.Inventory;
using Gameplay.Gameplay.Interaction;

namespace Gameplay.Inventory
{
    /// <summary>
    /// Отвечает за выбор, использование и авто-замену инструмента.
    /// Работает поверх InventoryProxy.
    /// </summary>
    public class ToolUserInventoryAdapter
    {
        private readonly PlayerInventoryContainer inventoryContainer;
        private readonly EquipmentContainer_old equipmentContainer;
        
        

        public ToolUserInventoryAdapter(PlayerInventoryContainer inventoryContainer, EquipmentContainer_old equipmentContainer)
        {
            this.inventoryContainer = inventoryContainer;
            this.equipmentContainer = equipmentContainer;
        }

        
        /// <summary>
        /// Ищет подходящий инструмент нужного класса и уровня.
        /// Возвращает индекс слота или null.
        /// Выбирает лучший по уровню.
        /// </summary>
        public (int?, bool inEquipment) GetBestToolFor(IToolRequirement requirement)
        {
            var neededType = requirement.RequiredToolType;

            int? bestSlot = null;
            int bestLevelFound = -1;
            bool equipment = false;

            for (int i = 0; i < 2; i++)
            {
                InventoryProxy proxy = null;// i == 0 
                    //? inventoryContainer.Inventory.InventoryProxy 
                    //: equipmentContainer.Inventory.InventoryProxy; 

                for (int s = 0; s < proxy.Slots.Count; s++)
                {
                    var slot = proxy.Slots[s];
                    if (slot.IsEmpty)
                        continue;

                    var item = slot.Item.Value;

                    bool success = false;
                    foreach (var type in neededType)
                    {
                        if (item.GetEquipClass() == type)
                            success = true;
                    }

                    if (!success) 
                        continue;
                    

                    // выбираем лучший уровень инструмента
                    // if (item.Level > bestLevelFound)
                    // {
                    //     equipment = i == 1;
                    //     bestSlot = s;
                    //     bestLevelFound = item.Level;
                    // }
                }

            }

            return (bestSlot, equipment);
        }


        /// <summary> Уменьшить прочность — как в LDOE </summary>
        public bool OnItemUsed(BaseInventoryData inventory, int slotIndex)
        {
            bool broken = false;
            var slotProxy = inventory.InventoryProxy.Slots[slotIndex];
            slotProxy.Durability.Value--;

            if (slotProxy.Durability.Value <= 0)
            {
                HandleItemBroken(inventory, slotIndex);
                broken = true;
            }
            
            inventory.OnChanged?.Invoke();
            return broken;
        }

        /// <summary>
        /// Удаляем предмет из инвентаря после поломки
        /// </summary>
        /// <param name="slotIndex"></param>
        private void HandleItemBroken(BaseInventoryData inventory, int slotIndex)
        {
            var slotProxy = inventory.InventoryProxy.Slots[slotIndex];
            if (!slotProxy.IsEmpty)
            {
                var item = slotProxy.Item.Value;
                var equipClass = item.GetEquipClass();
            
                // здесь можно вызывать попап о сломанном предмете
                // ...
            
                slotProxy.Clear();
                equipmentContainer.ClearVisual(EquipSlotType.Weapon);
            }
        }

        public void BindVisual(BaseInventoryData inventory, int slotIndex)
        {
            equipmentContainer.BindVisual(
                EquipSlotType.Weapon, 
                inventory.InventoryProxy.Slots[slotIndex].Item.Value);
        }

        public void ClearVisual()
        {
            var weaponSlot = equipmentContainer.Inventory.FindSlotIndex(EquipmentSlotType.WeaponMain);
            
            // если слот оружия пуст, просто удаляем визуал тулза
            if (equipmentContainer.Inventory.InventoryProxy.Slots[weaponSlot.Value].IsEmpty)
            {
                equipmentContainer.ClearVisual(EquipSlotType.Weapon);
            }

            // иначе восстанавливаем предмет из слоте 
            else
            {
                equipmentContainer.BindVisual(
                    EquipSlotType.Weapon,
                    equipmentContainer.Inventory.InventoryProxy.Slots[weaponSlot.Value].Item.Value);
            }
        }
    }
}
