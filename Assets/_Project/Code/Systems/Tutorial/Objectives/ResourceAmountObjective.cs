using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// State-семантика: "у игрока есть >= N предмета на складе лагеря". Требует
    /// CampStorageChangedEvent — интеграционная точка, см. Integration/PENDING_GAMEPLAY_EVENTS.md.
    /// </summary>
    public sealed class ResourceAmountObjective : TutorialStateRecheckObjectiveBase<CampStorageChangedEvent>
    {
        private readonly ITutorialInventoryQuery _inventory;
        private readonly ItemId _itemId;
        private readonly int _requiredAmount;

        public ResourceAmountObjective(ITutorialInventoryQuery inventory, ItemId itemId, int requiredAmount)
        {
            _inventory = inventory;
            _itemId = itemId;
            _requiredAmount = requiredAmount;
        }

        public override bool EvaluateCurrentState()
            => _inventory.GetCampStorageAmount(_itemId) >= _requiredAmount;
    }
}
