using UnityEngine;

namespace Galactic1.Code.Gameplay.Targeting
{
    /// <summary>
    /// UI-зона, попадание в которую во время targeting-жеста
    /// трактуется как отмена вместо подтверждения.
    /// TargetingInputPipeline знает только этот контракт,
    /// не знает ничего про HUD.
    /// </summary>
    public interface ITargetingCancelZone
    {
        bool ContainsScreenPoint(Vector2 screenPosition);
        void SetHighlighted(bool highlighted);
    }
}