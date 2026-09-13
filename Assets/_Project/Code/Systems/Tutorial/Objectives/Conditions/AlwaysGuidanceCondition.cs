using System;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>"Всегда истинно" — рантайм для condition == null (см. TutorialGuidanceDefinition
    /// докстринг) и для legacy single-target шагов, у которых нет authoring-условия вовсе.
    /// Синглтон: без состояния, без подписок, безопасно шарить между всеми guidance-entries.</summary>
    public sealed class AlwaysGuidanceCondition : ITutorialGuidanceCondition
    {
        public static readonly AlwaysGuidanceCondition Instance = new();
        private AlwaysGuidanceCondition() { }

        public void Start(Action onMightHaveChanged) { }
        public void Stop() { }
        public bool IsSatisfied() => true;
    }
}
