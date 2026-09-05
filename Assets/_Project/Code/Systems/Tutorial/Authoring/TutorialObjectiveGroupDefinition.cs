using System;
using System.Collections.Generic;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Композиция объективов одного шага. Плоский список + режим ALL/ANY —
    /// достаточно для первого тутора, вложенные группы намеренно не поддерживаются.
    /// </summary>
    [Serializable]
    public sealed class TutorialObjectiveGroupDefinition
    {
        public ObjectiveCompositionMode mode = ObjectiveCompositionMode.All;
        public List<TutorialObjectiveDefinition> objectives = new();

#if UNITY_EDITOR
        public bool Validate(TutorialStepId stepId, out string error)
        {
            var stepLabel = stepId?.DebugKey ?? "?";

            if (objectives == null || objectives.Count == 0)
            {
                error = $"Step '{stepLabel}': objective group is empty.";
                return false;
            }

            for (int i = 0; i < objectives.Count; i++)
            {
                if (objectives[i] == null)
                {
                    error = $"Step '{stepLabel}': objective group has a null entry at index {i}.";
                    return false;
                }

                if (!objectives[i].Validate(out var objError))
                {
                    error = $"Step '{stepLabel}': {objError}";
                    return false;
                }
            }

            error = null;
            return true;
        }
#endif
    }
}
