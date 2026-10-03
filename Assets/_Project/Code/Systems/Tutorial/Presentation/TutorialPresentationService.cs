using System;
using System.Collections.Generic;
using Galactic1.Code.Cameras;
using Galactic1.Code.Systems.Tutorial.Authoring;

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
        private readonly TutorialCameraBoundsRegistry _cameraBoundsRegistry;

        private ITutorialPresentationRenderer _renderer;
        private TutorialEffectivePresentation _activePresentation;
        private readonly List<Action> _pendingTargetUnsubs = new();
        private int _generation;

        public TutorialPresentationService(
            TutorialTargetRegistry targetRegistry,
            TutorialTargetResolverRegistry resolverRegistry,
            TutorialCameraBoundsRegistry cameraBoundsRegistry)
        {
            _targetRegistry = targetRegistry;
            _resolverRegistry = resolverRegistry;
            _cameraBoundsRegistry = cameraBoundsRegistry;
        }

        public void Show(TutorialEffectivePresentation presentation)
        {
            _generation++;
            ClearPendingSubscriptions();
            _activePresentation = presentation;
            Render(presentation);
        }

        /// <summary>Полный cleanup: HideVisuals() + снятие camera constraint. Использовать при
        /// действительной остановке presentation lifecycle (StopTutorial), не в обычном
        /// completion-transition — там нужен HideVisuals() отдельно (см. её докстринг).</summary>
        public void Hide()
        {
            HideVisuals();
            ClearCameraConstraint();
        }
        
        /// <summary>Скрывает tutorial visuals, НЕ трогая camera constraint. В окне между
        /// завершением Step A и фактической активацией Step B (completion delay в
        /// ScenarioTaskService) камера обязана оставаться в bounds шага A — иначе на время
        /// delay она станет полностью свободной, игрок сможет её сдвинуть, а при активации
        /// Step B произойдёт резкая коррекция позиции. Старый constraint снимается/заменяется
        /// атомарно внутри Show() следующего шага (см. ApplyCameraConstraint), а не здесь.</summary>
        public void HideVisuals()
        {
            _generation++;
            ClearPendingSubscriptions();
            _activePresentation = null;
            _renderer?.ClearAll();
        }

        /// <summary>Снимает camera constraint независимо от визуалов — для случаев, когда
        /// Show() следующего шага не последует вовсе (терминальное завершение кампании без
        /// продолжения в NextCampaignId), поэтому старые bounds иначе остались бы навсегда.</summary>
        public void ClearCameraConstraint()
        {
            ServiceLocator.Current.Get<CameraController>()?.ClearTutorialBounds();
        }

        /// <summary>Вызывается TutorialHUDController при загрузке новой сцены. Если Show()
        /// был вызван до того, как HUD этой сцены существовал — дорисовываем сейчас.</summary>
        public void AttachRenderer(ITutorialPresentationRenderer renderer)
        {
            ClearPendingSubscriptions();
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
            if (_renderer == null)
                return;

            RenderHighlights(presentation.HighlightRequests);
            SubscribeHighlightInvalidations(presentation.HighlightRequests);

            ResolveTarget(
                presentation.ArrowTargetId,
                _renderer.RenderArrow,
                _renderer.ClearArrow);

            ResolveTarget(
                presentation.CameraFocusTargetId,
                FocusCameraOn,
                () => { });
            
            ApplyCameraConstraint(presentation.CameraConstraint);
        }

        private void RenderHighlights(
            IReadOnlyList<TutorialTargetRequest> requests)
        {
            if (requests == null || requests.Count == 0)
            {
                _renderer.ClearHighlight();
                return;
            }

            var resolvedTargets = new List<ITutorialTarget>(requests.Count);

            foreach (var request in requests)
            {
                if (request == null)
                    continue;

                if (_resolverRegistry.TryResolve(request, out var target))
                    resolvedTargets.Add(target);
            }

            if (resolvedTargets.Count == 0)
            {
                _renderer.ClearHighlight();
                return;
            }

            _renderer.RenderHighlights(resolvedTargets);
        }
        
        private void SubscribeHighlightInvalidations(
            IReadOnlyList<TutorialTargetRequest> requests)
        {
            if (requests == null)
                return;

            foreach (var request in requests)
            {
                if (request == null)
                    continue;

                var resolver = _resolverRegistry.FindResolver(request);
                if (resolver == null)
                    continue;

                int capturedGeneration = _generation;

                void Callback()
                {
                    if (capturedGeneration != _generation)
                        return;

                    if (_renderer == null)
                        return;

                    RenderHighlights(_activePresentation.HighlightRequests);
                }

                resolver.SubscribeInvalidation(request, Callback);

                _pendingTargetUnsubs.Add(
                    () => resolver.UnsubscribeInvalidation(request, Callback));
            }
        }


        private void ApplyCameraConstraint(TutorialCameraConstraint constraint)
        {
            ServiceLocator.Current.TryGet<CameraController>(out var camera);
            if (camera == null) return;

            if (constraint == null || constraint.Mode == TutorialCameraConstraintMode.None)
            {
                camera.ClearTutorialBounds();
                return;
            }

            if (_cameraBoundsRegistry.TryGetBounds(constraint.BoundsTargetId, out var boundsTarget))
            {
                camera.SetTutorialBounds(boundsTarget.WorldBounds);
                return;
            }

            // Таргет ещё не на сцене — временно без ограничения, подпишемся на появление.
            camera.ClearTutorialBounds();

            int capturedGeneration = _generation;
            var capturedTargetId = constraint.BoundsTargetId;

            void Handler(ITutorialCameraBoundsTarget registered)
            {
                if (registered.TargetId != capturedTargetId) return;
                _cameraBoundsRegistry.OnBoundsRegistered -= Handler;
                if (capturedGeneration != _generation) return; // устаревший presentation
                ServiceLocator.Current.Get<CameraController>()?.SetTutorialBounds(registered.WorldBounds);
            }

            _cameraBoundsRegistry.OnBoundsRegistered += Handler;
            _pendingTargetUnsubs.Add(() => _cameraBoundsRegistry.OnBoundsRegistered -= Handler);
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
            ServiceLocator.Current.Get<CameraController>().FocusOnPosition(target.WorldAnchor.position);
        }

        private void ClearPendingSubscriptions()
        {
            foreach (var unsub in _pendingTargetUnsubs) unsub();
            _pendingTargetUnsubs.Clear();
        }
    }
}