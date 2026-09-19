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
        [Min(1)] public int requiredAmount = 1;

        [Header("From Source")]
        [Tooltip("Включить фильтр источника, из которого перемещается предмет.")]
        public bool filterFromSource;

        [Tooltip("Тип источника, из которого перемещается предмет.")]
        public InventorySourceType fromSourceType;

        [Header("To Source")]
        [Tooltip("Включить фильтр источника, в который перемещается предмет.")]
        public bool filterToSource;

        [Tooltip("Тип источника, в который перемещается предмет.")]
        public InventorySourceType toSourceType;
        
        
#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (itemId == null)
            {
                error = "ItemTransferredObjectiveDefinition: itemId is empty.";
                return false;
            }

            bool supported = toSourceType is InventorySourceType.BaseStorage
                or InventorySourceType.TransportCargo;
            if (!supported)
            {
                error = $"ItemTransferredObjectiveDefinition: toSourceType={toSourceType} не " +
                        "поддержан — нет единственного canonical instance для суммирования " +
                        "(per-owner или transient-only источник).";
                return false;
            }

            error = null;
            return true;
        }
#endif

    }
}