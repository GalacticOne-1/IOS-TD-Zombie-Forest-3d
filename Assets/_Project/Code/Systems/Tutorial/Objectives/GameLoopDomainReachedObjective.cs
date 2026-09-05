using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// "Сейчас находимся в домене X" — retroactive-семантика (может завершиться
    /// немедленно, если игрок уже там). Для transition-семантики ("только что
    /// перешли из RAID в WORLD_MAP") использовать DomainTransitionObjective.
    /// </summary>
    public sealed class GameLoopDomainReachedObjective : ITutorialObjective
    {
        private readonly IGameLoopStateQuery _query;
        private readonly TutorialStepDomain _targetDomain;
        private System.Action _onProgressChanged;
        public bool IsCompleted { get; private set; }

        public GameLoopDomainReachedObjective(IGameLoopStateQuery query, TutorialStepDomain targetDomain)
        {
            _query = query;
            _targetDomain = targetDomain;
        }

        public void Start(System.Action onProgressChanged)
        {
            _onProgressChanged = onProgressChanged;
            if (EvaluateCurrentState())
            {
                IsCompleted = true;
                _onProgressChanged?.Invoke();
                return;
            }
            _query.OnDomainTransition += OnTransition;
        }

        public void Stop() => _query.OnDomainTransition -= OnTransition;

        private void OnTransition(TutorialStepDomain from, TutorialStepDomain to)
        {
            if (!IsCompleted && to == _targetDomain)
            {
                IsCompleted = true;
                _onProgressChanged?.Invoke();
            }
        }

        public bool EvaluateCurrentState() => _query.CurrentDomain == _targetDomain;
        public bool EvaluateEvent(object gameplayEvent) => false;
    }
}
