
using System.Collections.Generic;
using Galactic1.Code.Core;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.AI.LOD;
using Galactic1.Code.Gameplay.Combat;
using Galactic1.Code.Gameplay.Enemies.Waves;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.CampDefense.Penalty;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Code.Systems.Raid.Scenarios;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.WorldMap.Definitions;
using Galactic1.RaidLoot.Runtime;
using Galactic1.RaidLoot.Scene.Lifecycle;
using Galactic1.RaidLoot.Services;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Runtime-модель активного рейда.
    /// Живёт только во время миссии (локации).
    /// Создаётся в RaidLaunchingState
    /// Используется TacticalSubStateMachine и всеми рейдовыми системами.
    /// </summary>
    public sealed class RaidRuntime
    {
        // =========================
        // Identity
        // =========================

        /// <summary>
        /// Уникальный идентификатор рейда (для аналитики / лога / результата).
        /// </summary>
        public int RaidId { get; set; }

        /// <summary>
        /// Id локации, в которой проходит рейд.
        /// </summary>
        public LocationId Id { get; set; }

        public LocationDefinition LocationDef { get; set; }
        
        public IRaidScenario Scenario { get; set; }
        
        public WaveSystem WaveSystem { get; set; }
        public WaveProgressRuntime WaveProgress { get; set; }
        // =========================
        // State
        // =========================

        /// <summary>
        /// Текущий статус рейда (Launching / InProgress / Extraction / Completed / Failed).
        /// </summary>
        public RaidStatus Status { get; set; } = RaidStatus.Launching;

        /// <summary>
        /// Текущий под-стейт тактического слоя.
        /// Используется TacticalSubStateMachine.
        /// </summary>
        public TacticalPhase TacticalPhase { get; set; } = TacticalPhase.None;


        // =========================
        // Time & Pressure (Aliens-like)
        // =========================

        /// <summary>
        /// Сколько времени прошло с начала рейда (в секундах).
        /// Игрок может не видеть это напрямую.
        /// </summary>
        public float ElapsedTime { get; set; }

        /// <summary>
        /// Уровень угрозы (0..1+).
        /// Используется Alien Director.
        /// </summary>
        public float ThreatLevel { get; set; }

        /// <summary>
        /// Флаг, что рейд вошёл в режим эскалации.
        /// </summary>
        public bool EscalationStarted { get; set; }


        // =========================
        // Core Runtime Systems
        // =========================

        /// <summary>
        /// Runtime-состояние отряда (потери, стресс, ранения).
        /// </summary>
        public SquadRuntime Squad { get; set; }
        public CampDefenderRuntime CampDefenders { get; set; }
        public RaidCampRuntime CampRuntime { get; set; }
        public RaidVehicleRuntime PlayerTransport { get; set; }
        public RaidDefenseFacilityRegistry DefenseFacilities { get; set; }
        public RaidEnemyRegistry Enemies { get; set; }
        
        public CombatRuntime Combat { get; set; }
        public AILODSystem AILOD { get; set; }
        public EnemySceneLifecycleSystem CurrentRaidLifecycle { get; set; }
        public LootContainerSceneLifecycleSystem CurrentRaidLootContainer { get; set; }
        
        

        // Loot
        public RaidLootBuffer LootBuffer { get; set; }
        public RaidLootEconomyState EconomyState { get; set; }
        public LootNormalizationService LootNormalizer { get; set; }
        
        // Camp Defense — штраф за поражение (аналог LootBuffer, но для потерь).
        // Заполняется в CampDefenseScenario.ApplyResults(), читается в BuildRaidResult().
        public CampDefensePenaltyResult PenaltyResult { get; set; } = CampDefensePenaltyResult.Empty;


        // =========================
        // Outcome
        // =========================

        /// <summary>
        /// Итоговый результат рейда.
        /// Заполняется перед выходом из рейда.
        /// </summary>
        public MissionResult MissionResult { get; set; }


        // =========================
        // Helpers
        // =========================

        /// <summary>
        /// Рейд считается активным и управляемым.
        /// </summary>
        public bool IsActive =>
            Status == RaidStatus.InProgress ||
            Status == RaidStatus.Extraction;

        /// <summary>
        /// Рейд завершён и больше не должен тикаться.
        /// </summary>
        public bool IsFinished =>
            Status == RaidStatus.Completed ||
            Status == RaidStatus.Failed;



        public void Tick(float dt)
        {
            if (IsFinished)
                return;

            ElapsedTime += dt;

            Squad?.Tick(dt);
            CampDefenders?.Tick(dt);
            Enemies?.Tick(dt);
            AILOD?.Tick(dt);
            DefenseFacilities?.Tick(dt);
            WaveSystem?.Tick(dt);
        }
        
        


        /// <summary>
        /// Рассчитывает финальный результат рейда.
        /// Вызывается ОДИН раз при завершении тактического слоя.
        /// </summary>
        // public RaidResultProxy CalculateResult()
        // {
        //     bool isSuccess = true;
        //     int killedEnemies = 0;
        //
        //     // ── Loot: читаем из буфера ────────────────────────────────────
        //     var mapper = new LootResultMapper();
        //     var lootReceived = LootBuffer != null
        //         ? mapper.Map(LootBuffer)
        //         : new List<RaidRewardLootData>();
        //     // ─────────────────────────────────────────────────────────────
        //
        //     int experienceGained = CalculateExperience(isSuccess, killedEnemies, Squad);
        //
        //     return new RaidResultProxy(new RaidResultData
        //     {
        //         IsSuccess = isSuccess,
        //         KilledEnemies = killedEnemies,
        //         ExperienceGained = experienceGained,
        //         LootReceived = lootReceived
        //     });
        // }


        public int CalculateExperience(
            bool isSuccess,
            int killedEnemies)
        {
            return 10;
            int exp = 0;

            // базовый опыт за участие
            exp += 50;

            // за убийства
            exp += killedEnemies * 5;

            // бонус за успех
            if (isSuccess)
                exp += 100;

            // штраф за потери
            exp -= Squad.CasualtiesCount * 20;

            return exp < 0 ? 0 : exp;
        }
    }
}
