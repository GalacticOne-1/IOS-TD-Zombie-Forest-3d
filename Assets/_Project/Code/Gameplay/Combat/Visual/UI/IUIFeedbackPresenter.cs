using Galactic1.Code.Gameplay.Combat.Data;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.UI
{
    /// <summary>
    /// Contract for the UI layer to receive combat feedback.
    ///
    /// Implement this in your UI presenter/view layer.
    /// UIFeedbackSystem depends only on this interface —
    /// gameplay has zero coupling to concrete UI classes.
    /// </summary>
    public interface IUIFeedbackPresenter
    {
        /// <summary>
        /// Show a floating damage number at world position.
        /// </summary>
        void ShowDamageNumber(Vector3 worldPosition, float damage, BodyPartType bodyPart);

        /// <summary>
        /// Show a kill indicator at world position.
        /// </summary>
        void ShowKillIndicator(Vector3 worldPosition);

        /// <summary>
        /// Show or update a suppression marker for a unit.
        /// </summary>
        void ShowSuppressionMarker(IUnitSceneContext target, float amount);
    }
}