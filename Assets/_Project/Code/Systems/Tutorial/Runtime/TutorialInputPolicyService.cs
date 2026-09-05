using Galactic1.Code.Systems.Interaction;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Runtime-сервис input-политики тутора. Структурный аналог InteractionPolicyService:
    /// не принимает решений, не знает про шаги/объективы. Restricted/Blocked транслируются
    /// в существующий InteractionPolicyService — тутор не создаёт второй gate для
    /// взаимодействий, а переиспользует уже принятую в проекте точку контроля.
    /// </summary>
    public sealed class TutorialInputPolicyService : IGameService
    {
        private readonly TutorialInputPolicy _policy = new();
        private readonly InteractionPolicyService _interactionPolicyService;

        public TutorialInputPolicy Policy => _policy;
        public TutorialInputMode Mode => _policy.Mode;

        public TutorialInputPolicyService(InteractionPolicyService interactionPolicyService)
        {
            _interactionPolicyService = interactionPolicyService;
        }

        public void Apply(TutorialInputMode mode, TutorialTargetId requiredTargetId = null)
        {
            _policy.Mode = mode;
            _policy.RequiredTargetId = requiredTargetId;

            switch (mode)
            {
                case TutorialInputMode.Free:
                case TutorialInputMode.Restricted:
                    // Fix: Restricted раньше не делал ничего, из-за чего DisableAll от
                    // предыдущего Blocked-шага оставался активным (soft guidance ≠ "не трогать
                    // предыдущее состояние"). Restricted тоже обязан явно установить свой
                    // baseline — взаимодействия доступны, ограничение выражается презентацией
                    // (хинты), а не блокировкой ввода.
                    _interactionPolicyService.Reset();
                    break;
                case TutorialInputMode.RequiredAction:
                case TutorialInputMode.Blocked:
                    _interactionPolicyService.DisableAll();
                    break;
            }
        }

        public void Reset()
        {
            _policy.Reset();
            _interactionPolicyService.Reset();
        }

        /// <summary>Вызывающая сторона (interaction-система) оперирует сырым string targetId,
        /// как и остальные gameplay-события/точки интеграции (см. ButtonPressedObjective) —
        /// сравниваем через RequiredTargetId.Guid, а не сам RuntimeId-ассет.</summary>
        public bool IsActionRequiredFor(string targetId)
            => _policy.Mode == TutorialInputMode.RequiredAction
               && _policy.RequiredTargetId != null
               && _policy.RequiredTargetId.Guid == targetId;
    }
}
