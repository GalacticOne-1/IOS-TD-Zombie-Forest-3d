
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Configs;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using ObservableCollections;
using R3;

namespace Galactic1.Game.Camp.Proxy
{
    /// <summary>
    /// Proxy базы — сериализуемое состояние, полностью через R3.
    /// Содержит инвентарь, верстаки и защитные объекты.
    /// </summary>
    public class BaseProxy
    {
        public readonly BaseData Origin;

        // --- Инвентарь ---
        /// <summary>
        /// Один инвентарь на все хранилища
        /// </summary>
        public Dictionary<StorageType, InventoryProxy> StorageInventories { get; } = new();
        
        public ObservableList<InboxSlotProxy> InboxSlots { get; } = new();

        // --- Верстаки / Производственные объекты ---
        public ObservableList<FacilityProxy> Buildings { get; } = new();

        // --- Защита базы ---
        //public ObservableList<DefenseProxy> Defenses { get; } = new();


        
        
        
        
        public BaseProxy(BaseData data)
        {
            Origin = data;

            InitializeInbox();
            InitializeInventory();
            InitializeBuildings();
        }


        #region Inbox

        private void InitializeInbox()
        {

            var originSlots = Origin.Inbox;

            foreach (var slot in originSlots)
            {
                GameContent.ResolveItem(slot.ItemKey, out slot.Item);

                var proxySlot = new InboxSlotProxy(slot);
                proxySlot.BindToSave(slot);

                InboxSlots.Add(proxySlot);
            }

            BindInbox(originSlots);
        }
        
        private void BindInbox(List<InboxSlotData> originList)
        {
            InboxSlots.ObserveAdd().Subscribe(e =>
            {
                originList.Add((InboxSlotData)e.Value.Origin);
            });

            InboxSlots.ObserveRemove().Subscribe(e =>
            {
                originList.Remove((InboxSlotData)e.Value.Origin);
            });

            InboxSlots.ObserveReplace().Subscribe(e =>
            {
                originList[e.Index] = (InboxSlotData)e.NewValue.Origin;
                e.NewValue.BindToSave(e.NewValue.Origin);
            });
        }
        
        #endregion


        #region Inventory

        private void InitializeInventory()
        {
            var pairs = Origin.StorageInventories.ToList();

            foreach (var pair in pairs)
            {
                var type = pair.Key;
                var savedSlots = pair.Value;

                // восстановление Item ссылок
                foreach (var slot in savedSlots)
                {
                    GameContent.ResolveItem(slot.ItemKey, out slot.Item);
                }

                // создание proxy
                GetOrCreateInventory(type);
            }
        }
        
        
        
        public InventoryProxy GetOrCreateInventory(StorageType type, int capacity = 0)
        {
            if (StorageInventories.TryGetValue(type, out var proxy))
                return proxy;

            if (!Origin.StorageInventories.ContainsKey(type))
                Origin.StorageInventories[type] = new List<InventorySlotData>();

            var originSlots = Origin.StorageInventories[type];

            var slots = new ObservableList<InventorySlotProxy>();
            proxy = new InventoryProxy(slots);

            StorageInventories[type] = proxy;

            // --- сначала создаём proxy слоты
            if (originSlots.Count == 0)
            {
                for (int i = 0; i < capacity; i++)
                {
                    var data = new InventorySlotData(null, "", 0, 0, 0);

                    originSlots.Add(data);

                    var proxySlot = new InventorySlotProxy(data);
                    proxySlot.BindToSave(data);

                    slots.Add(proxySlot);
                }
            }
            else
            {
                foreach (var slot in originSlots)
                {
                    var proxySlot = new InventorySlotProxy(slot);
                    proxySlot.BindToSave(slot);

                    slots.Add(proxySlot);
                }
            }

            // 🔴 подписываемся ТОЛЬКО после инициализации
            BindInventory(proxy, originSlots);

            return proxy;
        }
        
        private void BindInventory(
            InventoryProxy proxy,
            List<InventorySlotData> originList)
        {
            proxy.Slots.ObserveAdd().Subscribe(e =>
                originList.Add(e.Value.Origin));

            proxy.Slots.ObserveRemove().Subscribe(e =>
            {
                // var removed = originList.FirstOrDefault(s =>
                //     s.ItemKey == (e.Value.Item.Value?.ConfigId ?? ""));
                //
                // if (removed != null)
                //     originList.Remove(removed);
                originList.Remove(e.Value.Origin);
            });

            proxy.Slots.ObserveReplace().Subscribe(e =>
            {
                originList[e.Index] = e.NewValue.Origin;
                e.NewValue.BindToSave(e.NewValue.Origin);
            });
        }
        

        #endregion
        
        
        // private void InitializeInventory()
        // {
        //     var itemDatabase = ServiceLocator.Current.Get<ConfigProvider>().Get<ItemDatabase>();
        //     foreach (var slot in Origin.Inventory)
        //     {
        //         slot.Item = string.IsNullOrEmpty(slot.ItemKey) ? null : itemDatabase.GetItemByGuid(slot.ItemKey);
        //         InventorySlots.Add(new InventorySlotProxy(slot));
        //     }
        //
        //     // синхронизация со списком Origin.Inventory
        //     InventorySlots.ObserveAdd().Subscribe(e => Origin.Inventory.Add(e.Value.Origin));
        //     InventorySlots.ObserveRemove().Subscribe(e =>
        //     {
        //         var removedSlotProxy = e.Value;
        //         var removedSlot = Origin.Inventory.FirstOrDefault(s =>
        //             s.ItemKey == (removedSlotProxy.Item.Value?.ItemKey ?? ""));
        //         Origin.Inventory.Remove(removedSlot);
        //     });
        //     InventorySlots.ObserveReplace().Subscribe(e =>
        //     {
        //         Origin.Inventory[e.Index] = e.NewValue.Origin;
        //         e.NewValue.BindToSave(e.NewValue.Origin);
        //     });
        // }


        void InitializeBuildings()
        {
            // #2 buildings
            // все структуры в лагере связываем с масссивом в прокси для синхроницации
            Origin.Buildings.ForEach(entityData => Buildings.Add(new FacilityProxy(entityData)));
           
            // для добавления
            Buildings.ObserveAdd().Subscribe(e =>
            {
                var addedEntity = e.Value;
                Origin.Buildings.Add(addedEntity.Origin as FacilityData);
                DLog.Alert($"New building {addedEntity.UniqueId}");
            });
          
            // для удаления
            Buildings.ObserveRemove().Subscribe(e =>
            {
                var removedId = e.Value.UniqueId;
                Origin.Buildings.RemoveAll(b => b.UniqueId == removedId);
                DLog.Alert($"Remove building {removedId}");
            });
        }

        
    }
}
