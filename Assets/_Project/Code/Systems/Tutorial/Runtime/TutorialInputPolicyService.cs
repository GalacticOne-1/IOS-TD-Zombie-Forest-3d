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
        private readonly TutorialCapabilityPolicy _capabilityPolicy;
        private readonly InteractionPolicyService _interactionPolicyService;

        public TutorialInputPolicy Policy => _policy;
        public TutorialCapabilityPolicy Capabilities => _capabilityPolicy;
        public TutorialInputMode Mode => _policy.Mode;

        public TutorialInputPolicyService(
            InteractionPolicyService interactionPolicyService,
            TutorialCapabilityPolicy capabilityPolicy)
        {
            _interactionPolicyService = interactionPolicyService;
            _capabilityPolicy = capabilityPolicy;
        }

        
        public void Apply(TutorialInputMode mode, TutorialTargetId requiredTargetId = null)
        {
            _policy.Mode = mode;
            _policy.RequiredTargetId = requiredTargetId;

            switch (mode)
            {
                case TutorialInputMode.Free:
                case TutorialInputMode.Restricted:
                    _interactionPolicyService.Reset();
                    break;
                case TutorialInputMode.RequiredAction:
                case TutorialInputMode.Blocked:
                    _interactionPolicyService.DisableAll();
                    break;
            }
        }

        /// <summary>ADDED — единая точка применения composable capabilities для шага
        /// (см. TutorialCapabilityPolicy докстринг). Вызывается из
        /// TutorialService.ActivateStep рядом с существующим Apply(inputPolicy).</summary>
        public void ApplyCapabilities(bool canMove, bool canControlCamera, bool canInteract, bool canUseAbilities)
            => _capabilityPolicy.Set(canMove, canControlCamera, canInteract, canUseAbilities);

        
        public void Reset()
        {
            _policy.Reset();
            _capabilityPolicy.Reset();
            _interactionPolicyService.Reset();
        }

        // UNCHANGED
        public bool IsActionRequiredFor(string targetId)
            => _policy.Mode == TutorialInputMode.RequiredAction
               && _policy.RequiredTargetId != null
               && _policy.RequiredTargetId.Guid == targetId;
    }
}
