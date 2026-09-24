using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    [CreateAssetMenu(
        fileName = "TutorialStep_",
        menuName = "Game Configs/Tutorial/Step")]
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

        [Header("Guidance")]
        public List<TutorialGuidanceDefinition> guidance = new();

        [Header("Reward")]
        
        public TutorialRewardDefinition reward = new();

        [Header("Graph")]
        public List<TutorialTransitionDefinition> transitions = new();

        [Header("Persistence")]
        public bool isCheckpoint = true;

        [Header("Resume Safety")]
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

            // ADDED — camera constraint validation (mode Bounds требует boundsTargetId).
            if (!presentation.Validate(out error))
            {
                error = $"Step '{stepId.DebugKey}': presentation: {error}";
                return false;
            }

            if (!ValidateGuidance(out error))
                return false;

            if (!reward.Validate(stepId, out error))
                return false;

            for (int i = 0; i < transitions.Count; i++)
            {
                var t = transitions[i];

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

        private bool ValidateGuidance(out string error)
        {
            if (guidance == null)
            {
                error = null;
                return true;
            }

            for (int i = 0; i < guidance.Count; i++)
            {
                var g = guidance[i];
                if (g == null)
                {
                    error = $"Step '{stepId.DebugKey}': guidance list has a null entry at index {i}.";
                    return false;
                }

                bool isUnconditional = g.condition == null;
                if (isUnconditional && i != guidance.Count - 1)
                {
                    error = $"Step '{stepId.DebugKey}': unconditional guidance (condition = null) at index {i} " +
                            "makes subsequent guidance entries unreachable — must be last in the list.";
                    return false;
                }

                if (!g.Validate(stepId, i, out error))
                    return false;
            }

            error = null;
            return true;
        }
#endif
    }
}