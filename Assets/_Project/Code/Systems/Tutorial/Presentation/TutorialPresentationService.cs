using System;
using System.Collections.Generic;
using Galactic1.Code.Cameras;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Presentation.Galactic1.Code.Systems.Tutorial.Presentation;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Больше не знает про Inventory/Inbox/UnitSearch/FacilityCard/ConstructionTab —
    /// весь highlight-резолв делегирован TutorialTargetResolverRegistry (см. её
    /// докстринг). Новый target-тип добавляется регистрацией нового резолвера в DI,
    /// этот класс не меняется ни строкой (ключевое требование ТЗ п.10/19).
    ///
    /// Arrow/Camera НЕ мигрированы на query-систему (осознанно, см. ТЗ п.12) — по-прежнему
    /// TutorialTargetId через тот же TutorialTargetRegistry, тот же lifecycle, что и раньше.
    /// </summary>
    public sealed class TutorialPresentationService : ITutorialPresentationService, IGameService
    {
        private readonly TutorialTargetRegistry _targetRegistry;
        private readonly TutorialTargetResolverRegistry _resolverRegistry;

        private ITutorialPresentationRenderer _renderer;
        private TutorialEffectivePresentation _activePresentation;
        private readonly List<Action> _pendingTargetUnsubs = new();
        private int _generation;

        public TutorialPresentationService(
            TutorialTargetRegistry targetRegistry,
            TutorialTargetResolverRegistry resolverRegistry)
        {
            _targetRegistry = targetRegistry;
            _resolverRegistry = resolverRegistry;
        }

        public void Show(TutorialEffectivePresentation presentation)
        {
            _generation++;
            ClearPendingSubscriptions();
            _activePresentation = presentation;
            Render(presentation);
        }

        public void Hide()
        {
            _generation++;
            ClearPendingSubscriptions();
            _activePresentation = null;
            _renderer?.ClearAll();
        }

        /// <summary>Вызывается TutorialHUDController при загрузке новой сцены. Если Show()
        /// был вызван до того, как HUD этой сцены существовал — дорисовываем сейчас.</summary>
        public void AttachRenderer(ITutorialPresentationRenderer renderer)
        {
            _renderer = renderer;
            if (_activePresentation != null)
                Render(_activePresentation);
        }

        public void DetachRenderer(ITutorialPresentationRenderer renderer)
        {
            if (_renderer == renderer)
                _renderer = null;
        }

        private void Render(TutorialEffectivePresentation presentation)
        {
            if (_renderer == null) return;

            RenderHighlight(presentation.HighlightRequest);
            ResolveTarget(presentation.ArrowTargetId, _renderer.RenderArrow, _renderer.ClearArrow);
            ResolveTarget(presentation.CameraFocusTargetId, FocusCameraOn, () => { });
        }

        private void RenderHighlight(TutorialTargetRequest request)
        {
            if (request == null)
            {
                _renderer.ClearHighlight();
                return;
            }

            if (_resolverRegistry.TryResolve(request, out var target))
                _renderer.RenderHighlight(target);
            else
                _renderer.ClearHighlight();

            var resolver = _resolverRegistry.FindResolver(request);
            if (resolver == null) return;

            int capturedGeneration = _generation;

            void Callback()
            {
                if (capturedGeneration != _generation) return; // устаревший callback — Show()/Hide() уже сменили состояние
                if (_renderer == null) return;
                if (_resolverRegistry.TryResolve(request, out var reresolved))
                    _renderer.RenderHighlight(reresolved);
                else
                    _renderer.ClearHighlight();
            }

            resolver.SubscribeInvalidation(request, Callback);
            _pendingTargetUnsubs.Add(() => resolver.UnsubscribeInvalidation(request, Callback));
        }

        /// <summary>Fixed-таргет lookup для arrow/camera — дословно перенесено из старой
        /// реализации, поведение не менялось.</summary>
        private void ResolveTarget(TutorialTargetId targetId, Action<ITutorialTarget> onFound, Action onEmpty)
        {
            if (targetId == null) { onEmpty(); return; }
            if (_targetRegistry.TryGetTarget(targetId, out var target)) { onFound(target); return; }

            int capturedGeneration = _generation;
            void Handler(ITutorialTarget registered)
            {
                if (registered.TargetId != targetId) return;
                _targetRegistry.OnTargetRegistered -= Handler;
                if (capturedGeneration != _generation) return;
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