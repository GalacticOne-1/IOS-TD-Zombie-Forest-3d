using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Результат расчёта видимой части линии за один Tick.
    /// Не содержит ссылок на Unity-компоненты — только данные.
    /// </summary>
    public readonly struct TrailTrimResult
    {
        public readonly Vector3 VisibleStartPoint;
        public readonly int FirstVisibleSegmentIndex;
        public readonly bool IsValid;

        public TrailTrimResult(Vector3 visibleStartPoint, int firstVisibleSegmentIndex)
        {
            VisibleStartPoint = visibleStartPoint;
            FirstVisibleSegmentIndex = firstVisibleSegmentIndex;
            IsValid = true;
        }

        public static readonly TrailTrimResult Invalid = default;
    }
}