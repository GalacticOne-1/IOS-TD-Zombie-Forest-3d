using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.Core.Enums;
using Galactic1.Mobile.EventBus;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>"Предмет (не) экипирован в слот" — переиспользует ту же query, что
    /// ItemEquippedObjective (ITutorialInventoryQuery.IsItemEquippedByAnyStrategicUnit) и
    /// тот же trigger-event (ItemEquippedEvent), уже полностью интегрированный в проект —
    /// в отличие от UIScreenOpenGuidanceCondition, здесь нет "requires integration" разрыва.</summary>
    public sealed class ItemEquippedGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialInventoryQuery _inventory;
        private readonly EquipSlotType _slot;
        private readonly ItemId _itemId;
        private readonly bool _expectedEquipped;
        private EventBinding<ItemEquippedEvent> _binding;
        private Action _onMightHaveChanged;

        public ItemEquippedGuidanceCondition(
            ITutorialInventoryQuery inventory, EquipSlotType slot, ItemId itemId, bool expectedEquipped)
        {
            _inventory = inventory;
            _slot = slot;
            _itemId = itemId;
            _expectedEquipped = expectedEquipped;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _binding = new EventBinding<ItemEquippedEvent>(_ => _onMightHaveChanged?.Invoke());
            EventBus<ItemEquippedEvent>.Register(_binding);
        }

        public void Stop()
        {
            if (_binding != null)
                EventBus<ItemEquippedEvent>.Deregister(_binding);
            _binding = null;
            _onMightHaveChanged = null;
        }

        public bool IsSatisfied()
            => _inventory.IsItemEquippedByAnyStrategicUnit(_slot, _itemId) == _expectedEquipped;
    }
}
