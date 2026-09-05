using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Core.Enums;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_ItemEquipped", 
        menuName = "Game Configs/Tutorial/Objectives/Item Equipped")]
    public sealed class ItemEquippedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ItemEquipped";
        public EquipSlotType slot;
        [Tooltip("Пусто = засчитывается любой предмет в слоте.")]
        public ItemId itemId;
    }
}
