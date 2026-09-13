using System.Collections.Generic;
using UnityEngine;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>
    /// Композиция "все дочерние условия истинны" (AND). Дочерние условия — любые
    /// TutorialGuidanceConditionDefinition, включая другие AllOf/AnyOf — вложенность
    /// допустима (в отличие от TutorialObjectiveGroupDefinition, где вложенность
    /// намеренно не поддержана): нужна для выражений вида "A AND (B OR C)" из примера
    /// "Equip Pistol" в задаче ("Inventory Open AND Pistol Not Selected").
    /// </summary>
    [CreateAssetMenu(fileName = "Guidance_AllOf",
        menuName = "Game Configs/Tutorial/Guidance Conditions/All Of (AND)")]
    public sealed class AllOfGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "AllOf";

        public List<TutorialGuidanceConditionDefinition> conditions = new();

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (conditions == null || conditions.Count == 0)
            {
                error = "AllOfGuidanceConditionDefinition: conditions list is empty.";
                return false;
            }

            foreach (var c in conditions)
            {
                if (c == null)
                {
                    error = "AllOfGuidanceConditionDefinition: contains a null condition.";
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
