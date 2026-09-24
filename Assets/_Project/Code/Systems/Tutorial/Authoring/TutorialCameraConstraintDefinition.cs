using System;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>Authoring-описание "в какой области можно двигать камеру на этом шаге" —
    /// не хранит world-координаты напрямую (TutorialStepDefinition — persistent asset,
    /// координаты принадлежат конкретной сцене), только ссылку на scene-authored
    /// TutorialCameraBoundsBehaviour через тот же TutorialTargetId identity mechanism,
    /// что уже используют arrow/camera-focus таргеты.</summary>
    [Serializable]
    public sealed class TutorialCameraConstraintDefinition
    {
        public TutorialCameraConstraintMode mode = TutorialCameraConstraintMode.None;

        [Tooltip("Обязателен при mode = Bounds. Ссылается на TutorialCameraBoundsBehaviour " +
                 "на сцене с тем же TutorialTargetId.")]
        public TutorialTargetId boundsTargetId;

#if UNITY_EDITOR
        public bool Validate(out string error)
        {
            if (mode == TutorialCameraConstraintMode.Bounds && boundsTargetId == null)
            {
                error = "Camera constraint mode is Bounds, but boundsTargetId is not assigned.";
                return false;
            }

            error = null;
            return true;
        }
#endif
    }
}