using System;
using System.Collections.Generic;

namespace Galactic1.Structs
{
    [Serializable]
    public class TransportData
    {
        public string Id { get; set; }
        public string ConfigId { get; set; }
        
        public bool IsUnlocked { get; set; }                
        
        
        // --- Инвентарь транспорта ---
        public List<InventorySlotData> Inventory { get; set; } = new();
        
        // --- Экипировка ---
        public List<InventorySlotData> Equipment { get; set; } = new();
    }
}