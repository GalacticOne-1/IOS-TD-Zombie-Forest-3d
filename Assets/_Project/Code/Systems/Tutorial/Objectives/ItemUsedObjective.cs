using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Mobile.EventBus;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// "Предмет itemId использован (потрачен) ЗА ВРЕМЯ ЭТОГО ШАГА". Не ретроактивен —
    /// как EnemyKilledObjective, счётчик стартует с активацией шага.
    /// </summary>
    public sealed class ItemUsedObjective : TutorialEventObjectiveBase<ItemConsumabledEvent>
    {
        private readonly ItemId _itemId; // null = любой предмет
        private readonly int _requiredCount;
        private int _current;

        public ItemUsedObjective(ItemId itemId, int requiredCount)
        {
            _itemId = itemId;
            _requiredCount = requiredCount < 1 ? 1 : requiredCount;
        }

        protected override bool EvaluateEvent(ItemConsumabledEvent e)
        {
            if (_itemId != null && e.ItemId != _itemId)
                return false;

            _current += e.Amount;
            return _current >= _requiredCount;
        }

        // Прогресс показываем только для "использовать N раз", иначе задача остаётся текстовой.
        public override bool TryGetProgress(out int current, out int required)
        {
            current = _current;
            required = _requiredCount;
            return _requiredCount > 1;
        }
    }
}