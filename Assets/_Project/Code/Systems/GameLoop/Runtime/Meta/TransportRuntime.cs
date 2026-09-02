using System.Collections.Generic;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Sources;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Configs;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.World.StartLocation;
using Galactic1.Structs;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Runtime для техники в отряде/бою
    /// </summary>
    public class TransportRuntime : 
        InventoryOwnerRuntime, 
        IEquipmentStateListener, 
        ITransportRuntime
    {
        public readonly TransportProxy Proxy;
        
        public IInventorySource GetInventory => _sources[1];


        public string Id => Proxy.Id;
        public string ConfigId => Proxy.ConfigId.Value;
        public ItemConfig Item { get; private set; }
        
        public string GetPrefab()
        {
            if (!GameContent.ResolveItem(ConfigId, out var config))
            {
                DLog.Alert($"[TransportRuntime] Missing config: {ConfigId}", EDlogColor.RED);
                return null;
            }

            return config.PrefabPath;
        }
        
        
        public bool HasOverflow(int newCapacity)
            => GetInventory.HasOverflow(newCapacity);
        public List<InventorySlotRuntime> GetOverflowItems(int newCapacity)
            => GetInventory.GetOverflowItems(newCapacity);


        public TransportRuntime(TransportProxy proxy)
        {
            Proxy = proxy;
            var configProvider = ServiceLocator.Current.Get<ConfigProvider>();

            GameContent.ResolveItem(ConfigId, out var item);
            Item = item;
            
            // обновляем вместимость под первую машину
            // для создания инвентаря с правильным объемом
            var startingTransport = configProvider.Get<WorldStartConfig>().Transport;
            var inventoryConfig = configProvider.Get<TransportInventoryConfig>();
            inventoryConfig.UpdateCapacity(startingTransport.Vehicle.CargoCapacity);
            
            RegisterInventorySource(new InventoryProxySourceAdapter(
                $"VehicleEquip",
                this,
                configProvider.Get<TransportInventoryEquipmentConfig>(),
                Proxy.EquipmentProxy,
                InventorySourceType.TransportEquipment,
                this));

            RegisterInventorySource(new InventoryProxySourceAdapter(
                $"VehicleStorage",
                this,
                inventoryConfig,
                Proxy.InventoryProxy,
                InventorySourceType.TransportCargo,
                null));
        }
        
        
        public void ApplyConfig(ItemConfig vehicleItem)
        {
            Item = vehicleItem;
            var module = vehicleItem.GetModule<VehicleModule>();

            SetCapacity(module.CargoCapacity);
        }
       
        /// <summary>
        /// For changing capacity
        /// </summary>
        /// <param name="capacity"></param>
        void SetCapacity(int capacity)
        {
            if (GetInventory is InventoryProxySourceAdapter adapter)
            {
                adapter.SetCapacity(capacity);
            }
        }
        
        
        
        #region Invetory Equipment

        public bool Equip(int slotIndex)
        {
            return false; //EquipmentService.Equip(slotIndex);
        }

        public void Unequip(int slotIndex)
        {
            //EquipmentService.Unequip(slotIndex);
        }
        

        #endregion

        // Дополнительно: методы для боевой логики, модификаторов и т.д.
    }
}