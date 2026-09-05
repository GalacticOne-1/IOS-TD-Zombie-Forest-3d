using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Core.Enums;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Событие ItemEquippedEvent — интеграционная точка, требует одной строки в
    /// EquipmentRuntimeService.Equip() (см. Integration/PENDING_GAMEPLAY_EVENTS.md).
    /// EvaluateCurrentState переопределён — экипировка ретроактивна (см. п.15 исходного ТЗ:
    /// "handle actions that happened before the objective started").
    /// </summary>
    public sealed class ItemEquippedObjective : TutorialEventObjectiveBase<ItemEquippedEvent>
    {
        private readonly ITutorialInventoryQuery _inventory;
        private readonly EquipSlotType _slot;
        private readonly ItemId _itemId;

        public ItemEquippedObjective(ITutorialInventoryQuery inventory, EquipSlotType slot, ItemId itemId)
        {
            _inventory = inventory;
            _slot = slot;
            _itemId = itemId;
        }

        public override bool EvaluateCurrentState()
            => _inventory.IsItemEquippedByAnyStrategicUnit(_slot, _itemId);

        protected override bool EvaluateEvent(ItemEquippedEvent e)
            => e.Slot == _slot && (_itemId == null || e.ItemId == _itemId);
    }
}
