using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Core.Enums;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>Условие вида "предмет (не) экипирован в слот" — authoring-обёртка над той же
    /// query, что уже использует ItemEquippedObjective (ITutorialInventoryQuery), но здесь
    /// это не критерий завершения шага, а критерий выбора guidance-подсказки. Пример:
    /// в шаге "Equip Pistol" guidance-вариант с expectedEquipped=false решает, стоит ли ещё
    /// показывать EquipButton.</summary>
    [CreateAssetMenu(fileName = "Guidance_ItemEquipped",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Item Equipped")]
    public sealed class ItemEquippedGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "ItemEquipped";

        public EquipSlotType slot;

        [Tooltip("Пусто = засчитывается любой предмет в слоте.")]
        public ItemId itemId;

        [Tooltip("true = условие истинно, когда предмет ЭКИПИРОВАН; false = когда НЕ экипирован.")]
        public bool expectedEquipped = true;
    }
}
