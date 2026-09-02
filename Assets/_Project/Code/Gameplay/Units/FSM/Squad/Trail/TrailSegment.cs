using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Один сегмент упрощённого пути.
    /// PointB — конечная точка сегмента (начальная берётся из предыдущего
    /// сегмента или Points[0]).
    /// </summary>
    public readonly struct TrailSegment
    {
        public readonly Vector3 PointA;
        public readonly Vector3 PointB;
        public readonly Vector3 Direction;   // нормализован, PointA → PointB
        public readonly float Length;

        public TrailSegment(Vector3 a, Vector3 b)
        {
            PointA = a;
            PointB = b;
            Vector3 delta = b - a;
            Length = delta.magnitude;
            Direction = Length > 0.0001f ? delta / Length : Vector3.forward;
        }
    }

    /// <summary>
    /// Полностью неизменяемая геометрия упрощённого пути.
    ///
    /// Строится один раз TrailGeometryBuilder'ом и больше никогда
    /// не модифицируется. Любое отображение (какая часть видна,
    /// откуда начинается линия) вычисляется отдельно в
    /// TrailTrimCalculator, не трогая этот объект.
    ///
    /// Version — дешёвый способ для SquadTrailRenderer понять "это новый
    /// путь" (нужно SetPositions) против "тот же путь, просто Tick"
    /// (трогаем только первую вершину).
    /// </summary>
    public sealed class TrailGeometry
    {
        public static readonly TrailGeometry Invalid = new TrailGeometry();

        private readonly Vector3[] _points;
        private readonly TrailSegment[] _segments;
        private readonly float[] _cumulativeLengthAtSegmentStart;

        public bool IsValid { get; }
        public int Version { get; }
        public int PointCount => _points?.Length ?? 0;
        public int SegmentCount => _segments?.Length ?? 0;

        public IReadOnlyList<Vector3> Points => _points;
        public IReadOnlyList<TrailSegment> Segments => _segments;

        // Приватный конструктор для Invalid
        private TrailGeometry()
        {
            IsValid = false;
            Version = 0;
            _points = System.Array.Empty<Vector3>();
            _segments = System.Array.Empty<TrailSegment>();
            _cumulativeLengthAtSegmentStart = System.Array.Empty<float>();
        }

        /// <summary>
        /// Внутренний конструктор — вызывается только TrailGeometryBuilder'ом.
        /// </summary>
        internal TrailGeometry(Vector3[] points, TrailSegment[] segments, int version)
        {
            _points = points;
            _segments = segments;
            Version = version;
            IsValid = points.Length >= 2 && segments.Length >= 1;

            _cumulativeLengthAtSegmentStart = new float[segments.Length];
            float acc = 0f;
            for (int i = 0; i < segments.Length; i++)
            {
                _cumulativeLengthAtSegmentStart[i] = acc;
                acc += segments[i].Length;
            }
        }

        public Vector3 GetPoint(int index) => _points[index];
        public TrailSegment GetSegment(int index) => _segments[index];

        /// <summary>Суммарная длина пути от начала Points[0] до начала сегмента index.</summary>
        public float CumulativeLengthAtSegmentStart(int index) =>
            _cumulativeLengthAtSegmentStart[index];
    }
}