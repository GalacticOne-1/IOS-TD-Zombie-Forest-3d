using System.Collections;
using System.Collections.Generic;
using Galactic1.Code.Cameras.Configs;
using Galactic1.Code.Systems;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1.Code.Cameras
{
    public class CameraControllerMap : MonoBehaviour, IUpdate, IMainCamera
    {
        [SerializeField] private CameraConfig config;

        private Vector3 minBounds;
        private Vector3 maxBounds;

        private UIDetector _uiDetector;
        private Camera cam;
        private Transform trRoot;
        private Transform tr;

        private Vector3 velocity;
        private Vector3 lastMousePosition;
        private bool isDragging;

        private float targetZoomDistance;
        private float zoomVelocity;
        private bool isPinching;

        // Владение жестом: решается ОДИН раз в начале касания
        private bool gestureActive;
        private bool inputBlockedByUI;
        private int gestureFrames;
        private const int GestureRecheckFrames = 3;

        // Щипок: зум блокируется, только если ОБА пальца начали касание над UI
        private readonly HashSet<int> fingersOnUI = new HashSet<int>();
        private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        private PointerEventData pointerData;

        private bool externalBlocked;   // внешний OnFreeze, решается в начале жеста
        private bool pinchDecided;
        private bool pinchBlocked;

        public ReactiveProperty<bool> Freeze { get; set; }
        public DFuncResponse OnFreeze;

        public Camera Camera
        {
            get
            {
                if (cam == null)
                    cam = GetComponentInChildren<Camera>();
                return cam;
            }
        }

        public void Activate()
        {
            cam = GetComponentInChildren<Camera>();
            if (cam == null)
            {
                Debug.LogError("CameraController requires Camera!");
                enabled = false;
                return;
            }

            trRoot = transform;
            tr = transform.GetChild(0);
            targetZoomDistance = tr.localPosition.y;

            _uiDetector = ServiceLocator.Current.Get<UIDetector>();

            Freeze = new ReactiveProperty<bool>(false);
            Freeze.Skip(1).Subscribe(_ =>
            {
                isDragging = false;
                gestureActive = false;
                inputBlockedByUI = false;
                externalBlocked = false;
                pinchDecided = false;
                pinchBlocked = false;
                fingersOnUI.Clear();
                lastMousePosition = Input.mousePosition;
                velocity = Vector3.zero;
            });

            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() =>
            {
                IUpdateClear();
            }));
        }

        public void OnLevelLoaded(
            CameraConfig cameraConfig,
            Vector3 startPosition,
            Vector3 newMinBounds,
            Vector3 newMaxBounds,
            float? startZoom = null)
        {
            config = cameraConfig;

            // Сброс ввода
            isDragging = false;
            isPinching = false;
            gestureActive = false;
            inputBlockedByUI = false;
            externalBlocked = false;
            pinchDecided = false;
            pinchBlocked = false;
            fingersOnUI.Clear();
            velocity = Vector3.zero;
            zoomVelocity = 0f;

            // Сброс фризов
            if (Freeze != null)
                Freeze.Value = false;

            // Границы
            minBounds = newMinBounds;
            maxBounds = newMaxBounds;

            // Позиция
            trRoot.position = ClampToBounds(startPosition);

            // Зум
            var localPos = tr.localPosition;
            if (startZoom.HasValue)
                localPos.y = Mathf.Clamp(startZoom.Value, config.MinZoom, config.MaxZoom);
            tr.localPosition = localPos;
            targetZoomDistance = localPos.y;
        }

        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        public void UpdateM()
        {
            TrackTouches();

            // Фриз: ввод игнорируем, но зум доглаживаем
            if (Freeze.Value)
            {
                ApplyZoomSmoothing();
                return;
            }

            UpdateGestureOwnership();

            // Щипок (2+ пальца): блокировка, только если ОБА пальца на UI.
            // Один палец / мышь: как раньше.
            bool blocked;
            if (Input.touchCount >= 2)
            {
                blocked = IsPinchBlocked();
            }
            else
            {
                pinchDecided = false;
                blocked = inputBlockedByUI;
            }

            if (!blocked)
            {
                ReadZoomInput();
                HandleMouseDrag();
            }
            else
            {
                // Ввод заблокирован: HandleMouseDrag не увидит GetKeyUp,
                // поэтому сбрасываем драг и щипок, чтобы не осталось устаревшего состояния
                isDragging = false;
                isPinching = false;
            }

            if (!isDragging)
                ApplyInertia();

            ApplyMovement();

            // Всегда, независимо от UI и состояния ввода
            ApplyZoomSmoothing();
        }

        // ===== Щипок: UI-защита по каждому пальцу =====

        /// <summary>
        /// Запоминает, какие пальцы начали касание над UI.
        /// </summary>
        private void TrackTouches()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);

                if (t.phase == TouchPhase.Began)
                {
                    if (IsScreenPointOverUI(t.position))
                        fingersOnUI.Add(t.fingerId);
                    else
                        fingersOnUI.Remove(t.fingerId);
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    fingersOnUI.Remove(t.fingerId);
                }
            }

            if (Input.touchCount == 0)
                fingersOnUI.Clear();
        }

        /// <summary>
        /// Решается один раз в момент появления второго пальца.
        /// Зум заблокирован, только если оба пальца над UI
        /// (или сработал внешний OnFreeze в начале жеста).
        /// </summary>
        private bool IsPinchBlocked()
        {
            if (!pinchDecided)
            {
                pinchDecided = true;

                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);

                pinchBlocked = externalBlocked
                               || (fingersOnUI.Contains(a.fingerId) && fingersOnUI.Contains(b.fingerId));
            }

            return pinchBlocked;
        }

        private bool IsScreenPointOverUI(Vector2 screenPos)
        {
            var es = EventSystem.current;
            if (es == null)
                return false;

            if (pointerData == null || pointerData.currentInputModule != es.currentInputModule)
                pointerData = new PointerEventData(es);

            pointerData.position = screenPos;
            raycastResults.Clear();
            es.RaycastAll(pointerData, raycastResults);
            return raycastResults.Count > 0;
        }

        /// <summary>
        /// Все условия блокировки камеры (UI + внешний OnFreeze) проверяются
        /// ТОЛЬКО в начале жеста и дальше не пересматриваются. Иначе во время
        /// щипка палец, проходящий над UI, начинает попеременно блокировать
        /// и разблокировать зум.
        /// </summary>
        private bool IsBlockedAtGestureStart()
        {
            // Как в исходном CanProcessInput: OnFreeze блокирует, только если нет инерции.
            // Результат запоминаем отдельно: он нужен для решения по щипку.
            externalBlocked = OnFreeze != null && velocity == Vector3.zero && OnFreeze.Invoke();

            return externalBlocked || IsBlockedByUI();
        }

        // ===== Владение жестом (UI vs камера) =====

        private void UpdateGestureOwnership()
        {
            // Единая логика для мыши и тача, без привязки к #if платформы:
            // в редакторе с активной iOS/Android-платформой define UNITY_IOS задан,
            // но Input.touchCount == 0 (используется мышь), и проверка UI не срабатывала бы.
            bool pointerDown = Input.GetMouseButton(0) || Input.touchCount > 0;

            if (!pointerDown)
            {
                gestureActive = false;
                gestureFrames = 0;
                externalBlocked = false;

                // Колесо мыши: проверяем курсор в момент прокрутки
                inputBlockedByUI = Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0f
                                   && IsBlockedAtGestureStart();
                return;
            }

            if (!gestureActive)
            {
                // Начало жеста: решение принимается по первому касанию
                gestureActive = true;
                gestureFrames = 0;
                inputBlockedByUI = IsBlockedAtGestureStart();
                return;
            }

            // Короткое окно перепроверки: состояние UIDetector может обновиться
            // на кадр позже, чем пришло касание. Блокировка только добавляется,
            // снять её в середине жеста нельзя. Только пока палец один:
            // как только появился второй (щипок), решение больше не пересматривается.
            if (!inputBlockedByUI && Input.touchCount <= 1 && gestureFrames < GestureRecheckFrames)
            {
                gestureFrames++;
                if (IsBlockedByUI())
                    inputBlockedByUI = true;
            }
        }

        /// <summary>
        /// Та же проверка, что была в исходном CanProcessInput.
        /// UIDetector смотрит на Input.mousePosition, который на тач-устройствах
        /// равен позиции первого пальца.
        /// </summary>
        private bool IsBlockedByUI()
        {
            return _uiDetector.IsPointerOverUI || _uiDetector.HasUIUnderCursor();
        }

        // ===== Ввод =====

        private void HandleMouseDrag()
        {
#if UNITY_IOS || UNITY_ANDROID
            if (isPinching || Input.touchCount > 1)
            {
                // Мультитач: обрываем драг, иначе lastMousePosition устареет
                // и при возврате к одному пальцу камера прыгнет
                isDragging = false;
                return;
            }
#endif

            if (Input.GetKeyDown(config.DragButton))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
                velocity = Vector3.zero;
                return;
            }

            if (Input.GetKeyUp(config.DragButton))
            {
                isDragging = false;
                return;
            }

            if (!isDragging)
                return;

            // Защита от пропущенного GetKeyUp (например, системный жест iOS)
            if (!Input.GetKey(config.DragButton))
            {
                isDragging = false;
                return;
            }

            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            if (config.InvertDrag)
                mouseDelta = -mouseDelta;

            float zoomFactor = cam.orthographicSize / config.MaxZoom;

            // Дельта пикселей за кадр -> скорость, не зависящая от FPS.
            // ApplyMovement умножает скорость на deltaTime, поэтому перемещение
            // за кадр всегда равно delta * DragSpeed * zoomFactor / 60.
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            const float referenceDt = 1f / 60f;

            Vector3 dragMove = new Vector3(
                -mouseDelta.x,
                0,
                -mouseDelta.y
            ) * (config.DragSpeed * zoomFactor * referenceDt / dt);

            velocity = Vector3.Lerp(
                velocity,
                dragMove,
                config.DragResponsiveness * Time.deltaTime
            );
        }

        /// <summary>
        /// Только читает ввод и меняет targetZoomDistance.
        /// </summary>
        private void ReadZoomInput()
        {
            float zoomInput = 0f;
            isPinching = false;

#if UNITY_EDITOR || UNITY_STANDALONE
            zoomInput = Input.GetAxis("Mouse ScrollWheel") * config.ZoomSpeed;
#endif

#if UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount == 2)
            {
                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);

                float prev = Vector2.Distance(
                    a.position - a.deltaPosition,
                    b.position - b.deltaPosition);
                float curr = Vector2.Distance(a.position, b.position);

                zoomInput = (curr - prev) * config.PinchZoomSpeed;

                isPinching = true;
                isDragging = false;
                velocity = Vector3.zero;
            }
#endif

            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                targetZoomDistance = Mathf.Clamp(
                    targetZoomDistance - zoomInput,
                    config.MinZoom,
                    config.MaxZoom
                );
            }
        }

        /// <summary>
        /// Плавное движение к целевому зуму. Вызывается каждый кадр.
        /// </summary>
        private void ApplyZoomSmoothing()
        {
            var localPos = tr.localPosition;
            localPos.y = Mathf.SmoothDamp(
                localPos.y,
                targetZoomDistance,
                ref zoomVelocity,
                config.ZoomSmoothTime
            );
            tr.localPosition = localPos;
        }

        // ===== Движение =====

        private void ApplyInertia()
        {
            if (velocity == Vector3.zero)
                return;

            velocity = Vector3.Lerp(
                velocity,
                Vector3.zero,
                config.InertiaDamping * Time.deltaTime
            );

            if (velocity.magnitude < config.MinInertiaSpeed)
                velocity = Vector3.zero;
        }

        private void ApplyMovement()
        {
            if (velocity == Vector3.zero)
                return;

            Vector3 newPos = trRoot.position + velocity * Time.deltaTime;
            trRoot.position = ClampToBounds(newPos);
        }

        private Vector3 ClampToBounds(Vector3 position)
        {
            if (!config.UseCameraBounds)
                return position;

            position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
            position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
            position.z = Mathf.Clamp(position.z, minBounds.z, maxBounds.z);

            return position;
        }

        // ===== External API =====

        public void SetCameraPosition(Vector3 position)
        {
            trRoot.position = position;
            velocity = Vector3.zero;
        }

        public void AddForce(Vector3 force)
        {
            velocity += force;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }

        public void FocusOnPosition(Vector3 target, float duration = .35f)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothMoveToPosition(target, duration));
        }

        private IEnumerator SmoothMoveToPosition(Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = trRoot.position;
            targetPosition = ClampToBounds(targetPosition);

            float elapsed = 0f;
            velocity = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);

                trRoot.position = ClampToBounds(Vector3.Lerp(startPosition, targetPosition, t));

                yield return null;
            }

            trRoot.position = targetPosition;
        }

        public void FocusOnSquad()
        {
            var group = ServiceLocator.Current.Get<CameraTargetGroup>();

            if (group == null || !group.HasTargets)
                return;

            FocusOnPosition(group.GetCenter(), 0.35f);
        }
    }
}