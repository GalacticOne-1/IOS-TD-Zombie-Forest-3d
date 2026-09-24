using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Один guidance-вариант шага: "если condition истинно — показать presentation".
    /// Список guidance-вариантов шага (TutorialStepDefinition.guidance) упорядочен —
    /// побеждает ПЕРВЫЙ элемент с истинным condition (тот же "first satisfied wins"
    /// принцип, что у TutorialTransitionDefinition для графовых переходов). condition == null
    /// означает "безусловно истинно" — тот же authoring-паттерн, что у
    /// TutorialTransitionDefinition.condition; полезен как последний fallback-вариант
    /// в списке, или как единственный вариант для простого шага с одним статичным target
    /// (см. TutorialStepDefinition.guidance докстринг про backward compatibility с
    /// legacy single-highlight шагами).
    ///
    /// ВАЖНО: guidance НИКОГДА не завершает шаг и не знает про Objectives — это чисто
    /// presentation-уровневая штука (см. TutorialStepRuntimeState/TutorialGuidanceRuntimeState
    /// докстринги). Она также не персистится: после Restore/Restart guidance всегда
    /// резолвится заново из текущего game state — никакого "CurrentGuidanceIndex" в сейве нет.
    /// </summary>
    [Serializable]
    public sealed class TutorialGuidanceDefinition
    {
        [Tooltip("Пусто = условие всегда истинно (см. класс-докстринг).")]
        public TutorialGuidanceConditionDefinition condition;
        
        [Tooltip("Опционально. Overlay-панель с текстом для этого guidance-entry — см. " +
                 "TutorialGuidancePanelDefinition докстринг. Использует тот же condition, что " +
                 "highlight-таргет этого entry, но резолвится отдельным параллельным каналом.")]
        public TutorialGuidancePanelDefinition descriptionPanel = new();

        [Tooltip("Targets, которые будут показаны одновременно, если condition выполнен.")]
        public List<TutorialGuidanceTargetDefinition> presentations = new();
        

#if UNITY_EDITOR
        public bool Validate(TutorialStepId stepId, int index, out string error)
        {
            var stepLabel = stepId?.DebugKey ?? "?";

            if (presentations == null || presentations.Count == 0)
            {
                error =
                    $"Step '{stepLabel}': guidance entry {index} has no presentation targets.";

                return false;
            }

            for (int i = 0; i < presentations.Count; i++)
            {
                var presentation = presentations[i];

                if (presentation == null)
                {
                    error =
                        $"Step '{stepLabel}': guidance entry {index} " +
                        $"presentation {i} is null.";

                    return false;
                }

                if (!presentation.HasAnyTarget)
                {
                    error =
                        $"Step '{stepLabel}': guidance entry {index} " +
                        $"presentation {i} has no target " +
                        "(highlight/arrow/camera all empty).";

                    return false;
                }

                if (!presentation.Validate(out error))
                {
                    error =
                        $"Step '{stepLabel}': guidance entry {index}, " +
                        $"presentation {i}: {error}";

                    return false;
                }
            }

            if (condition != null && !condition.Validate(out error))
            {
                error =
                    $"Step '{stepLabel}': guidance entry {index}: {error}";

                return false;
            }

            error = null;
            return true;
        }
#endif
    }
}
