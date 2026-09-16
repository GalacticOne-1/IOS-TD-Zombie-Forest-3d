using Galactic1.Code.Systems.Construction.Configs;
using Galactic1.Code.UI.Construction;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Реализация ITutorialConstructionTabTargetProvider поверх живого
    /// ConstructionPanelView.Tabs — тот же паттерн резолва через ServiceLocator, что
    /// ConstructionFacilitySlotTargetProvider использует для FacilityListView.Cards.</summary>
    public sealed class ConstructionTabSlotTargetProvider : ITutorialConstructionTabTargetProvider
    {
        public bool TryGetTabTarget(ConstructionCategory category, out ITutorialTarget target)
        {
            var controller = ServiceLocator.Current.Get<ConstructionPanelController>();
            var tabs = controller?.View?.Tabs;

            if (tabs != null)
            {
                for (int i = 0; i < tabs.Count; i++)
                {
                    var tab = tabs[i];
                    if (tab != null && tab.gameObject.activeSelf && tab.Category == category)
                    {
                        target = new DynamicRectTutorialTarget((RectTransform)tab.transform);
                        return true;
                    }
                }
            }

            target = null;
            return false;
        }
    }
}
