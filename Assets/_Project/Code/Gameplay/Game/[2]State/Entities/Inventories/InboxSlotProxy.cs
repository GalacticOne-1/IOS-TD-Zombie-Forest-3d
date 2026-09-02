
namespace Galactic1.Code.Systems.Inbox
{
    public class InboxSlotProxy : InventorySlotProxy
    {
        public readonly string SlotId;
        
        /// <summary>
        /// Мировой час когда слот истечёт
        /// </summary>
        public readonly int ExpireWorldHour;
        
        public InboxSlotProxy(InboxSlotData data) : base(data)
        {
            SlotId = data.SlotId;
            ExpireWorldHour = data.ExpireWorldHour;
        }
    }
}