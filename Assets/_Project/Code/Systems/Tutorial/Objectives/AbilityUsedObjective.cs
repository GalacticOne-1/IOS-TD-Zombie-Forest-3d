using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Event-семантика: "способность itemId была реально использована ЗА ВРЕМЯ ЭТОГО
    /// ШАГА" — тот же паттерн, что ItemCollectedObjective. Не ретроактивен (сознательно —
    /// "использовал способность до начала шага" не должно засчитываться шагу, который
    /// требует сделать это здесь и сейчас, например Molotov-encounter).
    /// </summary>
    public sealed class AbilityUsedObjective : TutorialEventObjectiveBase<AbilityUsedEvent>
    {
        private readonly ItemId _itemId; // null = любая способность

        public AbilityUsedObjective(ItemId itemId) => _itemId = itemId;

        protected override bool EvaluateEvent(AbilityUsedEvent e)
            => _itemId == null || e.ItemId == _itemId;
    }
}
