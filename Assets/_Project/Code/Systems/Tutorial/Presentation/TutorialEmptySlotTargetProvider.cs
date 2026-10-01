
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Tutorial.Objectives;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Реализация поверх TutorialInventoryViewRegistry — тот же принцип, что
    /// TutorialUnitSlotTargetProvider/ITutorialItemSlotTargetProvider: обходим живые View,
    /// а не держим отдельный кэш слотов. Первый пустой слот ПЕРВОГО подходящего по типу
    /// источника View побеждает (first-match, как везде в Guidance).</summary>
    public sealed class TutorialEmptySlotTargetProvider : ITutorialEmptySlotTargetProvider
    {
        private readonly TutorialInventoryViewRegistry _registry;

        public TutorialEmptySlotTargetProvider(TutorialInventoryViewRegistry registry)
            => _registry = registry;

        public bool TryGetEmptySlotTarget(InventorySourceType sourceType, out ITutorialTarget target)
        {
            foreach (var view in _registry.Active)
            {
                if (view == null || view._source == null || view._source.Type != sourceType)
                    continue;

                var slotCount = view._access.GetSlots(view._source).Count;
                var slotsUI = view.SlotsUI;

                for (int i = 0; i < slotCount && i < slotsUI.Count; i++)
                {
                    if (!view.GetSlot(i).IsEmpty)
                        continue;

                    var slotView = slotsUI[i];
                    if (slotView == null || !slotView.gameObject.activeInHierarchy)
                        continue;

                    target = new DynamicRectTutorialTarget((RectTransform)slotView.transform);
                    return true;
                }
            }

            target = null;
            return false;
        }
    }
}