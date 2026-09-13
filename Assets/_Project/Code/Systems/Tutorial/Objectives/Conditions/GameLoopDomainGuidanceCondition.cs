using System;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>"Сейчас находимся в домене X" — та же retroactive-семантика, что
    /// GameLoopDomainReachedObjective; переиспользует тот же IGameLoopStateQuery и его
    /// OnDomainTransition как trigger (полностью интегрирован, без разрывов).</summary>
    public sealed class GameLoopDomainGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly IGameLoopStateQuery _query;
        private readonly TutorialStepDomain _domain;
        private Action _onMightHaveChanged;

        public GameLoopDomainGuidanceCondition(IGameLoopStateQuery query, TutorialStepDomain domain)
        {
            _query = query;
            _domain = domain;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _query.OnDomainTransition += OnTransition;
        }

        public void Stop() => _query.OnDomainTransition -= OnTransition;

        private void OnTransition(TutorialStepDomain from, TutorialStepDomain to) => _onMightHaveChanged?.Invoke();

        public bool IsSatisfied() => _query.CurrentDomain == _domain;
    }
}
