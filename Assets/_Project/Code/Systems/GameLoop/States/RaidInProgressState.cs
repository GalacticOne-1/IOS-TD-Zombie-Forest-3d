using System.Collections.Generic;
using System.Linq;
using DEV;
using Galactic1.Code.Cameras;
using Galactic1.Code.Core;
using Galactic1.Code.Data.Combat;
using Galactic1.Code.Gameplay.AI.LOD;
using Galactic1.Code.Gameplay.AoE;
using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.Audio.Grenades;
using Galactic1.Code.Gameplay.Audio.Weapons;
using Galactic1.Code.Gameplay.Combat;
using Galactic1.Code.Gameplay.Combat.Burst;
using Galactic1.Code.Gameplay.Combat.Hit;
using Galactic1.Code.Gameplay.Combat.Resolvers;
using Galactic1.Code.Gameplay.Combat.Suppression;
using Galactic1.Code.Gameplay.Combat.Visual;
using Galactic1.Code.Gameplay.Enemies.Definitions;
using Galactic1.Code.Gameplay.Enemies.Factories;
using Galactic1.Code.Gameplay.Enemies.Modifiers;
using Galactic1.Code.Gameplay.Enemies.Repositories;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Gameplay.Enemies.Spawning.Positioning;
using Galactic1.Code.Gameplay.Enemies.Variants;
using Galactic1.Code.Gameplay.Noise;
using Galactic1.Code.Gameplay.RaidDirector;
using Galactic1.Code.Systems.Enemies;
using Galactic1.Code.Systems.GameLoop.Tactical;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Code.Systems.Raid.Scenarios;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Code.Systems.Squad;
using Galactic1.Code.Systems.Survival;
using Galactic1.Code.Systems.UI;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.UI.RaidLoot;
using Galactic1.Code.WorldMap;
using Galactic1.Code.WorldMap.Definitions;
using Galactic1.Configs;
using Galactic1.Core.Systems.Factories;
using Galactic1.Core.Systems.GameSession;
using Galactic1.Core.UI;
using Galactic1.Gameplay.Locations;
using Galactic1.Meta.Configs.Recruitment;
using Galactic1.PoolObject;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definitions;
using Galactic1.RaidLoot.Events;
using Galactic1.RaidLoot.Runtime;
using Galactic1.RaidLoot.Scene.Lifecycle;
using Galactic1.RaidLoot.Services;
using Galactic1.RaidLoot.Systems;

namespace Galactic1.Code.Systems.GameLoop.States
{
    /// <summary>
    /// Состояние кор-лупа: Tactical слой рейда в локации.
    /// Подготовка Squad завершена, начинается пошаговый бой / симуляция рейда.
    ///
    /// Инициализирует весь spawn pipeline:
    ///   EnemySpawnSystem → EnemySpawnPipeline → EnemySceneLifecycleSystem → ZombieFactory → EnemyInstance
    ///
    /// Общий тактический пайплайн одинаков для всех типов рейда (exploration, camp defense, ...).
    /// Поведенческие различия инкапсулированы в RaidRuntime.Scenario (IRaidScenario) —
    /// это состояние не содержит условий по типу рейда.
    /// </summary>
    public class RaidInProgressState : GameLoopStateBase
    {
        public override GameLoopState Id => GameLoopState.RaidInProgress;

        public RaidInProgressState(DIContainer container) : base(container){}

        public override void Enter(GameLoopContext context)
        {
            base.Enter(context);
            DLog.Alert("RaidInProgressState enter", AppConstants.show_log_core);

            var configProvider = ServiceLocator.Current.Get<ConfigProvider>();
            var locationConfig = configProvider.Get<LocationsConfigs>()
                .Locations[context.Proxy.CurrentLocationStateId.Value];
            var accessService = ServiceLocator.Current.Get<InventoryManagementWindow>().controller.AccessService;

            SelectTactical(context);

            var locationDefinition = new LocationDefinitionBuilder().Build(locationConfig);
            
            // #1 Создаём Runtime рейда
            context.CurrentRaid = new RaidRuntime
            {
                Status = RaidStatus.Launching,
                RaidId = 999,
                Id = locationConfig.Id,
                LocationDef = locationDefinition,
                Squad = new SquadRuntime(
                    context.TacticalSelectedUnits.ToList(),
                    accessService,
                    configProvider.Get<PlayerArchetypeConfig>(),
                    configProvider.Get<SurvivorConsumptionConfig>()),
                PlayerTransport = new RaidVehicleRuntime(context.PlayerTransport, accessService),
                Enemies = new(),

                // * юниты и объекты лагеря добавляются в сценарии
                CampDefenders = null,
                DefenseFacilities = null
            };
            
            
            // #1.5 Создаём и подключаем сценарий — единственная точка полиморфизма по типу рейда
            var scenario = RaidScenarioFactory.Create(locationConfig.LocationType, _container);
            context.CurrentRaid.Scenario = scenario;
            scenario.Configure(context.CurrentRaid);

            // Подключаем статы к прокси игрока
            context.RebindDisplayUnitsForRaid(context.CurrentRaid.Squad.Units);
            new UIStatsController().Register(_container);
            foreach (var unit in context.CurrentRaid.Squad.Units)
                unit.BindInventoryPreview(accessService);

            // #2 Создаём и регистрируем spawn pipeline
            // Порядок: сначала pipeline (он только данные), затем lifecycle (он создаёт сцен-объекты)
            var spawnSystem = BuildSpawnSystem(context.CurrentRaid, configProvider);
            _container.RegisterInstance(spawnSystem);

            // Threat Director
            var director = BuildDirector(context.CurrentRaid, spawnSystem, configProvider);
            _container.RegisterInstance(director);

            // #3 систсема урона в локации
            context.CurrentRaid.Combat = BuildCombatRuntime(configProvider);

            // #3.5 AI LOD — централизованная система "спящих зомби", только для рейда
            if (scenario.Options.UseAmbientPopulation)
            {
                context.CurrentRaid.AILOD = BuildAILODSystem(context.CurrentRaid, configProvider);
                _container.RegisterInstance(context.CurrentRaid.AILOD);
            }


            // #4 Создаём lifecycle — подписывается на Raid.Enemies до SetSceneReady
            context.CurrentRaid.CurrentRaidLifecycle = new EnemySceneLifecycleSystem(
                _container.Resolve<ZombieFactory>(),
                context.CurrentRaid,
                ServiceLocator.Current.Get<EnemyRepository>());

            context.CurrentRaid.CurrentRaidLifecycle.SetSceneReady();

            // #5 полоска здоровья для врагов
            var healthBarSystem = new EnemyHealthBarSystem(
                ServiceLocator.Current.Get<PoolManager>(),
                _container.Resolve<UIRootView>().canvas,
                configProvider.Get<ObjectPoolConfigs>().UnitIndicatorWidgetConfig
            );
            _container.RegisterInstance(healthBarSystem);
            
            
            
            // #6 сервис для проверки статуса миссии
            var missionService = new MissionObjectiveService(
                context,
                context.CurrentRaid,
                new MissionStateProvider(
                    context.CurrentRaid,
                    context));
            _container.RegisterInstance(missionService);
            DevUpdate.I.missionObjectiveService = missionService; // *** для простого доступа к тестам

            // === подписываем событие проверки миссии 
            EventBus<MissionCompletedEvent>
                .Register(new EventBinding<MissionCompletedEvent>(OnMissionCompleted));
            
            
            
            
            // ============= ============= ============= ============= ============= ============= =============
            // === по готовности сцены спавним сущности и лут
            // это событие идет когда сцена готова!!
            ServiceLocator.Current.Get<SceneSessionManager>().OnSceneLoaded += () =>
            {
                var sceneSession = ServiceLocator.Current.Get<SceneSessionManager>().Session;
                var options = context.CurrentRaid.Scenario.Options;

                // ── Enemies (ambient) ─────────────────────────────────────────────
                // Общая система, включается сценарием через Options — сценарий её не создаёт.
                if (options.UseAmbientPopulation)
                {
                    var ambientPopulation = new AmbientEnemyPopulationSystem(
                        spawnSystem,
                        sceneSession.LocationContext.AmbientSpawnPoints);
                    _container.RegisterInstance(ambientPopulation);
                }

                // ── Loot ─────────────────────────────────────────────────────────
                // Общая система, включается сценарием через Options — тело метода не менялось.
                if (options.UseLoot)
                    BuildLootSystem(
                        context.CurrentRaid,
                        sceneSession.LocationContext,
                        configProvider);

                // ── Exit Zones ───────────────────────────────────────────────────
                if (options.UseExitZones)
                {
                    var exitZoneManager = new ExitZoneManager(context, missionService);
                    _container.RegisterInstance(exitZoneManager);
                }

                // ── Scenario-специфичные системы, которых нет в общем пайплайне ────
                // (например turrets/camp buildings у Camp Defense)
                context.CurrentRaid.Scenario.OnSceneLoaded(sceneSession);

                // DEV полигон
                if (DEV_polygon.I)
                    DEV_polygon.I.LoadPolygon(_container);

                // Создаём Sub-StateMachine для боя
                context.TacticalStateMachine = new TacticalSubStateMachine();
                context.TacticalStateMachine.Setup(
                    _container,
                    _context,
                    new List<ITacticalState>
                    {
                        new SUB_RaidStartState(_container),
                        new SUB_RaidActiveState(_container),
                        new SUB_RaidEventsState(),
                        new SUB_RaidCheckObjectivesState(),
                        new SUB_RaidCleanupState(sceneSession.LocationContext.LocationLootProfile)
                    });


                // Запускаем рейд ─────────────────────────────────────────────────────────
                context.TacticalStateMachine.ChangeState<SUB_RaidStartState>();
            };
        }

        /// <summary>
        /// Вызывается когда все тактические состояния завершились.
        /// Только сообщает кор-лупу о завершении и передаёт итог.
        /// </summary>
        public void OnRaidFinished(RaidResultProxy result)
        {
            if (_context == null || _context.CurrentRaid == null) return;

            _context.CurrentRaid.Scenario.OnBattleFinished();

            var sm = _container.Resolve<GameLoopStateMachine>();
            sm.ChangeState(GameLoopState.RaidResolving);
        }

        public override void Exit(GameLoopContext context)
        {
            DLog.Alert("RaidInProgressState exit", EDlogColor.YELLOW, AppConstants.show_log_core);

            var options = context.CurrentRaid.Scenario.Options;

            // Очищаем spawn system при выходе из рейда
            _container.Resolve<MissionObjectiveService>().Dispose();
            _container.Resolve<EnemySpawnSystem>().Clear();
            _container.Resolve<RaidDirectorRuntime>().Dispose();

            // Общие системы освобождаются здесь же, по тем же Options, которыми они были включены.
            if (options.UseExitZones)
                _container.Resolve<ExitZoneManager>().Dispose();

            // Сценарий освобождает только то, что сам создал в OnSceneLoaded (turrets и т.п.)
            context.CurrentRaid.Scenario.Cleanup();

            context.TacticalStateMachine = null;
        }

        
        /// <summary>
        /// Единственное место для заверешение миссии
        /// <br/>(Для любого сценария и локации)
        /// </summary>
        /// <param name="e"></param>
        private void OnMissionCompleted(MissionCompletedEvent e)
        {
            EventBus<MissionCompletedEvent>.Clear();
            
            _context.CurrentRaid.MissionResult = e.Result;
            
            _context.TacticalStateMachine.ChangeState(e.NextState);
        }
        
        
        
        // ── Приватные методы ──────────────────────────────────────────

        /// <summary>
        /// Собирает и связывает все компоненты spawn pipeline.
        /// Возвращает готовый EnemySpawnSystem.
        /// </summary>
        private EnemySpawnSystem BuildSpawnSystem(RaidRuntime raid, ConfigProvider configProvider)
        {
            var archetypeCache = new EnemyArchetypeDefinitionCache(
                new EnemyArchetypeDefinitionBuilder(),
                configProvider);

            var pipeline = new EnemySpawnPipeline(
                new EnemyVariantResolver(),
                new EnemyPresentationFactory(),
                new EnemyModifierPipeline(new EnemyModifierDatabase(configProvider)),
                new EnemyRuntimeDefinitionBuilder(),
                new EnemyRuntimeFactory(),
                new EnemySpawnPointResolver(),
                archetypeCache,
                raid);

            return new EnemySpawnSystem(pipeline);
        }


        private RaidDirectorRuntime BuildDirector(
            RaidRuntime raid,
            EnemySpawnSystem spawnSystem,
            ConfigProvider configProvider)
        {
            var config = configProvider.Get<DirectorConfig>();

            var spawnResolver = new DirectorSpawnResolver(
                config,
                ServiceLocator.Current.Get<CameraController>().Camera);

            var director = new RaidDirectorRuntime(
                config,
                spawnSystem,
                raid.Enemies,
                ServiceLocator.Current.Get<NoiseSystem>(),
                spawnResolver);

            return director;
        }


        private void BuildLootSystem(
            RaidRuntime raid,
            LocationContext locationContext,
            ConfigProvider configProvider)
        {
            // ── Инфраструктура ───────────────────────────────────────────────
            var lootRepository = ServiceLocator.Current.Get<LootContainerRepository>();
            var lootBuffer = new RaidLootBuffer();
            var balanceProfile = configProvider.Get<LootBalanceProfile>();
            var depletionCurve = configProvider.Get<DepletionCurveConfig>();
            var currentDay = ServiceLocator.Current.Get<GameTimeService>().CurrentDay;

            // ── Population: SpawnPoints → Runtime → Repository ───────────────
            var lootFactory = new LootContainerFactory(lootRepository);
            var lootPopulation = new LootPopulationSystem(lootFactory, locationContext.LootSpawnPoints);
            lootPopulation.Initialize();

            // ── Raid-wide economy state — создаётся до нормализатора ─────────
            var economyState = new RaidLootEconomyState();
            var normalizer = new LootNormalizationService(balanceProfile, economyState);

            // ── Services ─────────────────────────────────────────────────────
            var openService = new LootContainerOpenService(lootRepository);
            var depletion = new ContainerDepletionService(depletionCurve, currentDay);

            var generationService = new LootGenerationService(
                lootRepository,
                locationContext.LocationLootProfile,
                balanceProfile,
                depletionCurve,
                depletion,
                normalizer, // ← normalizer передаётся в оба сервиса
                raid.Id,
                currentDay);

            // ── Location Guaranteed: один раз ДО контейнеров ─────────────────
            var locationGuaranteedService = new LocationGuaranteedLootGenerationService(
                locationContext.LocationGuaranteedProfile,
                locationContext.LocationLootProfile,
                normalizer, // ← тот же экземпляр normalizer
                raid.Id,
                currentDay);

            locationGuaranteedService.Generate(lootBuffer);

            var autoPickup = new LootAutoPickupService(lootRepository, lootBuffer);

            // ── Scene Lifecycle ───────────────────────────────────────────────
            raid.CurrentRaidLootContainer = new LootContainerSceneLifecycleSystem(
                lootFactory,
                openService,
                configProvider.Get<LootContainerVisualDatabase>());
            raid.CurrentRaidLootContainer.SetSceneReady(locationContext.LootSpawnPoints);

            var lootContainers = raid.CurrentRaidLootContainer.Containers;
            var lootRewardsPanel = ServiceLocator.Current.Get<LootRewardsWorldSystem>();
            lootRewardsPanel.Initialize(lootContainers);
            ServiceLocator.Current.Get<ContainerProgressWorldSystem>().Initialize(lootContainers);

            // ── EventBus ─────────────────────────────────────────────────────
            EventBus<ContainerOpenedEvent>.Register(
                new EventBinding<ContainerOpenedEvent>(generationService.OnContainerOpened));
            EventBus<LootGeneratedEvent>.Register(
                new EventBinding<LootGeneratedEvent>(autoPickup.OnLootGenerated));
            EventBus<ContainerLootCollectedEvent>.Register(
                new EventBinding<ContainerLootCollectedEvent>(lootRewardsPanel.OnLootCollected));

            // ── Сохраняем для Cleanup ─────────────────────────────────────────
            raid.LootBuffer = lootBuffer;
            raid.EconomyState = economyState;
            raid.LootNormalizer = normalizer;

            _container.RegisterInstance(lootRepository);
            _container.RegisterInstance(lootBuffer);
            _container.RegisterInstance(raid.CurrentRaidLootContainer);

            // ── Очистка событий ──────────────────────────────────────────────
            EventBus<SceneServicesClearEvent>.Register(new EventBinding<SceneServicesClearEvent>(() =>
            {
                EventBus<ContainerOpenedEvent>.Clear();
                EventBus<LootGeneratedEvent>.Clear();
                EventBus<ContainerLootCollectedEvent>.Clear();
            }));
        }


        private CombatRuntime BuildCombatRuntime(ConfigProvider configProvider)
        {
            // ── Gameplay stack (unchanged) ────────────────────────────────────────
            var surfaceResolver = new SurfaceResolver();
            var bodyPartResolver = new BodyPartResolver();
            var hitResolver = new HitResolver(surfaceResolver, bodyPartResolver);
            var burstResolver = new BurstFireResolver();
            var suppressionConfig = configProvider.Get<SuppressionConfig>();
            var suppressionSystem = new SuppressionSystem(suppressionConfig);
            var suppressionAggregator = new BurstSuppressionAggregator();
            var combatEventService = new CombatEventService(suppressionSystem, suppressionAggregator);
            var batchProcessor = new CombatBatchProcessor(combatEventService);
            var weaponFireService = new WeaponFireService(burstResolver, hitResolver, batchProcessor);

            ServiceLocator.Current.Register(combatEventService);
            ServiceLocator.Current.Get<AoEService>().Initialize(combatEventService);


            EventBus<SceneServicesClearEvent>.Register(new EventBinding<SceneServicesClearEvent>(() =>
            {
                ServiceLocator.Current.Unregister<CombatEventService>();
            }));

            // ── Visual stack (Phase 3 addition) ──────────────────────────────────

            var visualRuntime = new CombatVisualRuntime(
                configProvider.CombatSurfaceFX,
                configProvider.CombatTracers,
                ServiceLocator.Current.Get<EffectRequestSystem>(),
                ServiceLocator.Current.Get<CameraController>().Camera,
                maxFxPerFrame: 8);

            var gunshotAudio = new WeaponAudioSystem();
            var grenadeAudio = new GrenadeAudioPlaybackSystem();
            var audioCueSystem = new AudioCueSystem();
            var voiceAudioSystem = new VoiceAudioSystem();
 
            return new CombatRuntime(
                weaponFireService,
                batchProcessor,
                suppressionSystem,
                visualRuntime,
                gunshotAudio,
                grenadeAudio,
                audioCueSystem,
                voiceAudioSystem,
                new CombatDebugDrawer(5f));
        }

        /// <summary>
        /// Собирает AILODSystem. Center provider берётся из FormationCenterDriver —
        /// того же источника истины о позиции отряда, которым пользуется
        /// FormationFollower/SquadTrailRenderer.
        ///
        /// FormationCenterDriver уже зарегистрирован в контейнере squad movement
        /// pipeline'ом (создаётся раньше в жизненном цикле рейда) — резолвим,
        /// а не создаём заново, чтобы не плодить второй "источник истины" о центре.
        /// </summary>
        private AILODSystem BuildAILODSystem(RaidRuntime raid, ConfigProvider configProvider)
        {
            var config = configProvider.Get<AILODConfig>();
            var enemyRepository = ServiceLocator.Current.Get<EnemyRepository>();
            var movementSystem = ServiceLocator.Current.Get<SquadController>().MovementSystem;

            var lod = new AILODSystem(
                config,
                enemyRepository,
                raid.Enemies);

            movementSystem.OnInitialized = _ => lod.Initialize(() => _.Center);

            return lod;
        }

        private void SelectTactical(GameLoopContext context)
        {
            context.ClearFromTactical();
            foreach (var squad in context.StrategicSquadUnits)
                context.SelectForTactical(squad.Id);
        }

    }
}