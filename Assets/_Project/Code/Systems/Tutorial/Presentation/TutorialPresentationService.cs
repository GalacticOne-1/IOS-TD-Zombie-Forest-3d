using System;
using System.Collections.Generic;
using Galactic1.Code.Cameras;
using Galactic1.Code.Systems.Tutorial.Authoring;

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
    /// </summary>
    public sealed class TutorialPresentationService : ITutorialPresentationService, IGameService
    {
        private readonly TutorialTargetRegistry _targetRegistry;

        private ITutorialPresentationRenderer _renderer;
        private TutorialPresentationDefinition _activeDefinition;
        private readonly List<Action> _pendingTargetUnsubs = new();
        private int _generation;

        public TutorialPresentationService(TutorialTargetRegistry targetRegistry)
        {
            _targetRegistry = targetRegistry;
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

            if (!string.IsNullOrEmpty(presentation.instructionTextKey))
                _renderer.RenderInstruction(presentation.instructionTextKey);
            else
                _renderer.ClearInstruction();

            ResolveTarget(presentation.highlightTargetId, _renderer.RenderHighlight, _renderer.ClearHighlight);
            ResolveTarget(presentation.arrowTargetId, _renderer.RenderArrow, _renderer.ClearArrow);
            ResolveTarget(presentation.cameraFocusTargetId, FocusCameraOn, () => { });
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
