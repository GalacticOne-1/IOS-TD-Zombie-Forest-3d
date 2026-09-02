using Galactic1.Code.GameDatabase;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Items;

namespace Galactic1.Code.Systems.ProductionPipeline
{
    /// <summary>
    /// Pipeline обработки завершённых production-заказов.
    /// Отвечает за:
    /// • авто-сбор
    /// • fallback (оставить в слоте)
    /// </summary>
    public sealed class AutoCollectPipeline
    {
        private readonly StorageRegistry _storage;
        private readonly IInventoryResourcesPort _inventory;
        private readonly ItemDatabase _items;

        public AutoCollectPipeline(
            StorageRegistry storage,
            IInventoryResourcesPort inventory,
            ItemDatabase items)
        {
            _storage = storage;
            _inventory = inventory;
            _items = items;

            EventBus<ProductionOrderCompletedEvent>
                .Register(new EventBinding<ProductionOrderCompletedEvent>(OnOrderCompleted));
        }

        private void OnOrderCompleted(ProductionOrderCompletedEvent e)
        {
            if (!GameContent.Items.TryGet(e.RecipeId, out var item))
                return;

            // Stage 1: Storage validation
            if (!_storage.HasStorageForAnyTag(item.Tags))
                return;

            // Stage 2: Capacity check (ВАЖНО — без partial!)
            var slot = new InventorySlotRuntime(item, e.Amount, item.Physical.maxDurability, 0);

            if (!_inventory.CanAdd(slot))
                return;

            // Stage 3: Commit
            var result = _inventory.TryAdd(slot);

            if (result.Remaining > 0)
            {
                // теоретически не должно произойти
                return;
            }
            
            EventBus<ProductionOrderAutoCollectedEvent>.Raise(
                new ProductionOrderAutoCollectedEvent
                {
                    JobId = e.JobId,
                    StationId = e.StationId,
                    RecipeId = e.RecipeId,
                    Orders = e.Orders,
                    Amount = e.Amount
                });

            // Stage 4: Notify runtime (опционально)
            // можно через отдельный event:
            // ProductionOrderAutoCollectedEvent
        }
    }
}