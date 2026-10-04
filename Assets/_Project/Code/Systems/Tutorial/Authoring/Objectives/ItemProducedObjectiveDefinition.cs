using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>Event-семантика: "произведи N штук предмета за время этого шага" (не ретроактивно).</summary>
    [CreateAssetMenu(fileName = "Objective_ItemProduced",
        menuName = "Game Configs/Tutorial/Objectives/Item Produced")]
    public sealed class ItemProducedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ItemProduced";

        [Tooltip("Пусто = засчитывается производство ЛЮБОГО предмета.")]
        public ItemId itemId;

        [Tooltip("Сколько ШТУК предмета нужно произвести (заказ может давать несколько штук).")]
        [Min(1)] public int requiredAmount = 1;
    }
}
