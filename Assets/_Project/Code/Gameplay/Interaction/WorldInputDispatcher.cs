using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Systems.Interaction;
using Galactic1.Code.Utility;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Interaction
{
    /// <summary>
    /// Центральный input router (AAA):
    /// 1. Raw pointer events (для targeting pipeline)
    /// 2. Gameplay commands (move / interact)
    /// </summary>
    public class WorldInputDispatcher :
        MonoBehaviour,
        IGameService,
        IUpdate,
        IWorldPointerService
    {
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask groundLayer;

        private Camera _camera;
        public UIInputExceptionRegistry UIInputExceptionRegistry { get; private set; }
        private SceneInteractionBlocker _interactionBlocker;

        private bool _overUI;

        // =========================
        // Movement
        // =========================
        public event Action<Vector3, MoveMode> OnMoveCommandIssued;

        public enum MoveMode
        {
            Walk,
            Run
        }

        private ClickDetector _clickDetector = new();
        private Vector2 _pendingClickPosition;
        private bool _pendingClick;
        private bool _touchActive;
        private bool _pointerGestureStartedOverUI;
        private bool _touchOverUI;
        private int _activeFingerId = -1;

        // =========================
        // Pointer (AAA pipeline input)
        // =========================
        public event Action<WorldPointerHit, WorldPointerHit> OnPointerDown;
        public event Action<WorldPointerHit, WorldPointerHit> OnPointerDrag;
        public event Action<WorldPointerHit, WorldPointerHit> OnPointerUp;
        public event Action OnCancel;

        // =========================
        // Init
        // =========================
        private void Start()
        {
            EventBus<LoadAndStartCoreEvent>.Register(
                new EventBinding<LoadAndStartCoreEvent>(() =>
                {
                    ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
                }));

            // для новой сцены очищаем подписки
            EventBus<SceneServicesResetReusableEvent>.Register(new EventBinding<SceneServicesResetReusableEvent>(() =>
            {
                OnPointerDown = null;
                OnPointerDrag = null;
                OnPointerUp = null;
                OnCancel = null;
            }));
        }

        public void Setup(Camera camera)
        {
            _camera = camera;
            _interactionBlocker = ServiceLocator.Current.Get<SceneInteractionBlocker>();
            UIInputExceptionRegistry = new UIInputExceptionRegistry();
        }

        // =========================
        // Update
        // =========================
        public void UpdateM()
        {
            ProcessPointerRaw(); // ВСЕГДА (targeting pipeline)

            if (_interactionBlocker.IsBlocked)
                return;

            ProcessGameplayInput();
            ResolvePendingClick();
        }

        public void IUpdateClear() { }

        // =========================
        // RAW POINTER (AAA)
        // =========================
        private void ProcessPointerRaw()
        {
#if UNITY_EDITOR || UNITY_STANDALONE

            if (Input.GetMouseButtonDown(0))
            {
                _pointerGestureStartedOverUI =
                    IsPointerOverAnyUI(Input.mousePosition);

                if (_pointerGestureStartedOverUI)
                    return;
                
                if (TryGetWorld(out var ground, out var any))
                    OnPointerDown?.Invoke(ground, any);
            }
            
            if (_pointerGestureStartedOverUI)
            {
                if (Input.GetMouseButtonUp(0))
                    _pointerGestureStartedOverUI = false;

                return;
            }

            if (Input.GetMouseButton(0))
            {
                if (TryGetWorld(out var ground, out var any))
                    OnPointerDrag?.Invoke(ground, any);
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (TryGetWorld(out var ground, out var any))
                    OnPointerUp?.Invoke(ground, any);
            }

            if (Input.GetMouseButtonDown(1))
            {
                OnCancel?.Invoke();
            }

#else

            if (Input.touchCount == 0)
                return;

            // Начало нового touch gesture
            if (!_touchActive)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);

                    if (touch.phase != TouchPhase.Began)
                        continue;

                    _activeFingerId = touch.fingerId;
                    _touchOverUI = IsPointerOverAnyUI(touch.position);

                    _touchActive = true;

                    // Touch начался на обычном UI — весь gesture блокируем.
                    if (_touchOverUI)
                        return;

                    if (TryGetWorld(touch.position, out var ground, out var any))
                        OnPointerDown?.Invoke(ground, any);

                    return;
                }
            }

            // Ищем именно тот finger, который начал gesture.
            if (_activeFingerId < 0)
                return;

            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);

                if (touch.fingerId != _activeFingerId)
                    continue;

                // Touch начался на UI — ничего из этого gesture
                // не должно попасть в gameplay.
                if (_touchOverUI)
                {
                    if (touch.phase == TouchPhase.Ended ||
                        touch.phase == TouchPhase.Canceled)
                    {
                        _touchActive = false;
                        _touchOverUI = false;
                        _activeFingerId = -1;
                    }

                    return;
                }

                switch (touch.phase)
                {
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:

                        if (TryGetWorld(touch.position, out var ground, out var any))
                            OnPointerDrag?.Invoke(ground, any);

                        break;

                    case TouchPhase.Ended:

                        if (TryGetWorld(touch.position, out var endGround, out var endAny))
                            OnPointerUp?.Invoke(endGround, endAny);

                        _touchActive = false;
                        _touchOverUI = false;
                        _activeFingerId = -1;

                        break;

                    case TouchPhase.Canceled:

                        _touchActive = false;
                        _touchOverUI = false;
                        _activeFingerId = -1;

                        break;
                }

                return;
            }

#endif
        }

        // =========================
        // GAMEPLAY INPUT
        // =========================
        private void ProcessGameplayInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE

            if (Input.GetMouseButtonDown(0))
                _overUI = IsPointerOverUI();

            if (Input.GetMouseButtonUp(0))
            {
                if (!PointerTracker.WasDragging && !_overUI)
                    ProcessWorldClick(Input.mousePosition);

                _overUI = false;
            }

#else
            if (Input.touchCount == 0)
                return;

            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                _overUI = UnityEngine.EventSystems.EventSystem.current
                    .IsPointerOverGameObject(touch.fingerId);

            if (touch.phase == TouchPhase.Ended)
            {
                if (!PointerTracker.WasDragging && !_overUI)
                    ProcessWorldClick(touch.position);

                _overUI = false;
            }
#endif
        }

        // =========================
        // WORLD CLICK
        // =========================
        private void ProcessWorldClick(Vector2 screenPos)
        {
            var ray = _camera.ScreenPointToRay(screenPos);

            // 1. interactable приоритет
            if (Physics.Raycast(ray, out var hit, 100f, interactableLayer))
            {
                if (hit.collider.GetComponentInParent<IInteractable>() is IInteractable i)
                {
                    i.OnInteract();
                    return;
                }
            }

            // 2. movement
            if (Physics.Raycast(ray, out var groundHit, 100f, groundLayer))
            {
                var clickType = _clickDetector.RegisterClick(screenPos);

                if (clickType == ClickType.Double)
                {
                    _pendingClick = false;
                    IssueMoveCommand(groundHit.point, MoveMode.Run);
                    return;
                }

                if (clickType == ClickType.Pending)
                {
                    _pendingClick = true;
                    _pendingClickPosition = screenPos;
                }
            }
        }

        private void ResolvePendingClick()
        {
            if (!_pendingClick)
                return;

            var result = _clickDetector.Update();

            if (result != ClickType.Single)
                return;

            _pendingClick = false;

            var ray = _camera.ScreenPointToRay(_pendingClickPosition);

            if (Physics.Raycast(ray, out var groundHit, 100f, groundLayer))
            {
                IssueMoveCommand(groundHit.point, MoveMode.Walk);
            }
        }

        private void IssueMoveCommand(Vector3 position, MoveMode mode)
        {
            OnMoveCommandIssued?.Invoke(position, mode);
        }

        // =========================
        // Utils
        // =========================
        public bool TryGetWorld(out WorldPointerHit result)
        {
            Vector2 screenPosition = Input.mousePosition;
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, 200f))
            {
                result = CreatePointerHit(hit, screenPosition);

                return true;
            }

            result = WorldPointerHit.Invalid;
            return false;
        }

        public bool TryGetWorld(
            out WorldPointerHit groundHit,
            out WorldPointerHit anyHit)
        {
            Vector2 screenPosition = Input.mousePosition;
            var ray = _camera.ScreenPointToRay(screenPosition);

            bool hitGround = Physics.Raycast(
                ray,
                out var ground,
                200f,
                groundLayer);

            bool hitAny = Physics.Raycast(
                ray,
                out var any,
                200f);

            groundHit = hitGround
                ? CreatePointerHit(ground, screenPosition)
                : WorldPointerHit.Invalid;

            anyHit = hitAny
                ? CreatePointerHit(any, screenPosition)
                : WorldPointerHit.Invalid;

            return hitGround || hitAny;
        }

        private bool TryGetWorld(
            Vector2 screenPosition,
            out WorldPointerHit groundHit,
            out WorldPointerHit anyHit)
        {
            var ray = _camera.ScreenPointToRay(screenPosition);

            bool hitGround = Physics.Raycast(
                ray,
                out var ground,
                200f,
                groundLayer);

            bool hitAny = Physics.Raycast(
                ray,
                out var any,
                200f);

            groundHit = hitGround
                ? CreatePointerHit(ground, screenPosition)
                : WorldPointerHit.Invalid;

            anyHit = hitAny
                ? CreatePointerHit(any, screenPosition)
                : WorldPointerHit.Invalid;

            return hitGround || hitAny;
        }
        
        
        // =========================
        // Utils
        // =========================

        private static WorldPointerHit CreatePointerHit(
            RaycastHit hit,
            Vector2 screenPosition)
        {
            return new WorldPointerHit
            {
                Position = hit.point,
                Normal = hit.normal,
                Collider = hit.collider,
                GameObject = hit.collider.gameObject,
                ScreenPosition = screenPosition,
                IsValid = true
            };
        }




        private bool IsPointerOverUI()
        {
#if UNITY_EDITOR || UNITY_STANDALONE

            return IsPointerOverUI(Input.mousePosition);

#else

            if (Input.touchCount == 0)
                return false;

            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);

                if (IsPointerOverUI(touch.position))
                    return true;
            }

            return false;

#endif
        }

        private bool IsPointerOverUI(Vector2 screenPosition)
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;

            if (eventSystem == null)
                return false;

            var eventData =
                new UnityEngine.EventSystems.PointerEventData(eventSystem)
                {
                    position = screenPosition
                };

            var results =
                new List<UnityEngine.EventSystems.RaycastResult>();

            eventSystem.RaycastAll(eventData, results);

            if (results.Count == 0)
                return false;

            foreach (var result in results)
            {
                if (IsUIException(result.gameObject))
                    return false;
            }

            return true;
        }

        private bool IsUIException(GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            Transform current = gameObject.transform;

            while (current != null)
            {
                if (UIInputExceptionRegistry.UiExceptionTags.Contains(current.tag))
                    return true;

                current = current.parent;
            }

            return false;
        }
        
        private bool IsPointerOverAnyUI(Vector2 screenPosition)
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;

            if (eventSystem == null)
                return false;

            var eventData =
                new UnityEngine.EventSystems.PointerEventData(eventSystem)
                {
                    position = screenPosition
                };

            var results =
                new List<UnityEngine.EventSystems.RaycastResult>();

            eventSystem.RaycastAll(eventData, results);

            return results.Count > 0;
        }
    }
}