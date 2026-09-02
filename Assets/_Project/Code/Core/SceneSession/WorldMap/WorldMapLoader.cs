using Galactic1.Code.WorldMap;
using Galactic1.Core.GameSession;
using UnityEngine;

namespace Galactic1.Core.Systems.GameSession.WorldMap
{
    /// <summary>
    /// Загружает визуал и основные объекты карты
    /// </summary>
    public class WorldMapLoader
    {
        public void Load(ref SceneSessionDefinition session)
        {
            // 1. Найти контроллер карты на сцене
            var controller = ServiceLocator.Current.Get<WorldMapController>();
            controller.Initialize();
            session.WorldMapContext.MapController = controller;

            // 2. Создать сервис карты
            var service = new WorldMapService();
            session.WorldMapContext.MapService = service;

            // 3. Инициализация стартового узла
            (MapNode home, MapNode current) node = controller.GetStartNode();
            service.Initialize(node.home, node.current);
            
            // камера должна стоять в текущей локации
            session.CameraPosition = node.current.transform.position;

            // 5. Синхронизация транспорта
            controller.Bind(service);

            Debug.Log("[WorldMapLoader] World map loaded");
        }

        
    }
}