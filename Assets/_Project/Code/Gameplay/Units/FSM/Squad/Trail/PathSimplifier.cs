using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    public static class PathSimplifier
    {
        private const float MinPointDistance = 0.05f;

        /// <summary>
        /// Максимальное перпендикулярное отклонение точки от прямой
        /// между соседними сохранёнными точками. Чем больше — тем прямее
        /// итоговая линия, но тем грубее повторяет исходный путь.
        /// </summary>
        private const float DouglasPeuckerEpsilon = 0.5f;

        public static List<Vector3> Simplify(IReadOnlyList<Vector3> rawPath)
        {
            var result = new List<Vector3>(rawPath.Count);
            if (rawPath.Count == 0) return result;

            // 1. Убираем почти совпадающие точки
            result.Add(rawPath[0]);
            for (int i = 1; i < rawPath.Count; i++)
            {
                Vector3 prev = result[^1];
                Vector3 curr = rawPath[i];
                if (Vector3.Distance(prev, curr) >= MinPointDistance)
                    result.Add(curr);
            }

            if (result.Count <= 2)
                return result;

            // 2. Douglas-Peucker: только прямые отрезки, никакого сглаживания.
            //    Заменяет угловой коллинеарный тест — устойчивее к мелким
            //    изломам A*-пути по navmesh, которые по отдельности не коллинеарны,
            //    но в сумме дают видимый зигзаг.
            var keep = new bool[result.Count];
            keep[0] = true;
            keep[result.Count - 1] = true;
            DouglasPeucker(result, 0, result.Count - 1, DouglasPeuckerEpsilon, keep);

            var simplified = new List<Vector3>(result.Count);
            for (int i = 0; i < result.Count; i++)
                if (keep[i])
                    simplified.Add(result[i]);

            return simplified;
        }

        private static void DouglasPeucker(
            List<Vector3> points, int first, int last,
            float epsilon, bool[] keep)
        {
            if (last <= first + 1) return;

            Vector3 a = points[first];
            Vector3 b = points[last];
            Vector3 dir = b - a;
            float lenSqr = dir.sqrMagnitude;

            float maxDist = -1f;
            int maxIndex = -1;

            for (int i = first + 1; i < last; i++)
            {
                float dist = PerpendicularDistance(points[i], a, dir, lenSqr);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    maxIndex = i;
                }
            }

            if (maxDist <= epsilon) return; // отрезок [first, last] уже достаточно прямой

            keep[maxIndex] = true;

            DouglasPeucker(points, first, maxIndex, epsilon, keep);
            DouglasPeucker(points, maxIndex, last, epsilon, keep);
        }

        private static float PerpendicularDistance(
            Vector3 point, Vector3 lineStart, Vector3 lineDir, float lineLenSqr)
        {
            if (lineLenSqr < 0.0001f)
                return Vector3.Distance(point, lineStart);

            Vector3 ap = point - lineStart;
            float t = Vector3.Dot(ap, lineDir) / lineLenSqr;
            Vector3
                projected = lineStart +
                            lineDir * t; // на бесконечной прямой, не клампим — нужна именно прямая дистанция
            return Vector3.Distance(point, projected);
        }
    }
}