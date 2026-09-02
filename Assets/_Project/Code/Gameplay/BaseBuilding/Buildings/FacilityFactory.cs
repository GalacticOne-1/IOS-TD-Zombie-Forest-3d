using System.Collections.Generic;
using Galactic1.Code.Gameplay.Construction;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Production;
using Galactic1.UI.WorldStatus;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    /// <summary>
    /// Центральная фабрика строительства базы.
    /// Единственная точка создания зданий.
    ///
    /// Bind/unbind логика вынесена в FacilitySceneBinder (аналог
    /// UnitSceneLifecycleSystem / SurvivorSceneBinder для юнитов).
    /// </summary>
    public class FacilityFactory
    {
        private readonly ConstructionService _constructionService;
        private readonly GridCoordinateService _gridCoordinateService;
        private readonly WorldStatusFactory _worldStatusFactory;

        private readonly Dictionary<string, FacilitySceneBinder> _binders = new();

        public FacilityFactory(
            ConstructionService constructionService,
            GridCoordinateService gridCoordinateService,
            WorldStatusFactory worldStatusFactory)
        {
            _constructionService = constructionService;
            _gridCoordinateService = gridCoordinateService;
            _worldStatusFactory = worldStatusFactory;
        }


        public bool HasSceneFacility(string id)
            => _binders.ContainsKey(id);
        
        

        /// <summary>
        /// Создание здания.
        /// Используется и в runtime, и при загрузке.
        /// </summary>
        public FacilityInstance Create(
            FacilityModule buildItem,
            IFacilityRuntime runtime)
        {
            var instance = $"{AppConstants.PATH_STRUCTURES}{buildItem.Item.PrefabPath}"
                .CreateGO(ServiceLocator.Current.Get<Environment>().playerObj)
                .GetComponent<FacilityInstance>();

            instance.UniqueId = runtime.Id;
            instance.ItemConfig = buildItem.Item;

            ServiceLocator.Current
                .Get<BaseFacilityRepository>()
                .Register(instance.UniqueId, instance);

            var binder = new FacilitySceneBinder(
                runtime,
                _constructionService,
                _worldStatusFactory);

            binder.Attach(instance, buildItem);
            _binders[runtime.Id] = binder;

            return instance;
        }

        /// <summary>
        /// Демонтаж здания.
        /// </summary>
        public void Remove(string buildingId)
        {
            // TODO:
            // - вернуть часть ресурсов
            // - освободить рабочих
            // - остановить производство
            // - отправить событие BuildingDemolishedEvent

            var repository = ServiceLocator.Current.Get<BaseFacilityRepository>();
            var rep = repository.TryGet(buildingId);
            repository.Unregister(buildingId, rep.instance);

            if (_binders.TryGetValue(buildingId, out var binder))
            {
                binder.Dispose(); // unregister из ConstructionService + Entity_Destroy
                _binders.Remove(buildingId);
            }
        }
    }
}