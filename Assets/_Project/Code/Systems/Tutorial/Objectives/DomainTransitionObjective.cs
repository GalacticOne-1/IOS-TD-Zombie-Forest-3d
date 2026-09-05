using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// СТРОГАЯ transition-семантика: "только что перешли из X в Y". НЕТ ретроактивного
    /// завершения — "уже находимся в Y" не считается доказательством "только что перешли
    /// из X" (используется, например, для ReturnedToWorldMap: from=Raid, to=WorldMap).
    /// </summary>
    public sealed class DomainTransitionObjective : ITutorialObjective
    {
        private readonly IGameLoopStateQuery _query;
        private readonly TutorialStepDomain _from, _to;
        private System.Action _onProgressChanged;
        public bool IsCompleted { get; private set; }

        public DomainTransitionObjective(IGameLoopStateQuery query, TutorialStepDomain from, TutorialStepDomain to)
        {
            _query = query;
            _from = from;
            _to = to;
        }

        public void Start(System.Action onProgressChanged)
        {
            _onProgressChanged = onProgressChanged;
            // Намеренно без shortcut на EvaluateCurrentState() — см. класс-докстринг.
            _query.OnDomainTransition += OnTransition;
        }

        public void Stop() => _query.OnDomainTransition -= OnTransition;

        private void OnTransition(TutorialStepDomain from, TutorialStepDomain to)
        {
            if (!IsCompleted && from == _from && to == _to)
            {
                IsCompleted = true;
                _onProgressChanged?.Invoke();
            }
        }

        public bool EvaluateCurrentState() => false;
        public bool EvaluateEvent(object gameplayEvent) => false;
    }
}
