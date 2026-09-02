using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Code.Systems.Raid.Survivors
{
    /// <summary>
    /// Подготавливает immutable definition бойца для рейда.
    /// </summary>
    public sealed class RaidSurvivorFactory
    {
        public RaidSurvivorSnapshot Create(
            UnitRuntime runtime,
            InventoryAccessService access,
            PlayerArchetypeConfig playerCfg)
        {
            //var statsDefault = ServiceLocator.Current.Get<ConfigProvider>().Get<PlayerStatsBase>();
            var baseStats = new Dictionary<StatId, float>();

            foreach (var s in runtime.Stats.GetBaseStats)
                baseStats[s.Key] = s.Value;

            var currentStats = new Dictionary<StatId, float>();

            foreach (var s in runtime.Stats.CurrentStats_)
                currentStats[s.Key] = s.Value.Value;

            var statsSnapshot = new SurvivorStatsSnapshot(
                baseStats,
                currentStats);

            var inventoryDataBase = 
                runtime.Sources[0].InventoryData;

            var equipmentSnapshot =
                InventorySnapshot.CreateFromSource(
                    runtime.Sources[0],
                    access);

            // var backpackSnapshot =
            //     InventorySnapshot.CreateFromSource(
            //         runtime.Sources[1],
            //         access);

            var equipmentState =
                runtime.EquipmentService.CreateReadonlySnapshot();
            
            
            var perception = new PerceptionDefinition(
                playerCfg.Perception.detectionRadius,
                playerCfg.Perception.updateInterval,
                playerCfg.Perception.hearingRadius,
                playerCfg.Perception.hearingSensitivity);

            var melee = new MeleeCombatDefinition(
                playerCfg.Combat.AttackRange,
                playerCfg.Combat.HitRange,
                playerCfg.Combat.Damage,
                playerCfg.Combat.Cooldown,
                playerCfg.Combat.ReadyToAttackAngle);

            var brainSettings = new PlayerBrainDefinition(
                playerCfg.Brain.autoEngageRange,
                playerCfg.Brain.autoCoverRange,
                playerCfg.Brain.reEngageDelay);
            
            var gameplayDefinition = new SurvivorGameplayDefinition(
                perception,
                melee,
                brainSettings,
                playerCfg.VoiceAudio);

            // ─────────────────────────────────────────────
            // Raid snapshot
            // ─────────────────────────────────────────────
            return new RaidSurvivorSnapshot(
                runtime.Proxy.Id,
                runtime.Proxy.ArchetypeId,
                runtime.DisplayName,
                gameplayDefinition,
                statsSnapshot,
                inventoryDataBase,
                equipmentSnapshot,
                null,
                equipmentState);
        }
    }
}