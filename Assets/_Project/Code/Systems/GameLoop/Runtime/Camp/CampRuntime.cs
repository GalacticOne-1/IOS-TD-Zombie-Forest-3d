
using System.Linq;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Sources;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Configs;
using Galactic1.Game.Camp.Proxy;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Runtime-класс базы. Хранит ссылки на Proxy и runtime-объекты верстаков/защиты.
    /// </summary>
    public class CampRuntime : InventoryOwnerRuntime
    {
        public readonly BaseProxy Proxy;
        private readonly GameTimeService _timeService;
        public readonly CampStorageInventoryConfig inventoryConfig;

        public CampRuntime(
            BaseProxy proxy,
            ConfigProvider configProvider,
            GameTimeService timeService)
        {
            Proxy = proxy;
            _timeService = timeService;
            inventoryConfig = configProvider.Get<CampStorageInventoryConfig>();
            
            // базовое хранилище лагеря
            //EnsureInventory(StorageType.Regular, proxy.Buildings[0]); // todo index first storage !!!
            // RegisterInventorySource(new InventoryProxySourceAdapter(
            //     "CampStorage",
            //     this,
            //     ServiceLocator.Current.Get<ConfigProvider>().Get<CampStorageInventoryConfig>(),
            //     Proxy.InventoryProxy,
            //     InventorySourceType.BaseStorage,
            //     null));
        }

        // =========================================================
        // INVENTORY API
        // =========================================================

        public IInventorySource GetInventory(StorageType type)
        {
            type = StorageType.Regular;// ! удалить, если нужно сделать разные инвентари для категорий !
            
            string id = inventoryConfig.GetInventoryId(type);

            return _sources.FirstOrDefault(s => s.SourceId == id);
        }

        
        /*
         *  инвентарь создается один раз при создании первого хранилища -> #2
         *  дальше каждое новое хранилище получает этот инвентарь и устанавливает вместимость -> #1
         */
        public IInventorySource EnsureInventory(StorageModule module)
        {
            var type = StorageType.Regular; // ! удалить, если нужно сделать разные инвентари для категорий !
            // сейчас все категории меняются на обычный инвентарь
            
            
            // #1 получение существующего инвентаря
            var existing = GetInventory(type);
            if (existing != null)
                return existing;

            // #2 создание нового инвентаря
            var proxy = Proxy.GetOrCreateInventory(type, module.Capacity); //  inventoryConfig.BaseCapacity

            var source = new InventoryProxySourceAdapter(
                inventoryConfig.GetInventoryId(type),
                this,
                inventoryConfig,
                proxy,
                InventorySourceType.BaseStorage,
                null);

            RegisterInventorySource(source);

            return source;
        }
        
        
        public IInventorySource RegisterStorage(StorageModule module)
        {
            return EnsureInventory(module);
        }
        

        public void RegisterAndResizeStorage(StorageModule module)
        {
            var inventory = RegisterStorage(module);

            if (inventory is InventoryProxySourceAdapter adapter)
            {
                var newCapacity = adapter.GetSlots().Count + module.Capacity;
                adapter.SetCapacity(newCapacity);
            }
        }
        
        public void UnregisterStorage(StorageModule module)
        {
            var inventory = GetInventory(module.StorageType);

            if (inventory is InventoryProxySourceAdapter adapter)
            {
                adapter.SetCapacity(adapter.GetSlots().Count - module.Capacity);
            }
        }
    }
}