using System;
using Galactic1.Code.Systems.Construction.Configs;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>category — обычный enum, не nullable: "нет значения" теперь выражается
    /// отсутствием самого Query (highlightTarget == null), а не magic-значением enum
    /// (см. п.4 ТЗ — отказ от nullable enum).</summary>
    [Serializable]
    public sealed class TutorialConstructionTabQuery : TutorialTargetQuery
    {
        public ConstructionCategory category;
    }
}