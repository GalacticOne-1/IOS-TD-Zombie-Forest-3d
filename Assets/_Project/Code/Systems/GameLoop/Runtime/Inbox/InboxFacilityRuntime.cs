using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Runtime объекта Inbox в лагере.
    ///
    /// Роль:
    /// • точка доступа игрока к Inbox
    /// • предоставляет слоты InboxRuntime
    /// • устанавливает базовый объем инвентаря
    /// </summary>
    public class InboxFacilityRuntime : 
        StorageFacilityRuntime, 
        IInboxFacilityRuntime
    {
        public override FacilityType Type => FacilityType.MainContainer;
        /// <summary>
        /// Runtime входящих наград
        /// </summary>
        public InboxRuntime Inbox { get; }

        public override bool CanUpgrade => false;

        public InboxFacilityRuntime(
            FacilityProxy proxy,
            StorageModule module,
            InboxRuntime inbox,
            CampRuntime campRuntime,
            GameTimeService timeService)
            : base(proxy, module, campRuntime, timeService)
        {
            Inbox = inbox;
            
            Inbox.OnInboxChanged += MarkStateChanged;
        }

        public override void Dispose()
        {
            Inbox.OnInboxChanged -= MarkStateChanged;
        }
    }
}