using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>
    /// Event-семантика: "расходуемый предмет реально использован ЗА ВРЕМЯ ЭТОГО ШАГА"
    /// (аптечка и т.п.). Опирается на ItemConsumabledEvent из ConsumableBehaviour.Consume —
    /// событие поднимается только после фактического расхода предмета.
    /// </summary>
    [CreateAssetMenu(fileName = "Objective_ItemUsed",
        menuName = "Game Configs/Tutorial/Objectives/Item Used")]
    public sealed class ItemUsedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ItemUsed";

        [Tooltip("Пусто = засчитывается использование ЛЮБОГО расходуемого предмета.")]
        public ItemId itemId;

        [Min(1)] public int requiredCount = 1;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (requiredCount < 1)
            {
                error = "ItemUsedObjectiveDefinition: requiredCount must be >= 1.";
                return false;
            }
            error = null;
            return true;
        }
#endif
    }
}