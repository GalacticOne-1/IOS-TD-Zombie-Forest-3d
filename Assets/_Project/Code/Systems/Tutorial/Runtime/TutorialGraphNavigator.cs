using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>Различает три исхода резолва перехода — раньше это был единственный
    /// string, и null означал одновременно "терминальный шаг" и "ни одно условие не
    /// подошло", что заставляло TutorialService ошибочно завершать кампанию во втором
    /// случае. Критический фикс.</summary>
    public enum TutorialGraphResult
    {
        NextStep,
        Terminal,
        NoTransitionMatched
    }

    public readonly struct TutorialTransitionResult
    {
        public readonly TutorialGraphResult Result;
        /// <summary>Валиден только при Result == NextStep.</summary>
        public readonly TutorialStepId NextStepId;

        private TutorialTransitionResult(TutorialGraphResult result, TutorialStepId nextStepId)
        {
            Result = result;
            NextStepId = nextStepId;
        }

        public static TutorialTransitionResult NextStep(TutorialStepId stepId) => new(TutorialGraphResult.NextStep, stepId);
        public static TutorialTransitionResult Terminal() => new(TutorialGraphResult.Terminal, null);
        public static TutorialTransitionResult NoMatch() => new(TutorialGraphResult.NoTransitionMatched, null);
    }

    /// <summary>
    /// Единственный владелец графовой навигации тутора (переходы, условные ветки).
    /// Ни TutorialCheckpointService, ни TutorialService не дублируют эту логику —
    /// оба вызывают Resolve для ОДНОГО шага за раз, никогда не проходят граф сами.
    /// </summary>
    public sealed class TutorialGraphNavigator
    {
        /// <summary>
        /// Terminal — шаг без transitions, либо первый удовлетворённый transition терминальный.
        /// NoTransitionMatched — transitions есть, но ни одно condition не удовлетворено
        /// (и нет безусловного fallback — см. TutorialStepDefinition.Validate, п.3).
        /// NextStep — обычный переход.
        /// </summary>
        public TutorialTransitionResult Resolve(TutorialStepDefinition current)
        {
            if (current.transitions == null || current.transitions.Count == 0)
                return TutorialTransitionResult.Terminal();

            foreach (var t in current.transitions)
            {
                if (t.IsSatisfied())
                    return t.IsTerminal ? TutorialTransitionResult.Terminal() : TutorialTransitionResult.NextStep(t.nextStepId);
            }

            return TutorialTransitionResult.NoMatch();
        }
    }
}
