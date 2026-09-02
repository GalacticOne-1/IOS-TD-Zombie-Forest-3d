using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Utility
{

    /// <summary>
    /// Набор вспомогательных геометрических методов для Unity 2D.
    /// Работает с Vector3/Vector2, клетками и проверкой коллайдеров.
    /// </summary>
    public static class Geometry2D
    {
        // -------------------------------
        // 📍 ОСНОВНЫЕ МЕТОДЫ
        // -------------------------------

        /// <summary>
        /// Возвращает геометрический центр между позициями.
        /// </summary>
        public static Vector2 GetCenter(List<Vector3> positions)
        {
            if (positions == null || positions.Count == 0) return Vector2.zero;

            Vector2 sum = Vector2.zero;
            foreach (Vector3 pos in positions)
                sum += (Vector2)pos;

            return sum / positions.Count;
        }

        /// <summary>
        /// Возвращает медианную позицию группы объектов.
        /// Более устойчива к выбросам, чем GetCenter.
        /// </summary>
        public static Vector2 GetMedianPosition(List<Vector3> positions)
        {
            if (positions == null || positions.Count == 0) return Vector2.zero;

            var xs = new List<float>();
            var ys = new List<float>();

            foreach (var pos in positions)
            {
                xs.Add(pos.x);
                ys.Add(pos.y);
            }

            xs.Sort();
            ys.Sort();
            int mid = xs.Count / 2;
            return new Vector2(xs[mid], ys[mid]);
        }

        /// <summary>
        /// Возвращает ближайшую позицию к заданной точке.
        /// </summary>
        public static Vector3 GetNearestPosition(List<Vector3> positions, Vector3 fromPoint)
        {
            Vector3 nearest = Vector3.zero;
            float minDist = float.MaxValue;

            foreach (var pos in positions)
            {
                float dist = Vector3.Distance(fromPoint, pos);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = pos;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Возвращает самую дальнюю позицию от заданной точки.
        /// </summary>
        public static Vector3 GetFarthestPosition(List<Vector3> positions, Vector3 fromPoint)
        {
            Vector3 farthest = Vector3.zero;
            float maxDist = float.MinValue;

            foreach (var pos in positions)
            {
                float dist = Vector3.Distance(fromPoint, pos);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    farthest = pos;
                }
            }

            return farthest;
        }

        /// <summary>
        /// Возвращает прямоугольную область (Bounds), охватывающую все позиции.
        /// </summary>
        public static Bounds GetBoundingBox(List<Vector3> positions)
        {
            if (positions == null || positions.Count == 0) return new Bounds(Vector3.zero, Vector3.zero);

            Bounds bounds = new Bounds(positions[0], Vector3.zero);
            foreach (var pos in positions)
                bounds.Encapsulate(pos);

            return bounds;
        }

        /// <summary>
        /// Возвращает среднее направление от referencePoint к группе позиций.
        /// </summary>
        public static Vector2 GetAverageDirection(List<Vector3> positions, Vector2 referencePoint)
        {
            if (positions == null || positions.Count == 0) return Vector2.zero;

            Vector2 sum = Vector2.zero;
            foreach (var pos in positions)
                sum += ((Vector2)pos - referencePoint).normalized;

            return (sum / positions.Count).normalized;
        }

        /// <summary>
        /// Возвращает нормализованный вектор от точки к центру группы.
        /// </summary>
        public static Vector2 GetDirectionToCenter(Vector2 from, List<Vector3> positions)
        {
            Vector2 center = GetCenter(positions);
            return (center - from).normalized;
        }

        /// <summary>
        /// Возвращает случайную точку внутри круга заданного радиуса вокруг центра.
        /// </summary>
        public static Vector2 GetRandomPointInRadius(Vector2 center, float radius)
        {
            Vector2 random = Random.insideUnitCircle * radius;
            return center + random;
        }

        // -------------------------------
        // 🔍 КЛЕТОЧНЫЕ МЕТОДЫ
        // -------------------------------

        /// <summary>
        /// Возвращает список клеток (Vector2) в кольце между minRadius и maxRadius.
        /// </summary>
        public static List<Vector3> GetCellsInRadius(
            Vector3 center, 
            float minRadius, 
            float maxRadius, 
            float cellSize)
        {
            List<Vector3> cells = new List<Vector3>();
            int range = Mathf.CeilToInt(maxRadius / cellSize);

            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    Vector3 cellPos = new Vector3(
                        Mathf.Round(center.x / cellSize) * cellSize + x * cellSize,
                        Mathf.Round(center.y / cellSize) * cellSize + y * cellSize
                    );

                    float distance = Vector3.Distance(center, cellPos);
                    if (distance <= maxRadius && distance > minRadius)
                        cells.Add(cellPos);
                }
            }

            return cells;
        }

        /// <summary>
        /// Возвращает клетки с проверкой коллайдеров через Physics2D.
        /// inverse = true → возвращает только пустые клетки.
        /// inverse = false → возвращает только занятые клетки.
        /// </summary>
        public static List<Vector3> GetCellsWithPhysics(
            Vector3 center, 
            float minRadius, 
            float maxRadius,
            float cellSize, 
            float cellOffset,
            LayerMask layerMask, 
            bool inverse = true)
        {
            var cells = GetCellsInRadius(center, minRadius, maxRadius, cellSize);
            var filtered = new List<Vector3>();

            Vector3 c;
            foreach (var cell in cells)
            {
                c = cell;
                c.x += cellOffset;
                c.y += cellOffset;
                Collider2D col = Physics2D.OverlapCircle(c, AppConstants.DetectionRadius, layerMask);
                if ((col != null && !inverse) || (col == null && inverse))
                    filtered.Add(c);
            }

            return filtered;
        }
        
        /// <summary>
        /// Возвращает пассив с объектами в радиусе
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static Collider2D[] GetEntitiesInRadius(
            Vector3 center, 
            float radius, 
            LayerMask layerMask)
        {
            return Physics2D.OverlapCircleAll(center, radius, layerMask);
        }
        
        
        
        /// <summary>
        /// Генерирует список случайных позиций в пределах радиуса.
        /// Учитывает минимальное расстояние между позициями и слой земли.
        /// </summary>
        public static List<Vector2> GetPositionsInRadius(
            Vector2 center,
            float radius,
            int count,
            LayerMask groundMask = default,
            float minDistanceBetweenExplosions = 0f,
            int maxAttemptsPerExplosion = 10)
        {
            List<Vector2> positions = new List<Vector2>();

            for (int i = 0; i < count; i++)
            {
                Vector2 chosenPos = center;

                for (int attempt = 0; attempt < maxAttemptsPerExplosion; attempt++)
                {
                    Vector2 candidate = center + Random.insideUnitCircle * radius;

                    // Проверка на землю
                    if (groundMask != 0 && Physics2D.OverlapPoint(candidate, groundMask) == null)
                        continue;

                    // Проверка минимальной дистанции
                    bool tooClose = false;
                    foreach (var pos in positions)
                    {
                        if (Vector2.Distance(candidate, pos) < minDistanceBetweenExplosions)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        chosenPos = candidate;
                        break;
                    }
                }

                positions.Add(chosenPos);
            }

            return positions;
        }

        // -------------------------------
        // 🖌️ ВИЗУАЛИЗАЦИЯ В GIZMOS
        // -------------------------------

        /// <summary>
        /// Рисует крестики в Scene View для списка клеток.
        /// color – цвет крестиков.
        /// size – половина длины крестика.
        /// </summary>
        public static void DrawCellsGizmos(List<Vector2> cells, Color color, float size = 0.2f)
        {
            if (cells == null) return;

            Gizmos.color = color;
            foreach (var cell in cells)
            {
                Gizmos.DrawLine(cell + Vector2.one * size, cell - Vector2.one * size);
                Gizmos.DrawLine(new Vector2(cell.x - size, cell.y + size), new Vector2(cell.x + size, cell.y - size));
            }
        }
    }


}