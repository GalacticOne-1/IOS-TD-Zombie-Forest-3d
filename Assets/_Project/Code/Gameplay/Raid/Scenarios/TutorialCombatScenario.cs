using Galactic1.Code.Core;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Core.GameSession;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Enemy;
using Galactic1.RaidLoot.Navigation;

namespace Galactic1.Code.Systems.Raid.Scenarios
{
    /// <summary>
    /// Chapter 2 combat tutorial — минимальный raid-сценарий: без wave/loot/ambient/
    /// exit-zones/defense-facilities/transport. Весь общий tactical pipeline
    /// (Combat/AI LOD/Spawn/Director/lifecycle) переиспользуется буквально из
    /// RaidInProgressState без единой правки — единственная точка полиморфизма
    /// (LocationType → сценарий) уже существует в RaidScenarioFactory.
    ///
    /// Ни одно из событий, которые слушает MissionObjectiveService (UnitKilledEvent/
    /// BuildingDestroyedEvent/WaveCompletedEvent/ExitReachedEvent/AllWavesCompletedEvent),
    /// в этой сцене не поднимается — MissionObjectiveService.Evaluate() органически
    /// никогда не вызывается, mission flow бездействует без специальной "заглушки".
    /// Завершение Chapter 2 целиком управляется TutorialService (terminal step графа
    /// Chapter 2) → ExitFromLocation(). EvaluateMission/ArePlayerForcesDestroyed
    /// существуют только чтобы удовлетворить контракт IRaidScenario — реально не
    /// вызываются, пока никто вручную не поднимет одно из пяти событий выше.
    /// </summary>
    public sealed class TutorialCombatScenario : IRaidScenario
    {
        private readonly DIContainer _container;
        private readonly GameLoopContext _gameLoopContext;
        private LootObstacleBuilder _obstacleBuilder;

        public EnemyAIProfile AIProfile => EnemyAIProfile.Raid;

        public ScenarioOptions Options { get; } = new ScenarioOptions
        {
            UseDefenseFacilities = false,
            UseWaveSpawner = false,
            UseAmbientPopulation = false,
            UseLoot = true,
            UseExitZones = false,
            UseTransport = false,
        };

        public TutorialCombatScenario(DIContainer container)
        {
            _container = container;
            _gameLoopContext = _container.Resolve<GameSession>().GameLoopContext;
        }

        public void Configure(RaidRuntime raid)
        {
            _gameLoopContext.CurrentRaid.DefenseFacilities = null;
            // Squad уже собран в RaidInProgressState.Enter() из context.TacticalSelectedUnits
            // (SelectTactical) — подтверждено: на старте Chapter 2 выбран ровно один юнит,
            // доп. фильтрации здесь не требуется.
        }

        public void OnSceneLoaded(SceneSessionDefinition scene)
        {
            var raid = _container.Resolve<GameSession>().GameLoopContext.CurrentRaid;

            if (raid.CurrentRaidLootContainer != null)
            {
                _obstacleBuilder = new LootObstacleBuilder(raid.CurrentRaidLootContainer);
                _obstacleBuilder.Build();
            }
        }

        public void OnBattleStarted() { }
        public void OnBattleFinished() { }
        public void Cleanup() { }
        public void ApplyResults() { }

        public void ExitFromLocation()
        {
            EventBus<HomeSceneRequestEvent>.Raise(new() { ResetRootPlayerScene = true });
        }

        public bool ArePlayerForcesDestroyed(MissionStateProvider state) => false;

        public MissionResult EvaluateMission(MissionContext context) => MissionResult.Running;

        public RaidResultProxy BuildRaidResult(RaidRuntime raid, MissionResult mission)
            => new(new RaidResultData
            {
                IsSuccess = true,
                KilledEnemies = 0,
                ExperienceGained = 0,
                LootReceived = new(),
                ResourcesLost = new(),
            });
    }
}
