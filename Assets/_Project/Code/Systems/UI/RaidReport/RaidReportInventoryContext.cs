using System.Collections.Generic;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Sources;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Configs;

namespace Galactic1.Code.UI.RaidReport
{
    /// <summary>
    /// Создаёт и хранит временные инвентари для экрана отчёта рейда.
    /// Живёт только во время показа отчёта — создаётся при входе, уничтожается при выходе.
    /// </summary>
    public class RaidReportInventoryContext
    {
        // Слева: транспорт (реальный источник из рейда)
        public IInventorySource TransportSource { get; }

        // Справа: временный буфер лута (не сохраняется никуда)
        public IInventorySource LootBufferSource { get; }

        // Справа (вкладка дрон): временный инвентарь дрона
        public IInventorySource DroneBufferSource { get; }

        // Порт для проверки вместимости транспорта
        public IInventoryResourcesPort TransportPort { get; }

        public RaidReportInventoryContext(
            IInventorySource transportSource,
            IConfigProvider configs)
        {
            // === Левый источник: реальный инвентарь транспорта
            // Sources[1] = TransportCargo (как в TransportRuntime)
            TransportSource = transportSource;
            TransportPort = TransportSource as IInventoryResourcesPort;

            // === Правый источник: временный буфер лута
            LootBufferSource = BuildLootBuffer(configs);

            // === Источник дрона: пустой временный инвентарь (3 слота)
            DroneBufferSource = BuildDroneBuffer(configs);
        }

        // ─── Builders ────────────────────────────────────────────────

        private static IInventorySource BuildLootBuffer(IConfigProvider configs)
        {
            var inventoryData = configs.Get<CrateInventoryConfig>();

            
            // var l = loot.Count;
            // List<InventorySlotRuntime> slots = new();
            // for (int i = 0; i < l; i++)
            //     slots.Add(new InventorySlotRuntime(loot[i].Item, loot[i].Amount, loot[i].Durability));
            
            // === создаём snapshot
            var snapshot = InventorySnapshot.CreateFromLoot(new(), 25);

            return new SnapshotInventorySource(
                "RaidLootBuffer",
                null,
                snapshot,
                inventoryData,
                InventorySourceType.LootContainer
            );
        }

        private static IInventorySource BuildDroneBuffer(IConfigProvider configs)
        {
            var inventoryData = configs.Get<WorldMapDroneInventoryConfig>();
        
            // Всегда 3 пустых слота
            List<InventorySlotRuntime> slots = new();
            for (int i = 0; i < inventoryData.BaseCapacity; i++)
                slots.Add(new InventorySlotRuntime(null, 0, 0, 0));
        
            // === создаём snapshot
            var snapshot = InventorySnapshot.CreateFromLoot(slots, inventoryData.BaseCapacity);
        
            return new SnapshotInventorySource(
                "RaidDroneBuffer",
                null,
                snapshot,
                inventoryData,
                InventorySourceType.WorldMapDrone);
        }
    }
}