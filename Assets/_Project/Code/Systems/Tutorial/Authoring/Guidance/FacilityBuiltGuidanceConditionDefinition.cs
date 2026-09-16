using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>Условие вида "здание этой конфигурации (не) построено" — authoring-обёртка
    /// над ITutorialConstructionQuery, по тому же принципу, что ItemEquippedGuidanceCondition
    /// над ITutorialInventoryQuery: не критерий завершения шага (для этого — 
    /// FacilityBuiltObjectiveDefinition), а критерий выбора guidance-подсказки. Пример:
    /// AllOf(ConstructionPanelOpen, FacilityBuilt(expectedBuilt=false)) — "подсвечивать
    /// карточку, пока здание ещё не построено".</summary>
    [CreateAssetMenu(fileName = "Guidance_FacilityBuilt",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Facility Built")]
    public sealed class FacilityBuiltGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "FacilityBuilt";

        [Tooltip("Пусто = засчитывается любое построенное здание.")]
        public ItemId itemId;

        [Tooltip("true = условие истинно, когда здание ПОСТРОЕНО; false = когда НЕ построено.")]
        public bool expectedBuilt = true;
    }
}
