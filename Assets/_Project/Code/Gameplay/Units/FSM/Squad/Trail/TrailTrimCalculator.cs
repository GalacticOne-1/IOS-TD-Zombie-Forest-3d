using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Вычисляет начало отображаемой линии.
    /// Работает только с упрощённой геометрией, поэтому не зависит
    /// от сегментов исходного A* пути.
    /// </summary>
    public static class TrailTrimCalculator
    {
        public static TrailTrimResult Compute(TrailRenderSnapshot snapshot, float startOffset)
        {
            if (!snapshot.IsValid)
                return TrailTrimResult.Invalid;

            TrailGeometry geometry = snapshot.Geometry;

            Vector3 desiredStart =
                snapshot.VisualCenter +
                snapshot.Forward * startOffset;

            int bestSegment = 0;
            Vector3 bestPoint = default;
            float bestSqr = float.MaxValue;

            // Полный поиск по геометрии вместо окна вокруг NavigationCenter.CurrentSegment —
            // VisualCenter может отставать от NavigationCenter дальше, чем окно SearchBehind/SearchAhead.
            for (int i = 0; i < geometry.SegmentCount; i++)
            {
                TrailSegment seg = geometry.GetSegment(i);
                Vector3 projected = ProjectOnSegment(seg, desiredStart);
                float sqr = (projected - desiredStart).sqrMagnitude;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    bestPoint = projected;
                    bestSegment = i;
                }
            }

            return new TrailTrimResult(bestPoint, bestSegment);
        }

        private static Vector3 ProjectOnSegment(
            TrailSegment segment,
            Vector3 point)
        {
            Vector3 ap = point - segment.PointA;

            float t = Mathf.Clamp(
                Vector3.Dot(ap, segment.Direction),
                0f,
                segment.Length);

            return segment.PointA + segment.Direction * t;
        }
    }
}