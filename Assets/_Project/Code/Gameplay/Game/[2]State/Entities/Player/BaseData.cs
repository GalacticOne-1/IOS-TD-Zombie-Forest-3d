using System.Collections.Generic;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Game.Camp.Proxy
{
    [System.Serializable]
    public class BaseData
    {
        // --- Инвентарь базы ---
        public Dictionary<StorageType, List<InventorySlotData>> StorageInventories { get; set; } = new();
        
        /// <summary>
        /// Входящие награды (Inbox)
        /// </summary>
        public List<InboxSlotData> Inbox = new();

        // --- Верстаки / Производственные объекты ---
        public List<FacilityData> Buildings { get; set; } 

        // --- Защитные объекты базы ---
        //public List<DefenseData> Defenses { get; set; } = new();
    }

    
}