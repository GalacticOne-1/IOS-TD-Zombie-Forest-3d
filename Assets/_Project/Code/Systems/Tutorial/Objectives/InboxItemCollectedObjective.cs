using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Mobile.EventBus;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Event-семантика: "подобрал N предмета ЗА ВРЕМЯ ЭТОГО ШАГА". НЕ путать с
    /// ResourceAmountObjective (state-семантика "владеет N предмета сейчас") — п.11
    /// исходного corrective ТЗ. Не ретроактивен: предмет, подобранный до активации
    /// шага, не засчитывается.
    /// </summary>
    public sealed class InboxItemCollectedObjective : TutorialEventObjectiveBase<InboxItemColectedEvent>
    {
        private readonly ItemId _itemId;

        public InboxItemCollectedObjective(ItemId itemId)
        {
            _itemId = itemId;
        }

        protected override bool EvaluateEvent(InboxItemColectedEvent e)
            => e.ItemId == _itemId;

    }
}