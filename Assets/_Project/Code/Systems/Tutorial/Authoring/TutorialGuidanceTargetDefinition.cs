using System;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Все presentation-таргеты шага (highlight/arrow/camera) теперь живут ИСКЛЮЧИТЕЛЬНО
    /// здесь — TutorialPresentationDefinition больше не содержит targeting-полей вообще
    /// (см. её текущую версию). Шаг без guidance-списка не имеет никакого таргетинга —
    /// legacy single-target fallback больше не существует.
    /// </summary>
    [Serializable]
    public sealed class TutorialGuidanceTargetDefinition
    {
        [SerializeReference]
        [Tooltip("Способ поиска highlight-таргета для этого guidance-варианта. Null = " +
                 "highlight не нужен для этой ветки condition.")]
        public TutorialTargetQuery highlightTarget;

        [Tooltip("Оставь пустым, если для этого guidance-варианта стрелка не нужна.")]
        public TutorialTargetId arrowTargetId;

        [Tooltip("Оставь пустым, если для этого guidance-варианта фокус камеры не нужен.")]
        public TutorialTargetId cameraFocusTargetId;
        public TutorialTargetId cameraBoundsTargetId;

        public bool HasAnyTarget =>
            highlightTarget != null || arrowTargetId != null || cameraFocusTargetId != null;

#if UNITY_EDITOR
        public bool Validate(out string error)
        {
            if (highlightTarget != null && !highlightTarget.Validate(out error))
                return false;

            error = null;
            return true;
        }
#endif
    }
}