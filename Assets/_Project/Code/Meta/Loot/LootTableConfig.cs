using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    /// <summary>
    /// Таблица лута контейнера.
    /// Содержит слоты (slot-based) и отдельный guaranteed-слой.
    ///
    /// Migration note: старое поле Entries можно оставить временно
    /// и пометить [Obsolete] пока все конфиги не переведены на Slots.
    /// </summary>
    [CreateAssetMenu(
        fileName = "LootTableConfig",
        menuName = "Game Configs/Loot/Loot Table Config")]
    public class LootTableConfig : ScriptableObject
    {
        [SerializeField] private LootTableId _id;
        
        
        [Header("Guaranteed layer — генерируется всегда, ДО слотов")] [SerializeField]
        private LootGuaranteedEntry[] _guaranteedEntries;
        

        [Header("Slot-based generation")] [SerializeField]
        private LootSlotConfig[] _slots; // “Сколько раз мы кидаем кубик?”



        public LootTableId Id => _id;
        public LootSlotConfig[] Slots => _slots;
        public LootGuaranteedEntry[] GuaranteedEntries => _guaranteedEntries;
    }
}