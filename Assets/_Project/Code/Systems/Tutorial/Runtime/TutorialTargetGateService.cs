
using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Реализация ITutorialTargetGateService. Семантика (см. class docstring в ТЗ):
    ///   - targetId == null                      → Allowed (нечего проверять)
    ///   - gate не сконфигурирован для target     → Allowed (target без gate работает как раньше)
    ///   - requiredStepId в gate не задан          → Allowed + LogError (misconfiguration,
    ///                                                 fail-open, не блокируем молча навсегда)
    ///   - !ITutorialService.IsActive              → Allowed (тутор не начат ИЛИ кампания уже
    ///                                                 полностью завершена — оба случая дают
    ///                                                 IsActive == false, см. TutorialRuntime.IsActive)
    ///   - IsStepCompleted(requiredStepId) == true → Allowed
    ///   - иначе                                   → Blocked(gate.blockedMessage)
    ///
    /// Soft Launch: один target → один requiredStepId. Расширение на AND/OR или несколько
    /// требований — замена TutorialTargetGateDefinition.requiredStepId на список условий
    /// внутри этого же сервиса, без изменения ITutorialTargetGateService контракта наружу.
    /// </summary>
    public sealed class TutorialTargetGateService : ITutorialTargetGateService
    {
        private readonly TutorialTargetGateRegistry _registry;
        private readonly ITutorialService _tutorialService;

        public TutorialTargetGateService(
            TutorialTargetGateRegistry registry,
            ITutorialService tutorialService)
        {
            _registry = registry;
            _tutorialService = tutorialService;
        }

        public TutorialTargetGateResult Evaluate(TutorialTargetId targetId)
        {
            if (targetId == null)
                return TutorialTargetGateResult.Allowed;

            if (!_registry.TryGetGate(targetId, out var gate))
                return TutorialTargetGateResult.Allowed;

            if (gate.requiredStepId == null)
            {
                Debug.LogError($"[TutorialTargetGateService] Gate for target '{targetId.DebugKey}' " +
                               "has no requiredStepId — treating as unlocked. Fix the gate asset.");
                return TutorialTargetGateResult.Allowed;
            }

            if (!_tutorialService.IsActive)
                return TutorialTargetGateResult.Allowed;

            if (_tutorialService.IsStepCompleted(gate.requiredStepId))
                return TutorialTargetGateResult.Allowed;

            return TutorialTargetGateResult.Blocked(ResolveMessage(gate));
        }

        /// <summary>Fix: DebugKey — служебный идентификатор ассета, не предназначен для
        /// показа игроку (см. ТЗ). Если useStepTitle=true, но заголовок шага недоступен
        /// (кампания сменилась/шаг не в активном графе/поле пустое в authoring) — падаем
        /// на фиксированный generic-текст, а не на DebugKey и не на пустую строку.</summary>
        private string ResolveMessage(TutorialTargetGateDefinition gate)
        {
            if (!gate.useStepTitle)
                return gate.blockedMessage;

            var titleKey = _tutorialService.GetStepTitleKey(gate.requiredStepId);
            if (string.IsNullOrEmpty(titleKey))
            {
                Debug.LogWarning($"[TutorialTargetGateService] useStepTitle=true for target " +
                                 $"'{gate.targetId?.DebugKey}', but step title is unavailable " +
                                 $"(step '{gate.requiredStepId.DebugKey}' not in active campaign " +
                                 "graph, or its presentation.instructionTitleKey is empty).");
                return "Сначала выполните текущее задание обучения.";
            }

            return string.Format(gate.messageTemplate, titleKey);
        }
    }
}