using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.Runtime.Production;   // ProductionOrderCompletedEvent — поправьте using, если namespace другой
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Event-семантика: "произведено N штук предмета ЗА ВРЕМЯ ЭТОГО ШАГА". Не ретроактивен:
    /// заказы, завершённые до активации шага (в т.ч. офлайн-догон при загрузке), не засчитываются.
    ///
    /// Считаются ГОТОВЫЕ заказы (ProductionOrderCompletedEvent), не забранные игроком:
    /// каждое событие = 1 заказ, вклад = Orders * Amount штук предмета.
    /// Прогресс персистентный (IPersistentTutorialObjective).
    /// </summary>
    public sealed class ItemProducedObjective
        : TutorialEventObjectiveBase<ProductionOrderCompletedEvent>, IPersistentTutorialObjective
    {
        private readonly ItemId _itemId;        // null = любой предмет
        private readonly int _requiredAmount;
        private int _produced;

        public ItemProducedObjective(ItemId itemId, int requiredAmount)
        {
            _itemId = itemId;
            _requiredAmount = Mathf.Max(1, requiredAmount);
        }

        // Ретроактивность только для восстановленного из сейва прогресса.
        public override bool EvaluateCurrentState() => _produced >= _requiredAmount;

        protected override bool EvaluateEvent(ProductionOrderCompletedEvent e)
        {
            if (_itemId != null && (e.RecipeId == null || e.RecipeId.Guid != _itemId.Guid))
                return false;

            _produced += Mathf.Max(1, e.Orders) * Mathf.Max(1, e.Amount);
            return _produced >= _requiredAmount;
        }

        public override bool TryGetProgress(out int current, out int required)
        {
            current = Mathf.Min(_produced, _requiredAmount);
            required = _requiredAmount;
            return true;
        }

        public int ProgressValue => _produced;
        public void RestoreProgress(int value) => _produced = Mathf.Max(0, value);
    }
}
