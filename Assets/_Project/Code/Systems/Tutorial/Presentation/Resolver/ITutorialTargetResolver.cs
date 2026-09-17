using System;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Runtime-абстракция резолва "запрос → текущий существующий таргет на сцене".
    /// Принимает ТОЛЬКО TutorialTargetRequest — никогда TutorialTargetQuery. Не знает о
    /// TutorialStep/TutorialService/transition/objective/reward/progression/renderer/UI.
    ///
    /// SubscribeInvalidation/UnsubscribeInvalidation — минимальный канал для "живых"
    /// резолверов (Inventory/Inbox/UnitSearch/FacilityCard/ConstructionTab): раньше
    /// TutorialPresentationService сам решал, на какие события подписаться, под каждый
    /// HighlightMode. Теперь это решает сам резолвер — сервис просто дергает
    /// Subscribe при показе и Unsubscribe при Hide/смене таргета, не зная, что там за
    /// событие (InventoryContentsChangedEvent/FacilityPanelOpenedEvent/...). Резолверы
    /// без динамики (FixedTarget) держат это как no-op.
    /// </summary>
    public interface ITutorialTargetResolver
    {
        bool CanResolve(TutorialTargetRequest request);
        bool TryResolve(TutorialTargetRequest request, out ITutorialTarget target);

        void SubscribeInvalidation(TutorialTargetRequest request, Action callback);
        void UnsubscribeInvalidation(TutorialTargetRequest request, Action callback);
    }
}