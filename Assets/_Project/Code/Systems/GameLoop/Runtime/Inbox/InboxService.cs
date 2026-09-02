
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.GameTime;

namespace Galactic1.Code.Systems.Inbox
{
    /// <summary>
    /// Сервис получения внешних наград.
    /// Используется:
    /// • IAP
    /// • ивенты
    /// • компенсации
    /// • админка
    /// </summary>
    public class InboxService : IGameService
    {
        private readonly InboxRuntime _runtime;
        private readonly GameTimeService _timeService;

        public InboxService(
            InboxRuntime runtime,
            GameTimeService timeService)
        {
            _runtime = runtime;
            _timeService = timeService;
        }
        
        
        /// <summary>
        /// Считает общее кол-во предмета в контейнере
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public int GetTotalAmount(RuntimeId itemId) => _runtime.GetTotalAmount(itemId);

        /// <summary>
        /// Добавление предмета
        /// </summary>
        /// <param name="configId"></param>
        /// <param name="amount"></param>
        /// <param name="durability">оставить -1 для полной прочности</param>
        public void AddReward(RuntimeId configId, int amount, int durability = -1, int ammoInMagazine = 0)
        {
            var item = GameContent.Items.Get(configId);
            
            int expire = _timeService.TotalWorldHours + 720;  // 30 day

            _runtime.AddReward(new InboxSlotData(
                item, 
                configId.Guid, 
                amount,
                durability == -1 ? item.Physical.maxDurability : durability,
                ammoInMagazine,
                expire));
        }

        /// <summary>
        /// Добавление предмета
        /// </summary>
        public void AddReward(InventorySlotRuntime slot)
            => AddReward(slot.Item.Id, slot.Amount, slot.Durability, slot.AmmoInMagazine);
    }
}