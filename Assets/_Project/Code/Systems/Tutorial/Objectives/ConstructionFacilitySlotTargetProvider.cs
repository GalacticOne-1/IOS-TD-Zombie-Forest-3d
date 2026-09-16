using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.UI.Construction;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Реализация ITutorialFacilitySlotTargetProvider поверх живого
    /// FacilityListView.Cards — без отдельного реестра: ConstructionPanelController уже
    /// регистрируется в ServiceLocator сам (см. его Initialize), достаточно резолвить один
    /// инстанс и обойти текущий набор карточек — тот же паттерн, что
    /// TutorialUnitSlotTargetProvider использует для InventoryManagementWindow.UnitList.</summary>
    public sealed class ConstructionFacilitySlotTargetProvider : ITutorialFacilitySlotTargetProvider
    {
        public bool TryGetFacilityTarget(ItemId facilityItemId, out ITutorialTarget target)
        {
            var controller = ServiceLocator.Current.Get<ConstructionPanelController>();
            var listView = controller?.View?.ListView;

            if (listView != null)
            {
                var cards = listView.Cards;
                for (int i = 0; i < cards.Count; i++)
                {
                    var card = cards[i];
                    // Скрытые (отфильтрованные текущей вкладкой) карточки не должны
                    // матчиться — FacilityListView.Filter() деактивирует их через
                    // SetActive(false), не удаляет (тот же принцип, что у
                    // UnitScrollListPresenter.ItemViews в TutorialUnitSlotTargetProvider).
                    if (card != null && card.gameObject.activeSelf && Matches(card.Facility, facilityItemId))
                    {
                        target = new DynamicRectTutorialTarget((RectTransform)card.transform);
                        return true;
                    }
                }
            }

            target = null;
            return false;
        }

        private bool Matches(FacilityModule facility, ItemId facilityItemId)
            => facility?.Item != null && (facilityItemId == null || facility.Item.Id == facilityItemId);
    }
}
