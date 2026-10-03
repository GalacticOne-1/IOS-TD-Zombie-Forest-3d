using System.Collections.Generic;
using Galactic1.Code.Core.State;
using Galactic1.Code.Systems.Tutorial.Objectives;
using R3;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>Чтение/запись персистентных счётчиков объективов в CGameStateTutorial.
    /// Сам не вызывает SaveGameState — это делает TutorialService.</summary>
    public static class TutorialObjectiveProgressStore
    {
        public static bool TryGet(
            ReactiveProperty<CGameStateTutorial> state,
            string stepGuid, 
            int index, 
            out int value)
        {
            value = 0;
            var saved = state.Value.objectiveProgress;
            if (saved == null) return false;

            foreach (var e in saved)
            {
                if (e.stepId == stepGuid && e.objectiveIndex == index)
                {
                    value = e.value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Пишет счётчики всех persistent-объективов шага. true, если было что писать.</summary>
        public static bool Save(ReactiveProperty<CGameStateTutorial> state, TutorialStepRuntimeState step)
        {
            var stepGuid = step.Definition.stepId.Guid;
            var entries = new List<TutorialObjectiveProgressEntry>();

            var old = state.Value.objectiveProgress;
            if (old != null)
                foreach (var e in old)
                    if (e.stepId != stepGuid) entries.Add(e);

            bool any = false;
            for (int i = 0; i < step.Objectives.Count; i++)
            {
                if (step.Objectives[i].Objective is not IPersistentTutorialObjective p) continue;
                entries.Add(new TutorialObjectiveProgressEntry
                {
                    stepId = stepGuid, objectiveIndex = i, value = p.ProgressValue
                });
                any = true;
            }

            if (!any) return false;
            StateWriter.Write(state, (ref CGameStateTutorial t) => t.objectiveProgress = entries);
            return true;
        }

        public static void Clear(ReactiveProperty<CGameStateTutorial> state, string stepGuid)
        {
            var old = state.Value.objectiveProgress;
            if (old == null || old.Count == 0) return;

            var kept = old.FindAll(e => e.stepId != stepGuid);
            StateWriter.Write(state, (ref CGameStateTutorial t) => t.objectiveProgress = kept);
        }
    }
}