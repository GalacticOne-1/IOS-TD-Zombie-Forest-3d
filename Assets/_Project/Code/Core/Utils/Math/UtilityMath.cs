using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Utility
{
    public static class UtilityMath
    {
        /// <summary>
        /// Определяет направление targetCoord относительно объекта на сетке.
        /// </summary>
        /// <param name="targetCoord">Координата целевой точки</param>
        /// <param name="objectCoord">Левая нижняя клетка объекта</param>
        /// <param name="objectSize">Размер объекта (ширина, высота)</param>
        /// <returns>Направление или null, если точка не сбоку (внутри или по диагонали)</returns>
        public static Vector2Int GetDirectionToPoint(
            Vector2Int targetCoord,
            Vector2Int objectCoord,
            Vector2Int objectSize)
        {
            int left = objectCoord.x;
            int right = objectCoord.x + objectSize.x - 1;
            int bottom = objectCoord.y;
            int top = objectCoord.y + objectSize.y - 1;

            
            // Внутри объекта — нет направления
            // if (targetCoord.x >= left && targetCoord.x <= right &&
            //     targetCoord.y >= bottom && targetCoord.y <= top)
            // {
            //     return null;
            // }

            // Слева
            if (targetCoord.x == left - 1 &&
                targetCoord.y >= bottom && targetCoord.y <= top)
            {
                return Vector2Int.left;
            }

            // Справа
            if (targetCoord.x == right + 1 &&
                targetCoord.y >= bottom && targetCoord.y <= top)
            {
                return Vector2Int.right;
            }

            // Снизу
            if (targetCoord.y == bottom - 1 &&
                targetCoord.x >= left && targetCoord.x <= right)
            {
                return Vector2Int.down;
            }

            // Сверху
            if (targetCoord.y == top + 1 &&
                targetCoord.x >= left && targetCoord.x <= right)
            {
                return Vector2Int.up;
            }

            // По диагонали или далеко
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// Определяет направление точки target относительно origin.
        /// Работает корректно для размера объекта 1x1
        /// </summary>
        public static Vector2Int GetRelativeDirection(Vector2Int origin, Vector2Int target)
        {
            Vector2Int result = Vector2Int.zero;
            Vector2Int direction = target - origin;

            // Проверка по доминирующей оси (та, по которой расстояние больше)
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                result.x = direction.x > 0 ? 1 : -1;
            else
                result.y = direction.y > 0 ? 1 : -1;

            return result;
        }
        
        
        
        
        
        
        
        /// <summary>
        /// Возвращает все объекты на указанной клетке с помощью Physics2D.OverlapCircleAll
        /// </summary>
        /// <param name="cellPosition">Мировая позиция центра клетки</param>
        /// <param name="radius">Радиус перекрытия (обычно ~0.4–0.5f для клетки 1x1)</param>
        /// <param name="layerMask">Какие слои проверять (можно передать LayerMask.GetMask(...))</param>
        /// <returns>Список найденных объектов (Collider2D)</returns>
        public static List<GameObject> GetObjectsOnCell(
            Vector2 cellPosition, 
            float radius = AppConstants.DetectionRadius, 
            LayerMask? layerMask = null)
        {
            LayerMask mask = layerMask ?? Physics2D.DefaultRaycastLayers;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(cellPosition, radius, mask);

            List<GameObject> objects = new List<GameObject>();
            foreach (var col in colliders)
            {
                objects.Add(col.gameObject);
            }

            return objects;
        }
        
        
        /// <summary>
        /// Проверяет наличие преград на клетке, исключая указанные объекты
        /// </summary>
        /// <param name="cellPosition">Позиция клетки в мировых координатах</param>
        /// <param name="ignoredObjects">Список объектов, которые нужно игнорировать при проверке</param>
        /// <param name="radius">Радиус перекрытия клетки</param>
        /// <param name="layerMask">Слои, по которым производится проверка</param>
        /// <returns>true — если есть преграда, false — если путь свободен</returns>
        public static bool HasObstacleOnCell(
            Vector2 cellPosition, 
            List<GameObject> ignoredObjects,
            float radius = AppConstants.DetectionRadius, 
            LayerMask? layerMask = null)
        {
            var objectsOnCell = GetObjectsOnCell(cellPosition, radius, layerMask);

            foreach (var obj in objectsOnCell)
            {
                if (ignoredObjects.Contains(obj))
                    continue;

                return true; // Найдена преграда
            }

            return false; // Преград не найдено
        }
        
        
        /// <summary>
        /// Проверяет наличие преград на клетке, исключая указанные объекты
        /// </summary>
        /// <param name="cellPosition">Позиция клетки в мировых координатах</param>
        /// <param name="ignoredObjects">Список объектов, которые нужно игнорировать при проверке</param>
        /// <param name="obstacleTags">Какие теги считаются преградами (например, "Wall", "Tower")</param>
        /// <param name="radius">Радиус перекрытия клетки</param>
        /// <param name="layerMask">Слои, по которым производится проверка</param>
        /// <returns>true — если есть преграда, false — если путь свободен</returns>
        public static bool HasObstacleOnCell(
            Vector2 cellPosition, 
            List<GameObject> ignoredObjects, 
            string[] obstacleTags, 
            float radius = AppConstants.DetectionRadius, 
            LayerMask? layerMask = null)
        {
            var objectsOnCell = GetObjectsOnCell(cellPosition, radius, layerMask);

            foreach (var obj in objectsOnCell)
            {
                if (ignoredObjects.Contains(obj))
                    continue;

                // если на одном слое должны находится разные по смыслу сущности
                // enemey - not obstacle
                // wall, tower - is obstacle
                foreach (var tag in obstacleTags)
                {
                    if (obj.CompareTag(tag))
                        return true; // Найдена преграда
                }
            }

            return false; // Преград не найдено
        }
    }
}