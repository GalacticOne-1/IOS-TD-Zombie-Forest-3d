
using System;
using System.Collections.Generic;
using Galactic1.Code.Cameras;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Code.Systems.World.Threats;
using Galactic1.Configs;
using Galactic1.Core;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Контроллер карты: связывает визуальные элементы с логикой карты.
    /// </summary>
    public class WorldMapController : MonoBehaviour, IGameService
    {
        [Header("Map Elements")]
        [SerializeField] private VehicleIconController vehicle;
        [SerializeField] private Transform mapNodesRoot;
        
        [Header("Map Elements")]
        [SerializeField] private Transform labelsRoot;

        private GameTimeService timeService;
        private WorldMapService mapService;
        public LocationOverview locationOverview { get; set; }
        private MapRouteRenderer routeRenderer;
        private CurrentLocationLabel currentLocationLabel;
        
        //public WorldUIFollow LocationEnter {get; private set;}
        
        public List<MapNode> MapNodes { get; private set; } = new();

        // время за перемещение к локации
        private int _pendingPathCost;


        public event Action<MapNode> OnLocationChanged; 
        
        

        public void Initialize()
        {
            Debug.Log("----------------------------- [WorldMapController]");
            
            // === сервис глобального времени
            timeService = ServiceLocator.Current.Get<GameTimeService>();
            
            
            //
            routeRenderer = GetComponent<MapRouteRenderer>();
            routeRenderer.Hide();
                
            // === передаем каждой локации свой конфиг
            var configBase = ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>();
            
            var l = mapNodesRoot.childCount;
            for (int i = 0; i < l; i++)
            {
                var node = mapNodesRoot.GetChild(i).GetComponent<MapNode>();
                MapNodes.Add(node);
                if (node.Id != null)
                    node.SetConfig = configBase.GetConfig(node.Id);
                node.OnNodeClicked = OnNodeClicked;
            }
            
            
            // === location label
            var labelPrefab = Resources.Load<LocationLabel>("Prefabs/UI/Gameplay/WorldMap/LocationLabel");
            foreach(var node in MapNodes)
            {
                var label = Instantiate(labelPrefab, labelsRoot);
                label.Bind(node);
                //node.SetMarker(marker); // опционально, если нужен доступ из ноды
            }
            
            // === current location
            currentLocationLabel = "Prefabs/UI/Gameplay/WorldMap/CurrentLocationLabel"
                .CreateGO(labelsRoot).GetChild(1).GetComponent<CurrentLocationLabel>();

            // === кнопка входа в локацию
            // var uiWorldRoot = ServiceLocator.Current.Get<UIManager>().TransformRoot.floatWorldRoot;
            // LocationEnter = "Prefabs/UI/Gameplay/WorldMap/LocationEnterLabel"
            //     .CreateGO(uiWorldRoot)
            //     .GetComponent<WorldUIFollow>();
            
            // подписки на события движения
            vehicle.OnMoveStarted += OnPlayerMoveStarted;
            vehicle.OnMoveFinished += OnPlayerMoveFinished;
        }

        public void Bind(WorldMapService service)
        {
            mapService = service;
            vehicle.Bind(mapService);
            
            // первичная инициализация метки
            currentLocationLabel.Bind(mapService.CurrentNode);
            currentLocationLabel.transform.parent.gameObject.SetActive(true);
            
            OnLocationChanged?.Invoke(mapService.CurrentNode);
            
            vehicle.transform.position = mapService.CurrentNode.transform.position;
        }

        /// <summary>
        /// Вернет дом и текущую ноду
        /// </summary>
        /// <returns></returns>
        public (MapNode home, MapNode current) GetStartNode()
        {
            MapNode home = null;
            MapNode current = null;

            var gameStateProxy = ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy;
            var loadedLocation = gameStateProxy.GameLoopContext.CurrentLocationNode.Value;

            var l = MapNodes.Count;
            for (int i = 0; i < l; i++)
            {
                if (home == null && MapNodes[i].IsCamp)
                    home = MapNodes[i];

                if (current == null && MapNodes[i].Id.Guid == loadedLocation)
                    current = MapNodes[i];
            }

            if (current == null)
                current = home;

            return (home, current);
        }

        public MapNode GetNode(LocationId id)
        {
            var l = MapNodes.Count;
            for (int i = 0; i < l; i++)
            {
                if (MapNodes[i].Id == id)
                    return MapNodes[i];
            }

            return null;
        }


        /// <summary>
        /// Вызывается при клике на узел на карте
        /// </summary>
        public void OnNodeClicked(MapNode targetNode)
        {
            // TODO
            // Сделать валидацию отряда до старта рейда                                                     // FIX
            
            if (targetNode == mapService.CurrentNode ||
                vehicle.IsMoving)
                return;
            
            // === время до угрозы
            var threat = ServiceLocator.Current.Get<WorldThreatService>().GetCurrentThreat();
            var remainingDays= threat?.GetRemainingDays(ServiceLocator.Current.Get<GameTimeService>().TotalWorldHours);
            
            // === время до локации и вернутся домой
            var toTargetTime = mapService.GetVisitCost(mapService.CurrentNode, targetNode);
            var backHomeTime = mapService.GetVisitCost(targetNode, mapService.HomeNode);
            
            _pendingPathCost = toTargetTime;

            float totalCost = toTargetTime + backHomeTime;
            //bool canVisit = totalCost <= daysUntilThreat;
            
            //if (!canVisit)
            //return;
            
            ServiceLocator.Current.Get<UIManager>().OpenScreen(
                UIScreenId.LocationOverview,
                null, 
                _ =>
            {
                
                EventBus<WorldMapLocationSelectedEvent>.Raise(new WorldMapLocationSelectedEvent()
                {
                    LocationId = targetNode.Id,
                });
                
                locationOverview.ShowNodeInfo(
                    targetNode,
                    true,
                    toTargetTime,
                    backHomeTime,
                    remainingDays.HasValue ? remainingDays.Value : -1,
                    () =>
                    {
                        // Запускаем визуальное перемещение
                        vehicle.MoveTo(targetNode);
                        
                        // показать маршрут
                        routeRenderer.ShowRoute(mapService.CurrentNode, targetNode);
                    }
                );
            });
        }
        
        
        
        private void OnPlayerMoveStarted()
        {
            // Игрок начал движение — скрываем метку текущей локации
            currentLocationLabel.transform.parent.gameObject.SetActive(false);
        }

        /// <summary>
        /// Транспорт закончил перемещение на новую локацию
        /// </summary>
        private void OnPlayerMoveFinished()
        {
            // Списываем время пути до локации
            timeService.SpendHours(_pendingPathCost, TimeAdvanceReason.MapMovement);
            _pendingPathCost = 0;
            
            // Игрок прибыл — обновляем текущую ноду в сервисе
            var newNode = mapService.CurrentNode;
            
            routeRenderer.Hide();

            // Перепривязываем метку
            currentLocationLabel.Bind(newNode);
            
            OnLocationChanged?.Invoke(newNode);

            // Показываем снова
            currentLocationLabel.transform.parent.gameObject.SetActive(true);
        }


        public void ToCurrentLocation()
        {
            ServiceLocator.Current.Get<IMainCamera>().FocusOnPosition(mapService.CurrentNode.transform.position);
        }
    }
}