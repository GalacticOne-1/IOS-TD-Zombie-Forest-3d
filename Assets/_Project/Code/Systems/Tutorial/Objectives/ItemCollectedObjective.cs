using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Event-семантика: "подобрал N предмета ЗА ВРЕМЯ ЭТОГО ШАГА". НЕ путать с
    /// ResourceAmountObjective (state-семантика "владеет N предмета сейчас") — п.11
    /// исходного corrective ТЗ. Не ретроактивен: предмет, подобранный до активации
    /// шага, не засчитывается.
    /// </summary>
    public sealed class ItemCollectedObjective : TutorialEventObjectiveBase<ItemPickedEvent>
    {
        private readonly ItemId _itemId;
        private readonly int _requiredAmount;
        private int _collected;

        public ItemCollectedObjective(ItemId itemId, int requiredAmount)
        {
            _itemId = itemId;
            _requiredAmount = requiredAmount;
        }

        protected override bool EvaluateEvent(ItemPickedEvent e)
        {
            if (e.Item == null || e.Item.Id != _itemId) return false;
            _collected += e.Amount;
            return _collected >= _requiredAmount;
        }
    }
}
