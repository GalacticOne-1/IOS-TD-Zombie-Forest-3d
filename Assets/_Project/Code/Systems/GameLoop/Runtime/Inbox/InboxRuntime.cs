using System;
using System.Linq;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Game.Camp.Proxy;
using ObservableCollections;
using R3;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Runtime системы входящих наград (Inbox).
    /// Не является инвентарём — только буфер получения.
    /// </summary>
    public class InboxRuntime
    {
        private readonly BaseProxy _proxy;
        private readonly GameTimeService _timeService;

        public ObservableList<InboxSlotProxy> Slots => _proxy.InboxSlots;

        public event Action OnInboxChanged;

        public InboxRuntime(BaseProxy proxy, GameTimeService timeService)
        {
            _proxy = proxy;
            _timeService = timeService;

            Slots.ObserveChanged().Subscribe(_ =>
            {
                OnInboxChanged?.Invoke();
            });
            
            //_timeService.HoursPassed += OnHoursPassed;

            //CleanupExpired();
        }
        
        private void OnHoursPassed(int hours, TimeAdvanceReason reason)
        {
            CleanupExpired();
        }

        private void CleanupExpired()
        {
            int now = _timeService.TotalWorldHours;

            var expired = Slots
                .Where(s => s.ExpireWorldHour <= now)
                .ToList();

            foreach (var slot in expired)
            {
                Slots.Remove(slot);
            }
        }
        
        
        /// <summary>
        /// Считает общее кол-во предмета
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public int GetTotalAmount(RuntimeId itemId)
        {
            int total = 0;

            var l = Slots.Count;
            for (int i = 0; i < l; i++)
            {
                var slot = Slots[i];

                if (slot.IsEmpty)
                    continue;

                if (slot.Item.Value.Id == itemId)
                    total += slot.Amount.Value;
            }

            return total;
        } 
        

        /// <summary>
        /// Добавляет награду во входящие.
        /// Всегда создаётся новый слот.
        /// </summary>
        public void AddReward(InboxSlotData slot)
        {
            var proxy = new InboxSlotProxy(slot);
            proxy.BindToSave(slot);

            Slots.Add(proxy);
            DLog.Alert($"Inbox add >> {slot.Item.Header.titleLid} / {slot.Amount}", EDlogColor.YELLOW); 
        }

        /// <summary>
        /// Удаляет слот после получения награды
        /// </summary>
        public void RemoveSlot(InboxSlotProxy slot)
        {
            Slots.Remove(slot);
            DLog.Alert($"Inbox remove >> {slot.Item.Value.Header.titleLid} / {slot.Amount.Value}", EDlogColor.YELLOW); 
        }
    }
}