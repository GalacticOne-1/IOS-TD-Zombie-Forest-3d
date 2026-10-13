using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
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

        [Tooltip("Если включено и на момент старта шага у игрока УЖЕ НЕТ требуемого количества " +
                 "предмета (например игрок потратил аптечку раньше) — объектив сразу считается " +
                 "выполненным, не дожидаясь нового использования.")]
        public bool skipIfNotEnoughItems;

        [Header("Skip check (используется только при skipIfNotEnoughItems)")]
        public InventorySourceType checkSourceA = InventorySourceType.BaseStorage;

        public InventorySourceType checkSourceB = InventorySourceType.TransportCargo;

#if UNITY_EDITOR
        // Только эти типы поддержаны в ITutorialInventorySourceAmountQuery.GetAmount
        private static bool IsSupportedCheckSource(InventorySourceType type)
            => type == InventorySourceType.BaseStorage
               || type == InventorySourceType.TransportCargo
               || type == InventorySourceType.UnitEquipment;

        public override bool Validate(out string error)
        {
            if (requiredCount < 1)
            {
                error = "ItemUsedObjectiveDefinition: requiredCount must be >= 1.";
                return false;
            }

            if (skipIfNotEnoughItems)
            {
                if (itemId == null)
                {
                    error = "ItemUsedObjectiveDefinition: skipIfNotEnoughItems requires itemId " +
                            "(для 'любого предмета' проверка количества не выполняется).";
                    return false;
                }

                if (!IsSupportedCheckSource(checkSourceA))
                {
                    error = $"ItemUsedObjectiveDefinition: checkSourceA = '{checkSourceA}' не поддержан. " +
                            "Допустимо только BaseStorage или TransportCargo.";
                    return false;
                }

                if (!IsSupportedCheckSource(checkSourceB))
                {
                    error = $"ItemUsedObjectiveDefinition: checkSourceB = '{checkSourceB}' не поддержан. " +
                            "Допустимо только BaseStorage или TransportCargo.";
                    return false;
                }
            }

            error = null;
            return true;
        }
#endif
    }
}