using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>AND-композиция дочерних guidance-условий. Start/Stop прозрачно
    /// проксируются во всех детей — каждый ребёнок сам подписывается на нужные ему события
    /// (см. ITutorialGuidanceCondition докстринг), эта обёртка не добавляет собственных
    /// подписок.</summary>
    public sealed class AllOfGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly IReadOnlyList<ITutorialGuidanceCondition> _children;

        public AllOfGuidanceCondition(IReadOnlyList<ITutorialGuidanceCondition> children) => _children = children;

        public void Start(Action onMightHaveChanged)
        {
            foreach (var c in _children)
                c.Start(onMightHaveChanged);
        }

        public void Stop()
        {
            foreach (var c in _children)
                c.Stop();
        }

        public bool IsSatisfied()
        {
            foreach (var c in _children)
                if (!c.IsSatisfied())
                    return false;
            return true;
        }
    }
}
