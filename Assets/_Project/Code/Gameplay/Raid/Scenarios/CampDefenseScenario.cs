using System.Linq;
using Galactic1.Code.Core;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.CampDefense;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Gameplay.Enemies.Waves;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Sources;
using Galactic1.Code.Systems.Camp;
using Galactic1.Code.Systems.CampDefense.Navigation;
using Galactic1.Code.Systems.CampDefense.Penalty;
using Galactic1.Code.Systems.CampDefense.Preparation;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Interaction;
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.UI.Inventory;
using Galactic1.Configs;
using Galactic1.Core.GameSession;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Enemy;
using Galactic1.Game.Meta.Items;
using Galactic1.Meta.Configs.Recruitment;
using UnityEngine;

namespace Galactic1.Code.Systems.Raid.Scenarios
{
    public class CampDefenseScenario : IRaidScenario
    {
        private readonly DIContainer _container;
        private readonly GameLoopContext _gameLoopContext;
        private readonly CampDefensePreparationService _preparationService;
        private RaidCombatFacilityRuntime _campHqRuntime;
        private CampDefenseObstacleBuilder _obstacleBuilder;
        private WaveSystem _waveSystem;
        private WaveProgressRuntime _waveProgress;
        private readonly CampDefenseConfig _campDefenseConfig;
        
        
        public EnemyAIProfile AIProfile => EnemyAIProfile.Siege;
        public ScenarioOptions Options { get; } = new ScenarioOptions
        {
            UseDefenseFacilities = true,
            UseWaveSpawner = true,
            UseAmbientPopulation = false,
            UseLoot = false,
            UseExitZones = false,
            UseTransport = true,
        };


        

        public CampDefenseScenario(DIContainer container)
        {
            _container = container;

            _container.Resolve<InteractionPolicyService>()
                .Policy.CanInteractWithFacilities = false;

            _gameLoopContext = _container.Resolve<GameSession>().GameLoopContext;
            _preparationService = _container.Resolve<CampDefensePreparationService>();

            var configProvider = ServiceLocator.Current.Get<ConfigProvider>();
            _campDefenseConfig = configProvider.Get<CampDefenseConfig>();
        }

        
        
        
        public void Configure(RaidRuntime raid)
        {
            var configProvider = ServiceLocator.Current.Get<ConfigProvider>();
            var accessService = ServiceLocator.Current
                .Get<InventoryManagementWindow>()
                .controller
                .AccessService;

            // Snapshot инвентаря лагеря на момент начала Camp Defense
            // var campRuntime = _gameLoopContext.CampRuntime;
            // _gameLoopContext.CurrentRaid.CampInventoryRuntime = new RaidCampInventoryRuntime(
            //     new RaidCampInventorySource(
            //         campRuntime.inventoryConfig.GetInventoryId(StorageType.Regular),
            //         campRuntime,
            //         InventorySnapshot.CreateFromSource(campRuntime.GetInventory(StorageType.Regular), accessService),
            //         campRuntime.GetInventory(StorageType.Regular).InventoryData,
            //         null
            //     )
            // );
            _gameLoopContext.CurrentRaid.CampRuntime = 
                new RaidCampRuntime(_gameLoopContext.CampRuntime, accessService);
            
            
            
            // #1 спавним юниты вне отряда
            _gameLoopContext.CurrentRaid.CampDefenders = new CampDefenderRuntime(
                _gameLoopContext.CampUnits.ToList(),
                accessService,
                configProvider.Get<PlayerArchetypeConfig>(),
                configProvider.Get<SurvivorConsumptionConfig>());
            
            // #2 регистрация защитных объектов
            _gameLoopContext.CurrentRaid.DefenseFacilities =
                new RaidDefenseFacilityRegistry(_gameLoopContext.DefenseFacilities.ToList());

            // #2
            _campHqRuntime = _gameLoopContext.CurrentRaid.DefenseFacilities
                .GetFacility(FacilityType.CampHQ);

            if (_campHqRuntime != null)
            {
                // каждая защита лагеря начинается со 100% HP здания
                //_campHqRuntime.Stats.SetStat(StatId.Health, _campDefenseConfig.CampHpDefault);
                _campHqRuntime.OnDestroyed += OnCampHqDestroyed;
            }
            
            
        }

        public void OnSceneLoaded(SceneSessionDefinition scene)
        {
            // === активация штаба что бы зомби могли атаковать
            var facilities = _gameLoopContext.CurrentRaid.DefenseFacilities.Facilities;
            foreach (var f in facilities)
            {
                var b = ServiceLocator.Current.Get<BaseFacilityRepository>().TryGet(f.Id);
                b.instance?.Entity_Activate();
            }

            // === Navigation obstacles (аддитивно, только для Camp Defense) ===
            _obstacleBuilder = new CampDefenseObstacleBuilder(
                ServiceLocator.Current.Get<BaseFacilityRepository>());
            _obstacleBuilder.Build();


            // === Wave spawner ===
            var waveConfig = _container.Resolve<IConfigProvider>().Get<WaveConfig>();
            var waveSpawnPoints = scene.LocationContext.WaveSpawnPoints;

            _waveProgress = new WaveProgressRuntime();
            _gameLoopContext.CurrentRaid.WaveProgress = _waveProgress;

            _waveSystem = new WaveSystem(
                waveConfig,
                _container.Resolve<EnemySpawnSystem>(),
                _gameLoopContext.CurrentRaid.Enemies,
                new WaveSpawnPointResolver(waveSpawnPoints),
                _waveProgress,
                _container.Resolve<EnemySpawnSystem>());

            _gameLoopContext.CurrentRaid.WaveSystem = _waveSystem;
        }

        public void OnBattleStarted()
        {
            _waveSystem?.StartFirstWave();
        }

        public void OnBattleFinished()
        {
            
        }

        public void Cleanup()
        {
            _waveSystem?.Dispose();
            if (_gameLoopContext.CurrentRaid != null)
            {
                _gameLoopContext.CurrentRaid.WaveSystem = null;
                _gameLoopContext.CurrentRaid.WaveProgress = null;
            }

            _waveSystem = null;
            _waveProgress = null;

            _obstacleBuilder?.Dispose();
            _obstacleBuilder = null;

            if (_campHqRuntime != null)
            {
                _campHqRuntime.OnDestroyed -= OnCampHqDestroyed;

                // всегда восстанавливаем лагерь
                _campHqRuntime.Stats.SetStat(
                    StatId.Health, 
                    _campHqRuntime.Config.Item.BuildingHealth.Settings.maxHealth);
            }

            _preparationService.CompleteDefense();
        }

        public void ApplyResults()
        {
            var raid = _gameLoopContext.CurrentRaid;
            
            RaidResolvingPipeline.Resolve(
                raid.CampDefenders.Units,
                _gameLoopContext.CampUnits,
                u => u.Proxy.Id
            );
            
            // 🔹 defense facility
            RaidResolvingPipeline.Resolve(
                raid.DefenseFacilities.Facilities,
                _gameLoopContext.DefenseFacilities,
                f => f.Id);
            
            RaidResolvingPipeline.Resolve(
                raid.CampRuntime,
                _gameLoopContext.CampRuntime,
                t => t.inventoryConfig.GetInventoryId(StorageType.Regular)
            );

            new CampFacilityPostResolveService(
                    _gameLoopContext,
                    _container.Resolve<IFacilityRuntimeService>())
                .Execute();
            
        }

        private void OnCampHqDestroyed()
        {
            // Camp Defense сам не завершается — только сообщает о факте через
            // существующий pipeline статуса миссии (MissionObjectiveService слушает это событие)
            EventBus<BuildingDestroyedEvent>.Raise(new BuildingDestroyedEvent
            {
                IsHeadquarters = true
            });
        }

        public void ExitFromLocation()
        {
            EventBus<HomeSceneRequestEvent>.Raise(new() { ResetRootPlayerScene = true });
        }

        public bool ArePlayerForcesDestroyed(MissionStateProvider state)
        {
            return state.AreCampDefendersDestroyed();
        }

        public MissionResult EvaluateMission(MissionContext context)
        {
            var result = new MissionResult();

            if (context.PlayerForcesDestroyed || context.HeadquartersDestroyed)
            {
                result = MissionResult.Defeat;
                _container.Resolve<GameSession>().GameLoopContext.CurrentRaid.Status = RaidStatus.Failed;
                result.EndReason = RaidEndReason.ObjectivesCompleted;

#if UNITY_EDITOR
                var s = "";
                if (context.PlayerForcesDestroyed) s = "all units killed";
                if (context.HeadquartersDestroyed) s = "HQ destroyed";
                Debug.LogError($"Mission Defeat: " + s);
#endif
            }
            else if (context.AllWavesCompleted)
            {
                result = MissionResult.Victory;
                _container.Resolve<GameSession>().GameLoopContext.CurrentRaid.Status = RaidStatus.Completed;
                result.EndReason = RaidEndReason.ObjectivesCompleted;

#if UNITY_EDITOR
                Debug.LogError($"Mission Victory: all zombies killed");
#endif
            }
            else
                return MissionResult.Running;

            return result;
        }

        public RaidResultProxy BuildRaidResult(RaidRuntime raid, MissionResult mission)
        {
            bool isSuccess = mission.Status == MissionStatus.Victory;
            int killedEnemies = 0;

            int experienceGained = raid.CalculateExperience(isSuccess, killedEnemies);
            
            // === штраф за поражение в Camp Defense
            raid.PenaltyResult = _container.Resolve<CampDefenseFailureService>().Evaluate(_gameLoopContext);
            
            // === маппим штраф в persisted-формат для отчёта (аналог LootResultMapper)
            var penaltyMapper = new CampDefensePenaltyResultMapper();
            var resourcesLost = penaltyMapper.Map(raid.PenaltyResult);
            
            

            return new RaidResultProxy(new RaidResultData
            {
                IsSuccess = isSuccess,
                KilledEnemies = killedEnemies,
                ExperienceGained = experienceGained,
                LootReceived = new(),
                MainBuildingDestroyed = _campHqRuntime?.IsDestroyed ?? false,
                ResourcesLost = resourcesLost
            });
        }
    }
}