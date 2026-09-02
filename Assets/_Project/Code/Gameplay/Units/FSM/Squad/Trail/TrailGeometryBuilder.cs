using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Превращает упрощённый список точек в неизменяемую TrailGeometry.
    ///
    /// Ничего не знает про LineRenderer, NavigationCenter, движение.
    /// Единственная обязанность: список точек → структура сегментов.
    ///
    /// Вызывается один раз при получении нового пути (см. SquadTrailRenderer.ShowPath).
    /// </summary>
    public static class TrailGeometryBuilder
    {
        private static int _versionCounter;

        public static TrailGeometry Build(IReadOnlyList<Vector3> rawPath)
        {
            var simplified = PathSimplifier.Simplify(rawPath);

            if (simplified.Count < 2)
                return TrailGeometry.Invalid;

            var points = simplified.ToArray();
            var segments = new TrailSegment[points.Length - 1];

            for (int i = 0; i < segments.Length; i++)
                segments[i] = new TrailSegment(points[i], points[i + 1]);

            _versionCounter++;
            return new TrailGeometry(points, segments, _versionCounter);
        }
    }
}