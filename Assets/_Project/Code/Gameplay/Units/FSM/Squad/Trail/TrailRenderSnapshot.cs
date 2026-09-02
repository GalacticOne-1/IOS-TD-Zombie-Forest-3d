using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Неизменяемый снимок состояния, нужного для отрисовки пути,
    /// на момент конкретного Tick.
    ///
    /// FormationCenterDriver — единственный источник всех полей.
    /// Этот struct не хранит собственного состояния между кадрами,
    /// он просто упаковывает то, чем driver уже владеет, для передачи
    /// в визуализацию без раскрытия внутренних приватных полей
    /// (_path, _segment) напрямую.
    /// </summary>
    public readonly struct TrailRenderSnapshot
    {
        public readonly TrailGeometry Geometry;
        public readonly int CurrentSegment;
        public readonly Vector3 VisualCenter;
        public readonly Vector3 NavigationCenter;
        public readonly Vector3 Forward;
        public readonly bool IsValid;

        public TrailRenderSnapshot(
            TrailGeometry geometry,
            int currentSegment,
            Vector3 navigationCenter,
            Vector3 visualCenter,
            Vector3 forward)
        {
            Geometry = geometry;
            CurrentSegment = currentSegment;
            NavigationCenter = navigationCenter;
            Forward = forward;
            VisualCenter = visualCenter;
            IsValid = geometry != null && geometry.IsValid;
        }

        public static readonly TrailRenderSnapshot Invalid = default;
    }
}