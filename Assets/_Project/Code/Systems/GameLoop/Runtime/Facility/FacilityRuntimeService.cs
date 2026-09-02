
using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Systems.Economy.Configs;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Configs;
using Galactic1.Core;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Camp.Proxy;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Production;
using Galactic1.Meta.Configs.Recruitment;
using Galactic1.Utility;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Central building registry and capacity calculator.
    /// Owns building collection and derived limits.
    /// </summary>
    public class FacilityRuntimeService : IFacilityRuntimeService
    {
        private readonly BaseProxy Proxy;
        private readonly GameLoopContext _gameLoopContext;
        private readonly GameTimeService _timeService;
        private readonly IEconomyService _economyService;
        private readonly IConfigProvider _configProvider;
        private readonly StorageRegistry _storageRegistry;
        
        

        public FacilityRuntimeService(
            GameSession gameSession, 
            GameTimeService timeService, 
            IEconomyService economyService,
            IConfigProvider configProvider, 
            StorageRegistry storageRegistry)
        {
            Proxy = gameSession.GameLoopContext.Proxy.BaseProxy;
            _gameLoopContext = gameSession.GameLoopContext;
            _timeService = timeService;
            _economyService = economyService;
            _configProvider = configProvider;
            _storageRegistry = storageRegistry;
        }

        public void Initialize()
        {
            // === восстановление из сохранения объектов лагеря
            foreach (var buildingProxy in Proxy.Buildings)
            {
                if (!GameContent.ResolveFacility(buildingProxy.ConfigId, out var facility))
                {
                    DLog.Alert("[FacilityRuntimeService] Missing facility config: {buildingProxy.ConfigId}",
                        EDlogColor.RED);
                    continue;
                }

                var runtime = CreateRuntime(buildingProxy, facility);
               _gameLoopContext.AddFacility(runtime);
            }
        }
        
        
        
        
        
        
        BaseCampFacilityRuntime CreateRuntime(
            FacilityProxy proxy,
            FacilityModule facilityItem)
        {
            BaseCampFacilityRuntime runtime = null;
            
            switch (facilityItem.FacilityType)
            {
                case FacilityType.MainContainer:
                    var mainContainer = new InboxFacilityRuntime(
                        proxy,
                        (StorageModule)facilityItem,
                        _gameLoopContext.InboxRuntime,
                        _gameLoopContext.CampRuntime,
                        _timeService);
                    runtime = mainContainer;
                    break;
                
                case FacilityType.Storage:
                    var storage = new StorageFacilityRuntime(
                        proxy, 
                        (StorageModule)facilityItem,
                        _gameLoopContext.CampRuntime,
                        _timeService);
                    runtime = storage;
                    _storageRegistry.Register(storage);
                    break;
                
                case FacilityType.Production:
                    runtime = new ProductionStationRuntime(
                        proxy, 
                        (CraftingStationModule)facilityItem, 
                        _timeService);
                    break;
                
                case FacilityType.Recycler:
                    runtime = new RecyclerStationRuntime(
                        proxy, 
                        (CraftingStationModule)facilityItem, 
                        _timeService);
                    break;
                
                case FacilityType.Tavern:
                    runtime = new RecruitmentTavernRuntime(
                        proxy,
                        (TavernModule)facilityItem,
                        _timeService,
                        _configProvider.Get<RecruitmentDatabase>(),
                        _configProvider.Get<RecruitmentSettingsConfig>(),
                        ServiceLocator.Current.Get<ICampCapacityService>(),
                        _economyService,
                        _configProvider.Get<EconomyConfig>(),
                        ServiceLocator.Current.Get<IIdentityGenerator>(),
                        ServiceLocator.Current.Get<IWeightedRandomService>(),
                        ServiceLocator.Current.Get<IRecruitEquipmentGenerator>()
                    );
                    break;
                
                case FacilityType.LivingModule:
                    runtime = new LivingModuleFacilityRuntime(
                        proxy, 
                        (LivingModule)facilityItem, 
                        _timeService,
                        ServiceLocator.Current.Get<ICampCapacityService>());
                    break;
                
                case FacilityType.Garage:
                    runtime = new GarageFacilityRuntime(
                        proxy,
                        (GarageModule)facilityItem,
                        _timeService,
                        _gameLoopContext);
                    break;
                
                
                case FacilityType.CampHQ:
                    runtime = new CampHQFacilityRuntime(
                        proxy,
                        (CampHQModule)facilityItem,
                        _timeService);
                    break;
                
                case FacilityType.Defense:
                    runtime = new WallFacilityRuntime( // сделал через WallFacilityRuntime т.к на релизе только стены
                        proxy,
                        (DefenseFacilityModule)facilityItem,
                        _timeService);
                    break;
                
            }

            // for saving
            runtime.OnStateChanged += ServiceLocator.Current.Get<GameSession>().MarkDirty;
            
            return runtime;
        }
        
        
        
        /// <summary>
        /// Полное создание нового здания
        /// </summary>
        public BaseCampFacilityRuntime CreateBuildingCompletely(
            FacilityModule facilityItem,
            BuildingFootprintRuntime footprint)
        {

            List<KeyValuePairSerializable<StatId, float>> statsBase = 
                new List<KeyValuePairSerializable<StatId, float>>();

            // === для защитных объектов устанавливаем базовое хп
            if (facilityItem.Item.HasModule<BuildingHealthModule>())
            {
                statsBase = DictionaryUtility.ToList(new Dictionary<StatId, float>()
                {
                    { StatId.Health, facilityItem.Item.BuildingHealth.Settings.maxHealth }
                });
            }
            
            // 1️⃣ создаём data
            var data = new FacilityData()
            {
                UniqueId = Guid.NewGuid().ToString(),
                ConfigGuid = facilityItem.Item.Id.Guid,
                Stats = statsBase,
                PosX = footprint.Origin.x,
                PosZ = footprint.Origin.y,
                Rotation = footprint.Rotation
            };
            
            // 2️⃣ proxy
            var proxy = new FacilityProxy(data);
            Proxy.Buildings.Add(proxy);
            
            // 3️⃣ runtime
            var runtime = CreateRuntime(proxy, facilityItem);

            // 4️⃣ scene instance
            _gameLoopContext.RegisterFacility(runtime);
            
            // регистрация хранилищ
            if (runtime is StorageFacilityRuntime storageRuntime)
            {
                _gameLoopContext.CampRuntime.RegisterAndResizeStorage((StorageModule)facilityItem);
            }
            
            ServiceLocator.Current.Get<IGameStateProvider>().SaveGameState();
            
            return runtime;
        }
        
        public void DeleteBuildingCompletely(string facilityId)
        {
            if(!_gameLoopContext.TryGetBuilding(facilityId, out var runtime))
                return;
            
            // уменьшаем вместимость хранилища
            if (runtime is StorageFacilityRuntime storageRuntime)
            {
                _storageRegistry.Unregister(storageRuntime);
                _gameLoopContext.CampRuntime.UnregisterStorage(storageRuntime.Module);
            }
            
            runtime.Dispose();
            
            var buildingProxy = Proxy.Buildings.FirstOrDefault(u => u.UniqueId == facilityId);
            if (buildingProxy != null)
                Proxy.Buildings.Remove(buildingProxy);
            
            _gameLoopContext.UnregisterFacility(facilityId);
            
            ServiceLocator.Current.Get<IGameStateProvider>().SaveGameState();
#if UNITY_EDITOR
            DLog.Alert($"[Lifecycle] Removed scene building: {facilityId}", EDlogColor.YELLOW);
#endif
        }


        
    }
}