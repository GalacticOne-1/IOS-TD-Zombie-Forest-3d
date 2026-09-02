
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Configs;
using Galactic1.Items;
using ObservableCollections;
using R3;

namespace Galactic1.Structs
{
    public class PlayerProxy
    {
        public readonly PlayerData Origin;
        
        
        // --- Состояние игрока ---
        public string Id => Origin.Id;
        public ReactiveProperty<bool> IsDead { get; }
        
        public Dictionary<StatId, ReactiveProperty<float>> Stats { get;}
        
        
        
        public ReactiveProperty<int> Level { get; }
        public ReactiveProperty<int> Experience { get; }
        
        public ReactiveProperty<string> Name { get; }
        public string ArchetypeId => Origin.ArchetypeId;
        
        // --- Позиция игрока ---
        public ReactiveProperty<float> PosX { get; }
        public ReactiveProperty<float> PosY { get; }
        public ReactiveProperty<float> PosZ { get; }

        // --- Инвентарь ---
        public InventoryProxy InventoryProxy { get; }
        public ObservableList<InventorySlotProxy> InventorySlots { get; } = new();

        // --- Экипировка ---
        public InventoryProxy EquipmentProxy { get; }
        public ObservableList<InventorySlotProxy> EquipmentSlots { get; } = new();

        // --- Активные эффекты / баффы / статус ---
        public ReactiveProperty<List<string>> ActiveEffects { get; }

        // --- Версия сохранения ---
        public ReactiveProperty<int> SaveVersion { get; }


        
        
        public PlayerProxy(PlayerData playerData)
        {
            Origin = playerData;    

            // R3
            IsDead = new(Origin.IsDead);
            Stats = new();
            Level = new(Origin.Level);
            Experience = new(Origin.Experience);
            Name = new(Origin.Name);
            
            PosX = new(Origin.PosX);
            PosY = new(Origin.PosY);
            PosZ = new(Origin.PosZ);
            
            // Создаём InventoryProxy
            InventoryProxy = new InventoryProxy(InventorySlots);
            EquipmentProxy = new InventoryProxy(EquipmentSlots);
            
            
            // subscription
            InitializeStats();
            InitializeInventory(); // здесь будет пусто !
            InitializeEquipment(); // снаряга загружается здесь
        }


        void InitializeStats()
        {
            Origin.Stats.ForEach(s => Stats[s.Key] = new(s.Value));
            
            
            IsDead.Skip(1).Subscribe(_ => Origin.IsDead = _);
            foreach (var stat in Stats)
            {
                // Находим индекс элемента с нужным ключом
                int index = Origin.Stats.FindIndex(s => s.Key == stat.Key);

                if (index >= 0)
                {
                    stat.Value.Skip(1).Subscribe(_ =>
                    {
                        var kv = Origin.Stats[index];
                        kv.Value = _;
                        Origin.Stats[index] = kv; // Обновляем элемент в списке
                    });
                }
            }
            
            Level.Skip(1).Subscribe(_ => Origin.Level = _);
            Experience.Skip(1).Subscribe(_ => Origin.Experience = _);
            Name.Skip(1).Subscribe(_ => Origin.Name = _);
            
            PosX.Skip(1).Subscribe(_ => Origin.PosX = _);
            PosY.Skip(1).Subscribe(_ => Origin.PosY = _);
            PosZ.Skip(1).Subscribe(_ => Origin.PosZ = _);
        }
        
        // Инвентаря пока нет !!!
        void InitializeInventory()
        {
            foreach (var slot in Origin.Inventory) // всегда пустой !
            {
                GameContent.ResolveItem(slot.ItemKey, out slot.Item);
                InventorySlots.Add(new InventorySlotProxy(slot));
            }

            
            // при добавлении нового слота в игре, связываем его с сохранением
            InventorySlots.ObserveAdd().Subscribe(e => Origin.Inventory.Add(e.Value.Origin));
            
            // так же при удалении удаляем сохранение
            InventorySlots.ObserveRemove().Subscribe(e =>
            {
                var removedSlotProxy = e.Value;
                var removedSlot = Origin.Inventory.FirstOrDefault(s =>
                    s.ItemKey == (removedSlotProxy.Item.Value?.Id.Guid ?? ""));
                Origin.Inventory.Remove(removedSlot);
            });
            InventorySlots.ObserveReplace().Subscribe(e =>
            {
                // Меняем в сохранении строго по индексу
                Origin.Inventory[e.Index] = e.NewValue.Origin;

                // Перепривязываем Item/Amount
                e.NewValue.BindToSave(e.NewValue.Origin);
            });
        }
        
        // Слоты снаряжения юнита
        void InitializeEquipment()
        {
            foreach (var slot in Origin.Equipment)
            {
                GameContent.ResolveItem(slot.ItemKey, out slot.Item);
                EquipmentSlots.Add(new InventorySlotProxy(slot));
            }

            
            // при добавлении нового слота в игре, связываем его с сохранением
            EquipmentSlots.ObserveAdd().Subscribe(e => Origin.Equipment.Add(e.Value.Origin));
            
            // так же при удалении удаляем сохранение
            EquipmentSlots.ObserveRemove().Subscribe(e =>
            {
                var removedSlotProxy = e.Value;
                var removedSlot = Origin.Equipment.FirstOrDefault(s =>
                    s.ItemKey == (removedSlotProxy.Item.Value?.Id.Guid ?? ""));
                Origin.Equipment.Remove(removedSlot);
            });
            
            EquipmentSlots.ObserveReplace().Subscribe(e =>
            {
                // Меняем в сохранении строго по индексу
                Origin.Equipment[e.Index] = e.NewValue.Origin;

                // Перепривязываем Item/Amount
                e.NewValue.BindToSave(e.NewValue.Origin);
            });
        }
    }
}