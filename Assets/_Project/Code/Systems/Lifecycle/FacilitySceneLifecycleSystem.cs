using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Production;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    /// <summary>
    /// Bridge между GameLoopContext и сценой.
    ///
    /// Отвечает только за существование scene-инстансов зданий.
    ///
    /// Runtime -> Scene
    /// </summary>
    public sealed class FacilitySceneLifecycleSystem
    {
        private readonly GameLoopContext _context;
        private FacilityFactory _factory;
        
        
        
        private readonly Dictionary<RuntimeId, FacilityModule> _facilityConfigs = new();
        
        private SceneUnitSource _mode;
        private bool _sceneReady;
        
        

        public FacilitySceneLifecycleSystem(
            GameLoopContext context,
            IReadOnlyDictionary<RuntimeId, FacilityModule> buildingConfigs)
        {
            _context = context;
            
            // #1 заполняем список 
            _facilityConfigs = new(buildingConfigs);

            EventBus<SceneServicesResetReusableEvent>.Register(
                new EventBinding<SceneServicesResetReusableEvent>(() => { _sceneReady = false; }));
        }

        /// <summary>
        /// Вызывается когда сцена лагеря полностью готова.
        /// </summary>
        public void InitializeScene(FacilityFactory factory, SceneUnitSource mode)
        {
            _factory = factory;
            _mode = mode;
            _sceneReady = true;

            InitialSync();

            _context.OnBuildingCreated += HandleBuildingCreated;
            _context.OnBuildingDeleted += HandleBuildingDeleted;

            EventBus<SceneServicesClearEvent>.Register(
                new EventBinding<SceneServicesClearEvent>(() =>
                {
                    _context.OnBuildingCreated -= HandleBuildingCreated;
                    _context.OnBuildingDeleted -= HandleBuildingDeleted;
                }));
        }

        /// <summary>
        /// Полная синхронизация Runtime -> Scene
        /// </summary>
        private void InitialSync()
        {
            foreach (var runtime in GetSourceFacilities())
                SpawnIfMissing(runtime);
        }
        

        /// <summary>
        /// Источник данных.
        ///
        /// Сейчас только Camp,
        /// позже здесь можно сделать CampDefense.
        /// </summary>
        // private IEnumerable<IFacilityRuntime> GetSourceFacilities()
        // {
        //     return _mode switch
        //     {
        //         SceneUnitSource.Camp => _context.Facilities,
        //
        //         SceneUnitSource.CampDefense =>
        //             _context.CurrentRaid.DefenseFacilities.Facilities,
        //
        //         _ => throw new ArgumentOutOfRangeException()
        //     };
        // }
        private IEnumerable<IFacilityRuntime> GetSourceFacilities()
        {
            if (_mode == SceneUnitSource.Camp)
                return _context.Facilities;

            if (_mode == SceneUnitSource.CampDefense)
            {
                var result = new List<IFacilityRuntime>();

                var raidIds = new HashSet<string>();

                // === защитные объекты с рейдовым рантайм
                foreach (var facility in _context.CurrentRaid.DefenseFacilities.Facilities)
                {
                    raidIds.Add(facility.Id);
                    result.Add(facility);
                }

                // === остальные объекты с обычным рантайм, не участвуют в бою но спавнятся
                foreach (var facility in _context.Facilities)
                {
                    if (!raidIds.Contains(facility.Id))
                        result.Add(facility);
                }

                return result;
            }

            throw new ArgumentOutOfRangeException();
        }

        public void HandleBuildingCreated(BaseCampFacilityRuntime runtime)
        {
            if (!_sceneReady)
                return;

            SpawnIfMissing(runtime);
        }

        private void SpawnIfMissing(IFacilityRuntime runtime)
        {
            if (_factory.HasSceneFacility(runtime.Id))
                return;

            _factory.Create(
                _facilityConfigs[runtime.Config.Item.Id],
                runtime);
        }

        public void HandleBuildingDeleted(string buildingId)
        {
            if (!_sceneReady)
                return;

            _factory.Remove(buildingId);
        }
    }
}