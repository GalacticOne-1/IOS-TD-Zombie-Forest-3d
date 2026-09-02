using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1
{
    /// <summary>
    /// Такой сервис создается один раз со сценой
    /// <br/>и когда сцена уничтожается  
    /// <br/>сервисы со всеми подписками тоже уничтожаются
    /// </summary>
    public class BaseFacilityService
    {
        private readonly GameLoopContext _gameLoopContext;
        private readonly FacilityFactory _facilityFactory;
        private readonly Dictionary<RuntimeId, FacilityModule> _facilityConfigs = new();
        
        // здесь восстанавливаем здания из сохранения
        public BaseFacilityService(
            GameLoopContext gameLoopContext,
            FacilityFactory facilityFactory,
            IReadOnlyDictionary<RuntimeId, FacilityModule> buildingConfigs)
        {
            _gameLoopContext = gameLoopContext;
            _facilityFactory = facilityFactory;

            
            

            // #1 заполняем список 
            _facilityConfigs = new(buildingConfigs);
            
            // синхронизация >> LOADING <<
            var facilityRuntimeService = ServiceLocator.Current.Get<IFacilityRuntimeService>();
            var buildings = _gameLoopContext.Facilities;
            foreach (var runtime in buildings)
                CreateInstance(runtime);
            // синхронизация >> LOADING <<
            
            _gameLoopContext.OnBuildingCreated += CreateInstance;
            _gameLoopContext.OnBuildingDeleted += DestroyInstance;
            
            EventBus<SceneServicesClearEvent>.Register(new EventBinding<SceneServicesClearEvent>(() =>
            {
                _gameLoopContext.OnBuildingCreated -= CreateInstance;
                _gameLoopContext.OnBuildingDeleted -= DestroyInstance;
            }));

        }


        /// <summary>
        /// Создание объекта в сцене
        /// </summary>
        private void CreateInstance(BaseCampFacilityRuntime runtime)
        {
            var config = _facilityConfigs[runtime.Config.Item.Id];
            //var buildSlot = buildSlotRegistry.Get(runtime.SlotId);

            _facilityFactory.Create(config, runtime);
        }

        private void DestroyInstance(string id)
        {
            _facilityFactory.Remove(id);
        }
    }
}