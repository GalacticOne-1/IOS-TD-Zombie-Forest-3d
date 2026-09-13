using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Tutorial.Presentation;

namespace Galactic1.Game.UI.Inbox
{
    /// <summary>Реализация ITutorialItemSlotTargetProvider поверх InboxCardView —
    /// живёт в Inbox UI namespace по тому же принципу, что TutorialInventorySlotTargetProvider
    /// живёт в Inventory UI namespace: только эта реализация знает про InboxCardView,
    /// тутор — нет.</summary>
    public sealed class TaskInboxSlotTargetProvider : ITutorialItemSlotTargetProvider
    {
        private readonly TutorialInboxViewRegistry _registry;

        public TaskInboxSlotTargetProvider(TutorialInboxViewRegistry registry)
        {
            _registry = registry;
        }

        public bool TryGetSlotTarget(ItemId itemId, out ITutorialTarget target)
        {
            foreach (var card in _registry.Active)
            {
                if (card != null && card.ItemId == itemId)
                {
                    target = new DynamicRectTutorialTarget(card.TakeButtonRect);
                    return true;
                }
            }

            target = null;
            return false;
        }
    }
}