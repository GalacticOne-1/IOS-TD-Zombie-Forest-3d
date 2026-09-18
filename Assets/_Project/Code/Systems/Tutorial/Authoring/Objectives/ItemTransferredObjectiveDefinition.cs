using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>
    /// Event-семантика: "переместил N предмета между двумя ИСТОЧНИКАМИ ЗА ВРЕМЯ ЭТОГО
    /// ШАГА" — по тому же принципу, что ItemCollectedObjective (не ретроактивен, счётчик
    /// с нуля от активации шага). В отличие от ItemEquippedObjective (слот экипировки
    /// конкретного юнита) — здесь обе стороны это произвольные IInventorySource
    /// (camp storage, transport cargo, контейнер, труп и т.п.), различаются по
    /// InventorySourceType, не по ссылке на инстанс (инстансы транзиентны — см.
    /// ItemTransferredEvent докстринг).
    /// </summary>
    [CreateAssetMenu(fileName = "Objective_ItemTransferred",
        menuName = "Game Configs/Tutorial/Objectives/Item Transferred")]
    public sealed class ItemTransferredObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ItemTransferred";

        [Tooltip("Пусто = засчитывается перемещение ЛЮБОГО предмета.")]
        public ItemId itemId;

        [Tooltip("Null = засчитывается перемещение ИЗ любого источника. Задай значение, " +
                 "чтобы требовать конкретный тип-источник (например BaseStorage).")]
        public InventorySourceType? fromSourceType;

        [Tooltip("Null = засчитывается перемещение В любой источник. Задай значение, " +
                 "чтобы требовать конкретный тип-источник (например TransportCargo).")]
        public InventorySourceType? toSourceType;

        [Min(1)] public int requiredAmount = 1;
    }
}