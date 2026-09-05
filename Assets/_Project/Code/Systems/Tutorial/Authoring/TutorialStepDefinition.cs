using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Узел графа тутора. Идентичность узла — stepId (не индекс в списке).
    /// Не содержит императивной логики: поведение шага полностью описывается
    /// objectiveGroup + presentation + transitions + requiredDomain.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TutorialStep_",
        menuName = "Galactic1/Tutorial/Step")]
    public sealed class TutorialStepDefinition : ScriptableObject
    {
        [Tooltip("Стабильный уникальный идентификатор шага (RuntimeId-ассет).")]
        public TutorialStepId stepId;

        [Tooltip("Id главы-владельца. Только для аналитики/дебага.")]
        public TutorialChapterId chapterId;

        [Tooltip("Порядковый индекс для аналитики (step_index).")]
        public int analyticsStepIndex;

        [Header("Objectives")]
        public TutorialObjectiveGroupDefinition objectives = new();

        [Header("Presentation")]
        public TutorialPresentationDefinition presentation = new();

        [Header("Graph")]
        [Tooltip("Переходы из этого шага. Пустой список = терминальный шаг тутора/главы.")]
        public List<TutorialTransitionDefinition> transitions = new();

        [Header("Persistence")]
        [Tooltip("Если true — по завершении шага фиксируется чекпоинт.")]
        public bool isCheckpoint = true;

        [Header("Resume Safety")]
        [Tooltip("Домен, в котором этот шаг безопасно резюмировать. Any = без ограничений.")]
        public TutorialStepDomain requiredDomain = TutorialStepDomain.Any;

#if UNITY_EDITOR
        public bool Validate(out string error)
        {
            if (stepId == null)
            {
                error = $"Step asset '{name}': stepId is empty.";
                return false;
            }

            if (!objectives.Validate(stepId, out error))
                return false;

            for (int i = 0; i < transitions.Count; i++)
            {
                var t = transitions[i];

                // Fix: безусловный transition (condition == null) делает все следующие за ним
                // transitions недостижимыми — "first satisfied wins" отдаёт им приоритет всегда.
                bool isUnconditional = t.condition == null;
                if (isUnconditional && i != transitions.Count - 1)
                {
                    error = $"Step '{stepId.DebugKey}': unconditional transition at index {i} makes " +
                            "subsequent transitions unreachable — must be last in the list.";
                    return false;
                }

                if (t.IsTerminal && i != transitions.Count - 1)
                {
                    error = $"Step '{stepId.DebugKey}': terminal transition must be last in the list.";
                    return false;
                }
            }

            error = null;
            return true;
        }
#endif
    }
}
