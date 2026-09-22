using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>
    /// Generic objective "способность X реально использована" — раздел 4/7 ТЗ Chapter 2.
    /// Не привязан к конкретному tutorial-шагу/предмету по имени (не
    /// "CompleteMolotovTutorialObjective") — itemId параметризует конкретный предмет
    /// (Molotov и т.п.), объектив переиспользуем для любой способности.
    /// </summary>
    [CreateAssetMenu(fileName = "Objective_AbilityUsed",
        menuName = "Game Configs/Tutorial/Objectives/Ability Used")]
    public sealed class AbilityUsedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "AbilityUsed";

        [Tooltip("Пусто = засчитывается использование ЛЮБОЙ способности.")]
        public ItemId itemId;
    }
}
