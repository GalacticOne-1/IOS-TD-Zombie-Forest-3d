using System;
using System.Linq;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Core.Systems.GameLoopSession;

namespace Galactic1.Code.Systems.Inbox
{
    /// <summary>
    /// Scene adapter для Inbox.
    ///
    /// Роль:
    /// • передаёт UI список входящих наград
    /// • выполняет получение награды
    /// • переносит награду в инвентарь игрока
    ///
    /// Runtime остаётся источником истины.
    /// </summary>
    public class InboxSceneAdapter : IFacilitySceneAdapter
    {
        private readonly IInboxFacilityRuntime _runtime;
        private readonly IInventoryResourcesPort _campPort, _transportPort;

        private InboxClaimDestination _destination;

        public FacilityType Type => _runtime.Type;
        
        
        public event Action OnStateChanged
        {
            add => _runtime.OnStateChanged += value;
            remove => _runtime.OnStateChanged -= value;
        }
        
        
        
        
        /*
         *  Конструктор создается при каждом открытии панели !!!
         */
        public InboxSceneAdapter(
            IInboxFacilityRuntime runtime,
            IInventoryResourcesPort camp,
            IInventoryResourcesPort transport,
            IInventoryResourcesPort raidTransport)
        {
            
            // === устанавливаем источник инвентаря при открытии панели ===
            var context = ServiceLocator.Current.Get<GameSession>().GameLoopContext;
            _destination = context.IsCampState
                ? InboxClaimDestination.Camp
                : InboxClaimDestination.Transport;
            
            
            _runtime = runtime;
            _campPort = camp;
            _transportPort = context.IsRaidState ? raidTransport : transport;
        }
        
        // =========================================================
        // DESTINATION
        // =========================================================

        private IInventoryResourcesPort DestinationPort
        {
            get
            {
                return _destination switch
                {
                    InboxClaimDestination.Camp => _campPort,
                    InboxClaimDestination.Transport => _transportPort,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        // =========================================================
        // CLAIM
        // =========================================================

        public bool TryClaimSlot(string slotId)
        {
            var slots = _runtime.Inbox.Slots;
            var slot = slots.FirstOrDefault(s => s.SlotId == slotId);

            if (slot == null)
                return false;

            return TryClaimSlot(slot);
        }


        /// <summary>
        /// Забрать награду из конкретного слота.
        /// Переносит весь stack.
        /// </summary>
        public bool TryClaimSlot(InboxSlotProxy slot)
        {
            if (slot == null)
                return false;

            var item = slot.Item.Value;

            if (item == null)
                return false;

            var runtimeSlot = new InventorySlotRuntime(
                item,
                slot.Amount.Value,
                slot.Durability.Value,
                slot.AmmoInMagazine.Value);
            
            var destination = DestinationPort;

            // Проверка вместимости
            if (!destination.CanAdd(runtimeSlot))
                return false;

            var result = destination.TryAdd(runtimeSlot);

            // перенос должен быть полностью
            if (result.Remaining > 0)
                return false;

            // удаляем из inbox
            _runtime.Inbox.RemoveSlot(slot);

            return true;
        }

        // =========================================================
        // CLAIM ALL
        // =========================================================

        /// <summary>
        /// Забрать все награды.
        /// Перенос происходит слотами.
        /// </summary>
        public void ClaimAll()
        {
            var slots =  _runtime.Inbox.Slots.ToList();

            foreach (var slot in slots)
            {
                TryClaimSlot(slot);
            }
        }

    }
    
    public enum InboxClaimDestination
    {
        Camp = 0,
        Transport = 1
    }
}