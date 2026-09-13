using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>OR-композиция дочерних guidance-условий. См. AllOfGuidanceCondition
    /// докстринг — та же механика, инвертированная.</summary>
    public sealed class AnyOfGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly IReadOnlyList<ITutorialGuidanceCondition> _children;

        public AnyOfGuidanceCondition(IReadOnlyList<ITutorialGuidanceCondition> children) => _children = children;

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
                if (c.IsSatisfied())
                    return true;
            return false;
        }
    }
}
