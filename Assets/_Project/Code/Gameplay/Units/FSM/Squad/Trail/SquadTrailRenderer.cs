using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Единственный класс, который знает про LineRenderer.
    ///
    /// На новый путь (Geometry.Version изменился):
    ///   один раз вызывает SetPositions() с полным упрощённым polyline.
    ///
    /// Каждый Tick:
    ///   обновляет только вершину 0 (видимое начало линии),
    ///   не трогая остальной массив, не аллоцируя.
    ///
    /// Источник геометрии — SquadMovementSystem.Geometry (через
    /// FormationCenterDriver). Этот класс её не строит и не хранит
    /// состояние пути — только читает снимок и кэширует последнюю версию.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class SquadTrailRenderer : MonoBehaviour
    {
        [SerializeField] private float startOffset = 1.5f;
        [SerializeField] private float lineHeight = 0.2f;
        private Transform targetMarker;

        private LineRenderer _lineRenderer;
        private FormationCenterDriver _centerDriver;

        private int _appliedGeometryVersion = -1;
        private Vector3[] _renderPoints;
        private bool _pathVisible;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = 0;
            _lineRenderer.enabled = false;

            targetMarker = transform.Find("target_marker");
            targetMarker.gameObject.SetActive(false);
        }

        public void Bind(FormationCenterDriver centerDriver)
        {
            _centerDriver = centerDriver;
        }

        public void ShowPath()
        {
            _pathVisible = true;
            _lineRenderer.enabled = true;
            _appliedGeometryVersion = -1;
        }

        public void HidePath()
        {
            _pathVisible = false;
            _lineRenderer.enabled = false;

            if (targetMarker != null)
                targetMarker.gameObject.SetActive(false);
        }

        public void Tick()
        {
            if (!_pathVisible || _centerDriver == null) return;

            TrailRenderSnapshot snapshot = _centerDriver.RenderSnapshot;
            if (!snapshot.IsValid)
            {
                _lineRenderer.positionCount = 0;
                return;
            }

            if (snapshot.Geometry.Version != _appliedGeometryVersion)
            {
                RebuildLine(snapshot.Geometry);
                UpdateTargetMarker(snapshot.Geometry);
                _appliedGeometryVersion = snapshot.Geometry.Version;
            }

            TrailTrimResult result = TrailTrimCalculator.Compute(snapshot, startOffset);
            if (!result.IsValid) return;

            _lineRenderer.SetPosition(0, WithLineHeight(result.VisibleStartPoint));
            HideConsumedSegments(result.FirstVisibleSegmentIndex);
        }

        private void RebuildLine(TrailGeometry geometry)
        {
            int count = geometry.PointCount;

            if (_renderPoints == null || _renderPoints.Length != count)
                _renderPoints = new Vector3[count];

            for (int i = 0; i < count; i++)
                _renderPoints[i] = WithLineHeight(geometry.GetPoint(i));

            _lineRenderer.positionCount = count;
            _lineRenderer.SetPositions(_renderPoints);
        }

        /// <summary>
        /// Ставит маркер в финальную точку упрощённой геометрии пути.
        /// Вызывается только при смене геометрии (новый путь), не каждый тик —
        /// финальная нода не двигается, пока путь не пересчитан заново.
        /// </summary>
        private void UpdateTargetMarker(TrailGeometry geometry)
        {
            if (targetMarker == null) return;
            if (!geometry.IsValid) return;

            Vector3 finalPoint = geometry.GetPoint(geometry.PointCount - 1);
            targetMarker.position = WithLineHeight(finalPoint);
            targetMarker.gameObject.SetActive(true);
        }

        private void HideConsumedSegments(int firstVisibleSegmentIndex)
        {
            for (int i = 1; i <= firstVisibleSegmentIndex; i++)
                _lineRenderer.SetPosition(i, _lineRenderer.GetPosition(0));
        }

        private Vector3 WithLineHeight(Vector3 point) => new Vector3(point.x, lineHeight, point.z);
    }
}