using Galactic1.Code.Core.State;
using R3;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Отвечает ТОЛЬКО на вопрос "можно ли безопасно резюмировать сохранённую позицию" —
    /// НИКОГДА не ищет другую позицию форвард-поиском по графу (это был P0-баг, который
    /// позволял пропускать обязательные RAID-шаги, прыгая сразу на безопасный WORLD_MAP-шаг
    /// в конце графа). Единственный вызов графовой навигации — РОВНО ОДИН шаг вперёд через
    /// TutorialGraphNavigator, то есть та же операция, что использует обычная прогрессия.
    ///
    /// snapshot.currentStepId/checkpointStepId — сырые guid-строки (CGameStateTutorial не
    /// хранит ScriptableObject-ссылки), резолвятся в TutorialStepDefinition через
    /// TutorialDefinition.GetStepByGuid. Дальше по графу — уже только typed TutorialStepId.
    /// </summary>
    public sealed class TutorialCheckpointService
    {
        public TutorialResumeDecision ResolveResume(
            TutorialDefinition definition,
            CGameStateTutorial snapshot,
            TutorialStepDomain currentDomain,
            TutorialGraphNavigator navigator)
        {
            var currentStep = string.IsNullOrEmpty(snapshot.currentStepId)
                ? null : definition.GetStepByGuid(snapshot.currentStepId);

            bool currentAlreadyCompleted = currentStep != null
                && snapshot.completedStepIds != null
                && snapshot.completedStepIds.Contains(currentStep.stepId.Guid);

            // Case A — сохранённая позиция валидна, не завершена, безопасна здесь и сейчас.
            if (currentStep != null && !currentAlreadyCompleted && IsSafeToResume(currentStep, currentDomain))
                return TutorialResumeDecision.ResumeCurrent(currentStep.stepId);

            // Legacy/Test4: currentStepId уже в completedStepIds. НЕ переигрываем его —
            // продолжаем строго на один шаг вперёд по графу (обычная прогрессия, не поиск).
            if (currentAlreadyCompleted)
            {
                var resolved = ResolveOneStepForward(definition, currentStep, currentDomain, navigator, out var reason);
                if (resolved != null)
                    return TutorialResumeDecision.ContinueFromResolvedProgress(resolved);

                return TutorialResumeDecision.Restart(BuildFallbackContext(
                    snapshot, currentDomain, reason ?? "Resolved-position successor is unsafe or missing."));
            }

            // Case B — сохранённая позиция небезопасна/отсутствует. Единственная допустимая
            // проверка — безопасен ли шаг СРАЗУ ПОСЛЕ чекпоинта. Если нет — STOP, никакого
            // дальнейшего поиска вперёд по графу.
            var checkpointStep = string.IsNullOrEmpty(snapshot.checkpointStepId)
                ? null : definition.GetStepByGuid(snapshot.checkpointStepId);

            if (checkpointStep != null)
            {
                var candidate = ResolveOneStepForward(definition, checkpointStep, currentDomain, navigator, out var reason);

                if (candidate != null)
                    return TutorialResumeDecision.ResumeFromCheckpoint(candidate);

                return TutorialResumeDecision.Restart(BuildFallbackContext(
                    snapshot, currentDomain,
                    reason ?? $"Step immediately after checkpoint '{checkpointStep.stepId.DebugKey}' is unsafe in domain {currentDomain}."));
            }

            return TutorialResumeDecision.Restart(BuildFallbackContext(
                snapshot, currentDomain, "No currentStepId and no checkpointStepId available."));
        }

        /// <summary>
        /// РОВНО ОДИН шаг вперёд — не цикл, не поиск. Возвращает null если следующего
        /// шага нет ИЛИ он небезопасен — вызывающий код обязан остановиться в обоих случаях.
        /// </summary>
        private TutorialStepDefinition ResolveOneStepForward(
            TutorialDefinition definition,
            TutorialStepDefinition from,
            TutorialStepDomain currentDomain,
            TutorialGraphNavigator navigator,
            out string unsafeReason)
        {
            unsafeReason = null;

            // Fix: используем тот же различающий Terminal/NoTransitionMatched результат,
            // что и обычная прогрессия — раньше здесь тоже терялось различие через null.
            var result = navigator.Resolve(from);

            if (result.Result == TutorialGraphResult.Terminal)
            {
                unsafeReason = $"'{from.stepId?.DebugKey}' is a terminal step (no successor).";
                return null;
            }

            if (result.Result == TutorialGraphResult.NoTransitionMatched)
            {
                unsafeReason = $"No transition condition matched after '{from.stepId?.DebugKey}' — cannot resolve resume successor.";
                return null;
            }

            var nextStep = definition.GetStep(result.NextStepId);
            if (nextStep == null)
            {
                unsafeReason = $"Successor stepId '{result.NextStepId?.DebugKey}' not found in definition.";
                return null;
            }

            if (!IsSafeToResume(nextStep, currentDomain))
            {
                unsafeReason = $"Successor '{nextStep.stepId?.DebugKey}' requires domain {nextStep.requiredDomain}, " +
                                $"current domain is {currentDomain}.";
                return null;
            }

            return nextStep;
        }

        public bool IsSafeToResume(TutorialStepDefinition step, TutorialStepDomain currentDomain)
            => step != null && (step.requiredDomain == TutorialStepDomain.Any || step.requiredDomain == currentDomain);

        public void MarkCheckpoint(ReactiveProperty<CGameStateTutorial> state, TutorialStepId stepId)
            => StateWriter.Write(state, (ref CGameStateTutorial t) => t.checkpointStepId = stepId?.Guid);

        public void ClearCheckpoint(ReactiveProperty<CGameStateTutorial> state)
            => StateWriter.Write(state, (ref CGameStateTutorial t) => t.checkpointStepId = null);

        private TutorialResumeFallbackContext BuildFallbackContext(
            CGameStateTutorial snapshot, TutorialStepDomain currentDomain, string reason)
            => new(snapshot.currentStepId, snapshot.checkpointStepId, currentDomain, reason);
    }
}
