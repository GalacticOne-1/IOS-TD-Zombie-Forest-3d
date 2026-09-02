using System;
using Galactic1.Code.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Targeting
{
    /// <summary>
    /// AAA pipeline для targeting input (RTS-style)
    /// Управляет pointer flow: down → drag → up → confirm / cancel
    /// </summary>
    public sealed class TargetingInputPipeline
    {
        private readonly IWorldPointerService _pointer;
        

        public event Action<Vector3> OnStart; // первый клик
        public event Action<Vector3> OnUpdate; // drag / move
        public event Action<Vector3> OnConfirm; // отпускание
        public event Action OnCancel; // отмена

        private bool _isActive;
        private bool _isDragging;

        private float _dragThreshold = 0.1f;
        private Vector3 _startPos;

        private ITargetingCancelZone _cancelZone;
        private bool _isInCancelZone;

        public TargetingInputPipeline(IWorldPointerService pointer)
        {
            _pointer = pointer;
        }

        // =========================
        // Cancel Zone
        // =========================

        /// <summary>
        /// Регистрирует UI Cancel Zone. Вызывается один раз при инициализации
        /// сцены (см. CombatTargetingService.Initialize), не привязано к
        /// конкретной targeting-сессии.
        /// </summary>
        public void SetCancelZone(ITargetingCancelZone cancelZone)
        {
            _cancelZone = cancelZone;
        }

        // =========================
        // Lifecycle
        // =========================
        public void Activate()
        {
            if (_isActive) return;

            _pointer.UIInputExceptionRegistry.AddUIInputExceptionTag("AbilityCancelZone");
            _pointer.OnPointerDown += HandleDown;
            _pointer.OnPointerDrag += HandleDrag;
            _pointer.OnPointerUp += HandleUp;
            _pointer.OnCancel += HandleCancel;

            _isActive = true;
        }

        public void Deactivate()
        {
            if (!_isActive) return;

            _pointer.UIInputExceptionRegistry.RemoveUIInputExceptionTag("AbilityCancelZone");
            _pointer.OnPointerDown -= HandleDown;
            _pointer.OnPointerDrag -= HandleDrag;
            _pointer.OnPointerUp -= HandleUp;
            _pointer.OnCancel -= HandleCancel;

            _isActive = false;
            _isDragging = false;

            ResetCancelZoneHighlight();
        }

        // =========================
        // Handlers
        // =========================

        private void HandleDown(WorldPointerHit ground, WorldPointerHit any)
        {
            _startPos = ground.Position;
            _isDragging = false;
            _isInCancelZone = false;

            OnStart?.Invoke(ground.Position);
        }

        private void HandleDrag(WorldPointerHit ground, WorldPointerHit any)
        {
            if (!_isDragging)
            {
                if (Vector3.Distance(_startPos, ground.Position) > _dragThreshold)
                    _isDragging = true;
            }

            UpdateCancelZoneState(any.ScreenPosition);

            OnUpdate?.Invoke(ground.Position);
        }

        private void HandleUp(WorldPointerHit ground, WorldPointerHit any)
        {
            bool releasedInCancelZone =
                _cancelZone != null && _cancelZone.ContainsScreenPoint(any.ScreenPosition);

            ResetCancelZoneHighlight();

            if (releasedInCancelZone)
            {
                OnCancel?.Invoke();
                return;
            }

            OnConfirm?.Invoke(ground.Position);
        }

        private void HandleCancel()
        {
            ResetCancelZoneHighlight();
            OnCancel?.Invoke();
        }

        // =========================
        // Cancel Zone helpers
        // =========================
        private void UpdateCancelZoneState(Vector2 screenPosition)
        {
            if (_cancelZone == null)
                return;

            bool inZone = _cancelZone.ContainsScreenPoint(screenPosition);

            if (inZone == _isInCancelZone)
                return;

            _isInCancelZone = inZone;
            _cancelZone.SetHighlighted(inZone);
        }

        private void ResetCancelZoneHighlight()
        {
            if (_cancelZone == null)
                return;

            if (_isInCancelZone)
            {
                _isInCancelZone = false;
                _cancelZone.SetHighlighted(false);
            }
        }
    }
}