
using Galactic1.Core;
using UnityEngine;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Логика карты: маршруты, расчёт времени, проверка доступности
    /// </summary>
    public class WorldMapService
    {
        public MapNode HomeNode { get; private set; }
        public MapNode CurrentNode { get; private set; }
        public MapNode TargetNode { get; private set; }
        
        
        
        

        /// <summary>
        /// Инициализация карты, установка стартового узла
        /// </summary>
        public void Initialize(MapNode homeNode, MapNode currentNode)
        {
            HomeNode = homeNode;
            CurrentNode = currentNode;
        }
        
        /// <summary>
        /// Полная стоимость визита в target из текущей позиции
        /// </summary>
        public int GetVisitCost(MapNode from, MapNode target)
        {
            return GetPathCost(from, target, target.GetVisitCost());
        }

        
        /// <summary>
        /// Стоимость пути между локациями
        /// </summary>
        public int GetPathCost(MapNode from, MapNode to, float visitedCost)
        {
            if (from == to) return 0;
            
            float distance = Vector3.Distance(from.transform.position, to.transform.position);
            float unitsPerHour = 10f / 24f; // 10 единиц = 1 день = 24 часа
            int hours = Mathf.CeilToInt((distance + visitedCost) / unitsPerHour);

            return hours;

            // float distance = Vector3.Distance(from.transform.position, to.transform.position);
            // float speedPerDay = 10f; // 10 единиц = 1 день
            // float cost = distance / speedPerDay;
            //
            // return cost;
        }
        
        /// <summary>
        /// Проверка: может ли игрок посетить target
        /// и вернуться домой до наступления угрозы
        /// </summary>
        public bool CanVisitAndReturnHome(
            MapNode current,
            MapNode target,
            MapNode home,
            float daysUntilHorde
        )
        {
            float toTarget = GetVisitCost(current, target);
            float backHome = GetVisitCost(target, home);

            float total = toTarget + backHome;
            return total <= daysUntilHorde;
        }

        
        /// <summary>
        /// Применить посещение локации
        /// </summary>
        public void SetCurrentNode(MapNode node)
        {
            CurrentNode = node;
            
            // saving
            var gameStateProvider = ServiceLocator.Current.Get<IGameStateProvider>();
            
            // TODO
            // must scene saving ...
            
            gameStateProvider.GameStateProxy.GameLoopContext.CurrentLocationNode.Value = CurrentNode.Id.Guid;
            gameStateProvider.SaveGameState();
        }


        
    }
}
