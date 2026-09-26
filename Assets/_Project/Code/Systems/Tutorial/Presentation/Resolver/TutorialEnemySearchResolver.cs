using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Не резолвит selection сам — только читает уже зафиксированную
    /// TutorialEnemyGroupSelectionService (владелец selection — EnemyGroupKilledObjective).
    /// Если objective ещё не стартовал, TryResolve возвращает false — highlight просто
    /// не показывается, как и для любого нерезолвленного таргета.
    /// </summary>
    public sealed class TutorialEnemySearchResolver : ITutorialTargetResolver
    {
        private readonly TutorialEnemyGroupSelectionService _selection;
        private readonly Dictionary<Action, Action<TutorialTargetId>> _handlers = new();

        public TutorialEnemySearchResolver(TutorialEnemyGroupSelectionService selection) => _selection = selection;

        public bool CanResolve(TutorialTargetRequest request) => request is TutorialEnemySearchRequest;

        public bool TryResolve(TutorialTargetRequest request, out ITutorialTarget target)
        {
            var r = (TutorialEnemySearchRequest)request;

            if (_selection.TryGetNearestAlive(r.OriginTargetId, out var enemy))
            {
                target = new DynamicWorldTutorialTarget(enemy.transform);
                return true;
            }

            target = null;
            return false;
        }

        public void SubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            var r = (TutorialEnemySearchRequest)request;

            void Handler(TutorialTargetId changedOrigin)
            {
                if (changedOrigin == r.OriginTargetId)
                    callback();
            }

            _handlers[callback] = Handler;
            _selection.OnSelectionChanged += Handler;
        }

        public void UnsubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            if (!_handlers.TryGetValue(callback, out var handler)) return;
            _selection.OnSelectionChanged -= handler;
            _handlers.Remove(callback);
        }
    }
}