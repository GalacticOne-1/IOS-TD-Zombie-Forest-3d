using System;
using System.Collections.Generic;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Опциональная конфигурация наград шага. Награды выдаются через Inbox
    /// (TutorialRewardService → InboxService) СТРОГО при генуинном завершении шага —
    /// не при показе, не при Skip (см. TutorialService.ResolveTransition).
    /// Пустой список — валидное состояние ("без награды").
    /// </summary>
    [Serializable]
    public sealed class TutorialRewardDefinition
    {
        public List<TutorialRewardItemDefinition> items = new();

        public bool HasRewards => items != null && items.Count > 0;

#if UNITY_EDITOR
        public bool Validate(TutorialStepId stepId, out string error)
        {
            var stepLabel = stepId?.DebugKey ?? "?";

            if (items == null) { error = null; return true; }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    error = $"Step '{stepLabel}': reward list has a null entry at index {i}.";
                    return false;
                }
            }

            error = null;
            return true;
        }
#endif
    }
}