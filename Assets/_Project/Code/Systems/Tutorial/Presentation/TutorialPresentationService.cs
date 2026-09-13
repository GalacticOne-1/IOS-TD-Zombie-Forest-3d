using System;
using System.Collections.Generic;
using Galactic1.Code.Cameras;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Mobile.EventBus;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Единственная точка, которую вызывает TutorialService. Не знает про Unity-сцену:
    /// хранит "что должно быть показано" и делегирует рендер текущему
    /// ITutorialPresentationRenderer, если он есть. Сцена появляется/пропадает
    /// асинхронно относительно смены tutorial-шага — AttachRenderer()/таргеты могут
    /// "догонять" уже установленный Show().
    ///
    /// _generation — токен поколения (см. Show/Hide/ResolveTarget): колбэк ожидания
    /// таргета из предыдущей презентации не может повлиять на текущую, даже если
    /// сработает уже после Show() новой презентации.
    ///
    /// Highlight имеет ДВЕ стратегии резолва (presentation.highlightTargetId ИЛИ
    /// presentation.highlightItemId, взаимоисключимы — см. TutorialPresentationDefinition
    /// докстринг): фиксированный TutorialTargetId идёт через TutorialTargetRegistry
    /// (ResolveTarget, с ожиданием "targetId появится позже" через OnTargetRegistered);
    /// highlightItemId идёт через ITutorialItemSlotTargetProvider (ResolveItemTarget) —
    /// это ВСЕГДА синхронный live-резолв без ожидания, т.к. у "слота с предметом X" нет
    /// аналога OnTargetRegistered — если сейчас не найдено, просто ничего не подсвечиваем
    /// до следующего Show() (guidance перерисовывает presentation на каждое relevant-
    /// событие, см. TutorialGuidanceRuntimeState).
    /// </summary>
    public sealed class TutorialPresentationService : ITutorialPresentationService, IGameService
    {
        private readonly TutorialTargetRegistry _targetRegistry;
        private readonly ITutorialItemSlotTargetProvider _inventorySlotProvider;
        private readonly ITutorialItemSlotTargetProvider _inboxSlotProvider;

        private ITutorialPresentationRenderer _renderer;
        private TutorialPresentationDefinition _activeDefinition;
        private readonly List<Action> _pendingTargetUnsubs = new();
        private int _generation;

        public TutorialPresentationService(
            TutorialTargetRegistry targetRegistry,
            ITutorialItemSlotTargetProvider inventorySlotProvider, 
            ITutorialItemSlotTargetProvider inboxSlotProvider)
        {
            _targetRegistry = targetRegistry;
            _inventorySlotProvider = inventorySlotProvider;
            _inboxSlotProvider = inboxSlotProvider;
        }

        public void Show(TutorialPresentationDefinition presentation)
        {
            _generation++;
            ClearPendingSubscriptions();
            _activeDefinition = presentation;
            Render(presentation);
        }

        public void Hide()
        {
            _generation++;
            ClearPendingSubscriptions();
            _activeDefinition = null;
            _renderer?.ClearAll();
        }

        /// <summary>Вызывается TutorialHUDController при загрузке новой сцены. Если Show()
        /// был вызван до того, как HUD этой сцены существовал — дорисовываем сейчас.</summary>
        public void AttachRenderer(ITutorialPresentationRenderer renderer)
        {
            _renderer = renderer;
            if (_activeDefinition != null)
                Render(_activeDefinition);
        }

        public void DetachRenderer(ITutorialPresentationRenderer renderer)
        {
            if (_renderer == renderer)
                _renderer = null;
        }

        private void Render(TutorialPresentationDefinition presentation)
        {
            if (_renderer == null) return;

            if (presentation.highlightInboxItemId != null)
            {
                ResolveItemTarget(_inboxSlotProvider, presentation.highlightInboxItemId,
                    _renderer.RenderHighlight, _renderer.ClearHighlight);
                SubscribeInboxHighlightRetrigger(presentation.highlightInboxItemId);
            }
            else if (presentation.highlightItemId != null)
            {
                ResolveItemTarget(_inventorySlotProvider, presentation.highlightItemId,
                    _renderer.RenderHighlight, _renderer.ClearHighlight);
                SubscribeInventoryHighlightRetrigger(presentation.highlightItemId);
            }
            else
            {
                ResolveTarget(presentation.highlightTargetId, _renderer.RenderHighlight, _renderer.ClearHighlight);
            }

            ResolveTarget(presentation.arrowTargetId, _renderer.RenderArrow, _renderer.ClearArrow);
            ResolveTarget(presentation.cameraFocusTargetId, FocusCameraOn, () => { });
        }

        /// <summary>
        /// highlightItemId резолвится живым поиском по слотам (ResolveItemTarget) —
        /// у него, в отличие от TutorialTargetRegistry, нет "таргет появился" события,
        /// зато есть обратная проблема: предмет может СМЕНИТЬ слот без единого события,
        /// на которое уже подписано какое-либо guidance-условие (обычный drag внутри уже
        /// открытого инвентаря не эквипит и не открывает экран). InventoryContentsChangedEvent —
        /// узкоспециальный EventBus-канал именно под этот случай (см. его докстринг).
        ///
        /// Подписка живёт в _pendingTargetUnsubs — тот же generation-защищённый lifecycle,
        /// что у ResolveTarget: следующий Show()/Hide() инкрементит _generation и вызовет
        /// ClearPendingSubscriptions(), эта подписка снимется вместе со всеми остальными.
        /// Намеренно НЕ идёт через ITutorialGuidanceCondition/TutorialGuidanceRuntimeState —
        /// это вопрос "куда физически резолвится уже выбранный highlight", а не "какое
        /// guidance сейчас активно" (см. TutorialPresentationDefinition.highlightItemId
        /// докстринг про разделение Guidance/Presentation).
        /// </summary>
        private void SubscribeInventoryHighlightRetrigger(ItemId itemId)
        {
            int capturedGeneration = _generation;
            var binding = new EventBinding<InventoryContentsChangedEvent>(_ =>
            {
                if (capturedGeneration != _generation) return;
                if (_renderer == null) return;
                ResolveItemTarget(_inventorySlotProvider, itemId, _renderer.RenderHighlight, _renderer.ClearHighlight);
            });
            EventBus<InventoryContentsChangedEvent>.Register(binding);
            _pendingTargetUnsubs.Add(() => EventBus<InventoryContentsChangedEvent>.Deregister(binding));
        }
        
        private void SubscribeInboxHighlightRetrigger(ItemId itemId)
        {
            int capturedGeneration = _generation;
            var binding = new EventBinding<FacilityPanelOpenedEvent>(_ =>
            {
                if (capturedGeneration != _generation) return;
                if (_renderer == null) return;
                ResolveItemTarget(_inboxSlotProvider, itemId, _renderer.RenderHighlight, _renderer.ClearHighlight);
            });
            EventBus<FacilityPanelOpenedEvent>.Register(binding);
            _pendingTargetUnsubs.Add(() => EventBus<FacilityPanelOpenedEvent>.Deregister(binding));
        }

        private void ResolveTarget(TutorialTargetId targetId, Action<ITutorialTarget> onFound, Action onEmpty)
        {
            if (targetId == null) { onEmpty(); return; }
            if (_targetRegistry.TryGetTarget(targetId, out var target)) { onFound(target); return; }

            int capturedGeneration = _generation;
            void Handler(ITutorialTarget registered)
            {
                if (registered.TargetId != targetId) return;
                _targetRegistry.OnTargetRegistered -= Handler;
                if (capturedGeneration != _generation) return; // устаревший колбэк — игнор
                if (_renderer != null) onFound(registered);
            }
            _targetRegistry.OnTargetRegistered += Handler;
            _pendingTargetUnsubs.Add(() => _targetRegistry.OnTargetRegistered -= Handler);
        }

        /// <summary>Синхронный live-резолв "слот, где сейчас предмет X" — см. class docstring
        /// про отсутствие ожидания (в отличие от ResolveTarget). Провайдер опционален
        /// (может быть не подключён, например в изолированных тестах презентации) —
        /// в этом случае ведём себя как "предмет не найден", а не падаем.</summary>
        private void ResolveItemTarget(
            ITutorialItemSlotTargetProvider provider,
            ItemId itemId, 
            Action<ITutorialTarget> onFound, Action onEmpty)
        {
            if (provider != null && provider.TryGetSlotTarget(itemId, out var target))
                onFound(target);
            else
                onEmpty();
        }

        private void FocusCameraOn(ITutorialTarget target)
        {
            if (target.WorldAnchor == null) return;
            ServiceLocator.Current.Get<IMainCamera>().FocusOnPosition(target.WorldAnchor.position);
        }

        private void ClearPendingSubscriptions()
        {
            foreach (var unsub in _pendingTargetUnsubs) unsub();
            _pendingTargetUnsubs.Clear();
        }
    }
}
