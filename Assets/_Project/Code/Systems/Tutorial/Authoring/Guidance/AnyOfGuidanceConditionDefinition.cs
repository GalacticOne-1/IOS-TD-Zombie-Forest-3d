using System.Collections.Generic;
using UnityEngine;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>Композиция "хотя бы одно дочернее условие истинно" (OR). См.
    /// AllOfGuidanceConditionDefinition докстринг — та же логика композиции, инвертированная.</summary>
    [CreateAssetMenu(fileName = "Guidance_AnyOf",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Any Of (OR)")]
    public sealed class AnyOfGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "AnyOf";

        public List<TutorialGuidanceConditionDefinition> conditions = new();

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (conditions == null || conditions.Count == 0)
            {
                error = "AnyOfGuidanceConditionDefinition: conditions list is empty.";
                return false;
            }

            foreach (var c in conditions)
            {
                if (c == null)
                {
                    error = "AnyOfGuidanceConditionDefinition: contains a null condition.";
                    return false;
                }
                if (!c.Validate(out error))
                    return false;
            }

            error = null;
            return true;
        }
#endif
    }
}
