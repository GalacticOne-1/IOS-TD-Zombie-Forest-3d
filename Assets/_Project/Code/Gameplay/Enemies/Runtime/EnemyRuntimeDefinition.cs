
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Audio.Voice;
using Galactic1.Code.Gameplay.Enemies.Stats;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.Systems.Raid.Enemies
{
    /// <summary>
    /// Иммутабельное runtime-определение врага.
    ///
    /// Создаётся ТОЛЬКО через EnemyRuntimeFactory.BuildDefinition().
    /// Содержит полностью разрезолвленные данные:
    ///   — разрезолвленный визуальный вариант (Presentation.PrefabId)
    ///   — финальные статы с применёнными модификаторами
    ///   — все боевые, поведенческие и перцептивные параметры
    ///
    /// Сцен-слой НИКОГДА не читает ScriptableObject — только эту дефиницию.
    ///
    /// ПРАВИЛО: PrefabId берётся из Presentation.PrefabId, дублирование убрано.
    /// </summary>
    public sealed class EnemyRuntimeDefinition : UnitGameplayDefinition
    {
        public EnemyId EnemyId { get; }
        public string DisplayName { get; }
        
        public EnemyAIProfile AIProfile { get; }

        /// <summary>
        /// Визуальное представление: prefabId, аниматор, locomotion profile.
        /// Уже разрезолвлено EnemyVariantResolver — сцен-слой читает только это.
        /// PrefabId доступен через Presentation.PrefabId.
        /// </summary>
        public EnemyPresentationDefinition Presentation { get; }

        public EnemyAIDefinition BrainDefinition { get; }
        public MovementDefinition MovementDefinition { get; }
        public TargetingDefinition TargetingDefinition { get; }
        public EnemyCombatDefinition CombatDefinition { get; }
        public EnemyPackDefinition Pack { get; }

        /// <summary>Иммутабельный снапшот финальных статов (после модификаторов).</summary>
        public EnemyStatsSnapshot StatsSnapshot { get; }

        public float ThreatLevel { get; }
        public string LootTableId { get; }
        public bool IsElite { get; }

        /// <summary>
        /// Удобный прокси к Presentation.PrefabId.
        /// Оставлен для обратной совместимости — убедись что код постепенно
        /// мигрирует на Presentation.PrefabId напрямую.
        /// </summary>
        public string PrefabId => Presentation?.GameplayPrefabId;

        public EnemyRuntimeDefinition(
            EnemyId enemyId,
            string displayName,
            EnemyPresentationDefinition presentation,
            EnemyAIDefinition brainDefinition,
            MovementDefinition movementDefinition,
            PerceptionDefinition perceptionDefinition,
            TargetingDefinition targetingDefinition,
            EnemyCombatDefinition combatDefinition,
            MeleeCombatDefinition meleeCombatDefinition,
            EnemyPackDefinition pack,
            EnemyStatsSnapshot statsSnapshot,
            EnemyAIProfile aiProfile,
            VoiceAudioConfig voiceAudio,
            float threatLevel,
            string lootTableId,
            bool isElite,
            string prefabId) // параметр оставлен для совместимости, игнорируется — берётся из presentation
            : base(perceptionDefinition, meleeCombatDefinition, voiceAudio)
        {
            EnemyId = enemyId;
            DisplayName = displayName;
            Presentation = presentation;
            BrainDefinition = brainDefinition;
            MovementDefinition = movementDefinition;
            TargetingDefinition = targetingDefinition;
            CombatDefinition = combatDefinition;
            Pack = pack;
            StatsSnapshot = statsSnapshot;
            AIProfile = aiProfile;
            ThreatLevel = threatLevel;
            LootTableId = lootTableId;
            IsElite = isElite;
            // prefabId-параметр намеренно не используется: истина в Presentation.PrefabId
        }
    }
}