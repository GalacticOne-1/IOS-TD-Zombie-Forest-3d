using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Configs;
using Galactic1.Items;
using ObservableCollections;
using R3;

namespace Galactic1.Structs
{
    public class TransportProxy
    {
        public readonly TransportData Origin;
        
        public string Id => Origin.Id;
        
        /// конфиг транспорта
        public ReactiveProperty<string> ConfigId { get; }
        
        // --- Состояние ---
        public ReactiveProperty<bool> IsUnlocked { get; }
        
        
        // --- Инвентарь ---
        public InventoryProxy InventoryProxy { get; }
        public ObservableList<InventorySlotProxy> InventorySlots { get; } = new();
        
        // --- Экипировка ---
        public InventoryProxy EquipmentProxy { get; } 
        public ObservableList<InventorySlotProxy> EquipmentSlots { get; } = new();
        
        
        
        

        //public ObservableList<ModuleSlotProxy> ModuleSlots { get; } = new();
        
        
        
        // --- Активные эффекты / баффы / статус ---
        
        
        
        
        

        public TransportProxy(TransportData data)
        {
            Origin = data;

            IsUnlocked = new(data.IsUnlocked);
            
            // modules
            ConfigId = new(data.ConfigId);
            
            // Создаём InventoryProxy
            InventoryProxy = new InventoryProxy(InventorySlots);
            EquipmentProxy = new InventoryProxy(EquipmentSlots);

            InitializeInventory();
            InitializeEquipment();
            //InitializeSlots();
            //BindProperties();
        }
        
        
        private void InitializeInventory()
        {
            foreach (var slot in Origin.Inventory)
            {
                GameContent.ResolveItem(slot.ItemKey, out slot.Item);
                InventorySlots.Add(new InventorySlotProxy(slot));
            }

            // синхронизация со списком Origin.Inventory
            InventorySlots.ObserveAdd().Subscribe(e => Origin.Inventory.Add(e.Value.Origin));
            InventorySlots.ObserveRemove().Subscribe(e =>
            {
                var removedSlotProxy = e.Value;
                var removedSlot = Origin.Inventory.FirstOrDefault(s =>
                    s.ItemKey == (removedSlotProxy.Item.Value?.Id.Guid ?? ""));
                Origin.Inventory.Remove(removedSlot);
            });
            InventorySlots.ObserveReplace().Subscribe(e =>
            {
                Origin.Inventory[e.Index] = e.NewValue.Origin;
                e.NewValue.BindToSave(e.NewValue.Origin);
            });
        }
        
        private void InitializeEquipment()
        {
            foreach (var slot in Origin.Equipment)
            {
                GameContent.ResolveItem(slot.ItemKey, out slot.Item);
                EquipmentSlots.Add(new InventorySlotProxy(slot));
            }

            // синхронизация со списком Origin.Inventory
            EquipmentSlots.ObserveAdd().Subscribe(e => Origin.Equipment.Add(e.Value.Origin));
            EquipmentSlots.ObserveRemove().Subscribe(e =>
            {
                var removedSlotProxy = e.Value;
                var removedSlot = Origin.Equipment.FirstOrDefault(s =>
                    s.ItemKey == (removedSlotProxy.Item.Value?.Id.Guid ?? ""));
                Origin.Equipment.Remove(removedSlot);
            });
            EquipmentSlots.ObserveReplace().Subscribe(e =>
            {
                Origin.Equipment[e.Index] = e.NewValue.Origin;
                e.NewValue.BindToSave(e.NewValue.Origin);
            });
        }

        void InitializeSlots()
        {
            // foreach (var slot in Origin.ModuleSlots)
            //     ModuleSlots.Add(new ModuleSlotProxy(slot));
            //
            // // подписка на добавление/удаление
            // ModuleSlots.ObserveAdd().Subscribe(e => Origin.ModuleSlots.Add(e.Value.Origin));
            // ModuleSlots.ObserveRemove().Subscribe(e => Origin.ModuleSlots.Remove(e.Value.Origin));
        }

        void BindProperties()
        {
            IsUnlocked.Skip(1).Subscribe(_ => Origin.IsUnlocked = _);
        }
    }

    // public class ModuleSlotProxy
    // {
    //     public readonly ModuleSlotData Origin;
    //     public ReactiveProperty<string> ModuleGuid { get; }
    //
    //     public ModuleSlotProxy(ModuleSlotData data)
    //     {
    //         Origin = data;
    //         ModuleGuid = new(data.ModuleGuid);
    //         ModuleGuid.Skip(1).Subscribe(_ => Origin.ModuleGuid = _);
    //     }
    // }
}