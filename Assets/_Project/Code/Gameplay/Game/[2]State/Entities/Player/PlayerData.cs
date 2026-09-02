using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Utility;

namespace Galactic1.Structs
{
    [System.Serializable]
    public class PlayerData
    {
        // --- Состояние игрока ---
        public string Id { get; set; }
        public bool IsDead { get; set; }
        public List<KeyValuePairSerializable<StatId, float>> Stats { get; set; }
        
        public int Level { get; set; }
        public int Experience { get; set; }
        
        public string Name { get; set; }
        public string ArchetypeId { get; set; }              // ConfigId архетипа → для получения префаба

        // --- Позиция игрока ---
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        // --- Инвентарь ---
        public List<InventorySlotData> Inventory { get; set; }

        // --- Экипировка ---
        public List<InventorySlotData> Equipment { get; set; }

        // --- Активные эффекты / баффы / статус ---
        public List<string> ActiveEffects { get; set; }

        // --- Версия сохранения ---
        public int SaveVersion { get; set; } = 1;
        
    }
}