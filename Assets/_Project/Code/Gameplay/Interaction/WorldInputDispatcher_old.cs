
using System;
using Galactic1.Code.Systems.Interaction;
using Galactic1.Code.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Code.Gameplay.Interaction
{
    /// <summary>
    /// Центральная система обработки кликов / тапов игрока.
    /// </summary>
    public class WorldInputDispatcher_old : MonoBehaviour, IGameService, IUpdate
    {
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask groundLayer;

        private Camera camera;
        private bool overUI;

        public event Action<Vector3, MoveMode> OnMoveCommandIssued;

        public enum MoveMode
        {
            Walk,
            Run
        }


        private SceneInteractionBlocker interactionBlocker;
        private ClickDetector clickDetector = new ClickDetector();
        private Vector2 pendingClickPosition;
        private bool pendingClick;


        private void Start()
        {
            EventBus<LoadAndStartCoreEvent>.Register(new EventBinding<LoadAndStartCoreEvent>(() =>
            {
                ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
            }));
        }

        public void Setup(Camera camera)
        {
            this.camera = camera;

            interactionBlocker = ServiceLocator.Current.Get<SceneInteractionBlocker>();
        }


        public void UpdateM()
        {
            if (interactionBlocker.IsBlocked)
                return;

#if UNITY_EDITOR || UNITY_STANDALONE

            if (Input.GetMouseButtonDown(0))
                overUI = IsPointerOverUI();

            if (Input.GetMouseButtonUp(0))
            {
                if (!PointerTracker.WasDragging && !overUI)
                    ProcessWorldClick(Input.mousePosition);

                overUI = false;
            }

#else

            if (Input.touchCount == 0)
                return;

            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                overUI = UnityEngine.EventSystems.EventSystem.current
                    .IsPointerOverGameObject(touch.fingerId);

            if (touch.phase == TouchPhase.Ended)
            {
                if (!PointerTracker.WasDragging && !overUI)
                    ProcessWorldClick(touch.position);

                overUI = false;
            }

#endif
            
            ResolvePendingClick();
        }

        public void IUpdateClear()
        {
        }

        bool IsPointerOverUI()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
#else
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                        return true;
                }
            }
            return false;
#endif
        }

        private void ProcessWorldClick(Vector2 screenPos)
        {
            var ray = camera.ScreenPointToRay(screenPos);

            // 1. interactable имеет приоритет
            if (Physics.Raycast(ray, out var hit, 100f, interactableLayer))
            {
                if (hit.collider.GetComponentInParent<IInteractable>() is IInteractable i)
                {
                    i.OnInteract();
                    return;
                }
            }

            // движение
            if (Physics.Raycast(ray, out var groundHit, 100f, groundLayer))
            {
                var clickType = clickDetector.RegisterClick(screenPos);

                if (clickType == ClickType.Double)
                {
                    pendingClick = false;
                    IssueMoveCommand(groundHit.point, MoveMode.Run);
                    return;
                }

                if (clickType == ClickType.Pending)
                {
                    pendingClick = true;
                    pendingClickPosition = screenPos;
                }
            }
        }

        private void ResolvePendingClick()
        {
            if (!pendingClick)
                return;

            var result = clickDetector.Update();

            if (result != ClickType.Single)
                return;

            pendingClick = false;

            var ray = camera.ScreenPointToRay(pendingClickPosition);

            if (Physics.Raycast(ray, out var groundHit, 100f, groundLayer))
            {
                IssueMoveCommand(groundHit.point, MoveMode.Walk);
            }
        }

        private void IssueMoveCommand(Vector3 position, MoveMode mode)
        {
            OnMoveCommandIssued?.Invoke(position, mode);
        }


    }
}