using Galactic1.Code.WorldMap;
using Galactic1.Configs;
using Galactic1.Core;
using Galactic1.Core.GameSession;
using UnityEngine;
using Galactic1.Core.Systems.GameSession;
using Galactic1.Gameplay.Locations.Modes;

namespace Galactic1.Gameplay.Locations
{
    /// <summary>
    /// Главный загрузчик локации. Аналог LevelController в LDoE.
    /// 
    /// Он:
    /// 1) читает тип локации (Camp / Regular / Event)
    /// 2) вызывает нужный загрузчик (CampLoader или RegularLevelLoader)
    /// 3) создает LocationContext
    /// 4) управляет очисткой
    /// </summary>
    public class LocationLoader : MonoBehaviour
    {
        private ILocationLoaderMode _modeLoader;
        private ILocationCleanerMode _modeCleaner;

        public void Load(out SceneSessionDefinition session, DIContainer container)
        {
            var gameStateProvider = container.Resolve<IGameStateProvider>();
            var configProvider = container.Resolve<IConfigProvider>();
            

            // #1 Получаем данные локации из конфига
            var locationConfig = configProvider.Get<LocationsConfigs>()
                .Locations[gameStateProvider.GameStateProxy.GameLoopContext.CurrentLocationStateId.Value];
            
            
            // #2 Создаем единый контекст
            var context = new LocationContext();
            context.LcoationId = gameStateProvider.GameStateProxy.GameLoopContext.CurrentLocationStateId.Value;
            context.LocationConfig = locationConfig;

            // #3 Определяем тип загрузки
            switch (locationConfig.LocationType)
            {
                case LocationType.Home:
                    _modeLoader = new HomeLoader();
                    _modeCleaner = new HomeCleaner();
                    _modeCleaner.Clear(context);
                    break;
            
                case LocationType.Components:
                case LocationType.Scrap:
                    _modeLoader = new RegularLevelLoader();
                    _modeCleaner = new RegularLevelCleaner();
                    break;
            
                default:
                    _modeLoader = new RegularLevelLoader();
                    _modeCleaner = new RegularLevelCleaner();
                    break;
            }
            
            // #4 Запуск загрузки
            _modeLoader.Load(context, container);

            // очистка локации
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(Clear));
            
            // ****************************************************************************************************
            
            
            // #5 Создаем контекст для текущей локации
            var sceneDefinition = FindAnyObjectByType<SceneContext>();
            var sceneDefinitionData = sceneDefinition.GetDefinitionData();
            session = new SceneSessionDefinitionLoader().LoadDefinition(
                sceneDefinitionData,
                context);
            
        }


        public void Clear()
        {
            _modeCleaner?.Clear(ServiceLocator.Current.Get<SceneSessionManager>().Session.LocationContext);
        }
    }
}