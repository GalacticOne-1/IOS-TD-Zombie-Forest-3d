using Galactic1.Code.Gameplay.Equipment.Snapshots;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Inventory;

namespace Galactic1.Code.Systems.Raid.Survivors
{
    /// <summary>
    /// Raid-time snapshot for a survivor unit.
    ///
    /// Contains ONLY raid/save state:
    ///   — StatsSnapshot (current HP, etc.)
    ///   — InventorySnapshot
    ///   — EquipmentStateSnapshot
    ///
    /// Does NOT inherit UnitGameplayDefinition / UnitRuntimeDefinition.
    /// Gameplay systems (PhysicsPerception, MeleeAttack) are driven by
    /// GameplayDefinition, which is always valid and raid-independent.
    ///
    /// UnitInstance reads GameplayDefinition.
    /// RaidUnitRuntime reads both (GameplayDefinition + this snapshot).
    /// </summary>
    public sealed class RaidSurvivorSnapshot
    {
        // ── Identity ──────────────────────────────────────────────────────

        public string UnitId { get; }
        public string ArchetypeId { get; }
        public string DisplayName { get; }
        public bool IsHungry { get; }
        public bool IsThirsty { get; }

        // ── Gameplay config (always valid) ────────────────────────────────

        /// <summary>
        /// Immutable gameplay configuration.
        /// This is the object UnitInstance uses — not the snapshot.
        /// </summary>
        public SurvivorGameplayDefinition GameplayDefinition { get; }

        // ── Raid snapshot (raid-only state) ───────────────────────────────

        public SurvivorStatsSnapshot StatsSnapshot { get; }
        public InventoryDataBase InventoryData { get; }
        public InventorySnapshot EquipmentSnapshot { get; }
        public InventorySnapshot BackpackSnapshot { get; }
        public EquipmentSnapshot EquipmentStateSnapshot { get; }

        // ── Convenience passthrough (avoids call-site .GameplayDefinition.X) ──

        public PlayerBrainDefinition BrainDefinition => GameplayDefinition.BrainDefinition;

        // ── Ctor ──────────────────────────────────────────────────────────

        public RaidSurvivorSnapshot(
            string unitId,
            string archetypeId,
            string displayName,
            SurvivorGameplayDefinition gameplayDefinition,
            SurvivorStatsSnapshot statsSnapshot,
            InventoryDataBase inventoryData,
            InventorySnapshot equipmentSnapshot,
            InventorySnapshot backpackSnapshot,
            EquipmentSnapshot equipmentStateSnapshot)
        {
            UnitId = unitId;
            ArchetypeId = archetypeId;
            DisplayName = displayName;
            GameplayDefinition = gameplayDefinition
                                 ?? throw new System.ArgumentNullException(nameof(gameplayDefinition));
            StatsSnapshot = statsSnapshot;
            InventoryData = inventoryData;
            EquipmentSnapshot = equipmentSnapshot;
            BackpackSnapshot = backpackSnapshot;
            EquipmentStateSnapshot = equipmentStateSnapshot;
            
            
            IsHungry = statsSnapshot.CurrentStats[StatId.Hunger] <= 0;
            IsThirsty = statsSnapshot.CurrentStats[StatId.Thirst] <= 0;
        }
    }
}