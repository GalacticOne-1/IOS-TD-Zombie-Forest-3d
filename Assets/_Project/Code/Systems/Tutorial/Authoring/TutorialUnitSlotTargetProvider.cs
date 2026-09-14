using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Presentation;
using Galactic1.Code.UI.Inventory;
using UnityEngine;

namespace Galactic1.Code.UI.Units
{
    /// <summary>Реализация ITutorialUnitSlotTargetProvider поверх живого
    /// UnitScrollListPresenter.ItemViews — без отдельного реестра: presenter уже сам
    /// централизованно владеет актуальным пулом UnitBadgeView (в отличие от Inbox-карточек,
    /// которые регистрируются сами по себе через TutorialInboxCardRegistry), поэтому
    /// достаточно резолвить один инстанс через InventoryManagementWindow (тот же паттерн,
    /// что TaskInventorySlotTargetProvider использует для InventoryAccessService).</summary>
    public sealed class TutorialUnitSlotTargetProvider : ITutorialUnitSlotTargetProvider
    {
        private readonly GameLoopContext _context;

        public TutorialUnitSlotTargetProvider(GameLoopContext context)
        {
            _context = context;
        }

        public bool TryGetUnitTarget(TutorialUnitSearchCriteria criteria, out ITutorialTarget target)
        {
            var window = ServiceLocator.Current.Get<InventoryManagementWindow>();
            var presenter = window?.UnitList;

            if (presenter != null)
            {
                foreach (var view in presenter.ItemViews)
                {
                    // Скрытые (лишние из пула) элементы не должны матчиться — Rebuild()
                    // деактивирует их через gameObject.SetActive(false), не удаляет.
                    if (view != null && view.gameObject.activeSelf && Matches(view.UnitId, criteria))
                    {
                        target = new DynamicRectTutorialTarget(view.RectTr);
                        return true;
                    }
                }
            }

            target = null;
            return false;
        }

        private bool Matches(string unitId, TutorialUnitSearchCriteria criteria)
        {
            if (string.IsNullOrEmpty(unitId)) return false;

            bool isFree = !_context.IsStrategicSquadMember(unitId);
            if (isFree != criteria.requireFree)
                return false;

            // TODO: персональный уровень юнита не реализован в проекте (нет соответствующего
            // поля у UnitRuntime/UnitDisplayData на момент написания) — когда появится,
            // добавить: if (criteria.filterByLevel && GetUnitLevel(unitId) != criteria.requiredLevel) return false;

            return true;
        }
    }
}