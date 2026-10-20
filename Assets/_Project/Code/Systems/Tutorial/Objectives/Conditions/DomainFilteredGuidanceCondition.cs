using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Декоратор: условие истинно, только если текущий домен входит в список разрешённых
    /// И внутреннее условие истинно. Подписывается на OnDomainTransition как на триггер
    /// переоценки (источник истины — IsSatisfied()). Внутреннее условие стартует всегда,
    /// даже если домен сейчас не подходит, чтобы оно могло сигнализировать об изменениях,
    /// когда игрок вернётся в нужный домен.
    /// </summary>
    public sealed class DomainFilteredGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialGuidanceCondition _inner;
        private readonly IGameLoopStateQuery _query;
        private readonly HashSet<TutorialStepDomain> _allowed;
        private Action _onMightHaveChanged;

        public DomainFilteredGuidanceCondition(
            ITutorialGuidanceCondition inner,
            IGameLoopStateQuery query,
            IEnumerable<TutorialStepDomain> allowedDomains)
        {
            _inner = inner;
            _query = query;
            _allowed = new HashSet<TutorialStepDomain>(allowedDomains);
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _query.OnDomainTransition -= OnTransition; // защита от двойной подписки
            _query.OnDomainTransition += OnTransition;
            _inner.Start(onMightHaveChanged);
        }

        public void Stop()
        {
            _query.OnDomainTransition -= OnTransition;
            _inner.Stop();
            _onMightHaveChanged = null;
        }

        public bool IsSatisfied()
            => _allowed.Contains(_query.CurrentDomain) && _inner.IsSatisfied();

        private void OnTransition(TutorialStepDomain from, TutorialStepDomain to)
            => _onMightHaveChanged?.Invoke();
    }
}