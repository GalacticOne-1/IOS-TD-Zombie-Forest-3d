using System.Collections;
using Galactic1.Code.Cameras.Configs;
using Galactic1.Code.Systems;
using Galactic1.Code.Systems.Squad;
using Galactic1.Code.Systems.Tutorial.Runtime;
using UniRx;
using UnityEngine;

namespace Galactic1.Code.Cameras
{
    public class CameraController : MonoBehaviour, IUpdate, IMainCamera
    {
        [SerializeField] private CameraConfig config;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        private TutorialCapabilityPolicy _tutorialCapabilities;

        private TutorialCapabilityPolicy TutorialCapabilities
            => _tutorialCapabilities ??= ServiceLocator.Current.Get<TutorialCapabilityPolicy>();

        // ADDED — tutorial camera bounds state
        private bool _hasTutorialBounds;
        private Vector3 _tutorialMinBounds;
        private Vector3 _tutorialMaxBounds;

        [Space] [SerializeField] private Transform trPivot;
        [SerializeField] private Transform trCam;

        private Vector3 minBounds;
        private Vector3 maxBounds;


        private UIDetector _uiDetector;
        private Camera cam;
        private Transform trRoot;

        private Vector3 velocity;
        private Vector3 lastMousePosition;
        private bool isDragging;

        private float targetTilt;
        private float tiltVelocity;
        private float targetZoomDistance;
        private float zoomVelocity;
        private bool isPinching;

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

        public Vector3 FocusPosition
        {
            get
            {
                if (ScreenCenterToBuildPlane(out var point, Vector2.zero))
                    return point;
                return trRoot.position;
            }
        }

        private SquadController _squadController;

        private SquadController SquadCtl
        {
            get
            {
                if (_squadController == null)
                    _squadController = ServiceLocator.Current.Get<SquadController>();
                return _squadController;
            }
        }

        private float _runtimeMaxZoom;

        private Touch t0;
        private Touch t1;
        private Vector2 t0Prev;
        private Vector2 t1Prev;
        private float prevDistance;
        private float currDistance;
        private float pinchDelta;
        private RaycastHit? hit;

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
            _uiDetector = ServiceLocator.Current.Get<UIDetector>();

            Freeze = new ReactiveProperty<bool>(false);
            Freeze.Skip(1).Subscribe(_ =>
            {
                isDragging = false;
                lastMousePosition = Input.mousePosition;
                velocity = Vector3.zero;
            });

            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() =>
            {
                IUpdateClear();
                _squadController = null;
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

            isDragging = false;
            velocity = Vector3.zero;

            if (Freeze != null)
                Freeze.Value = false;

            // ADDED — новая сцена не должна унаследовать tutorial bounds предыдущей;
            // TutorialPresentationService.Hide()/новый Show() применит свежий constraint
            // отдельно, это защитный сброс на случай, если presentation-слой не успел.
            _hasTutorialBounds = false;

            minBounds = newMinBounds;
            maxBounds = newMaxBounds;

            trRoot.position = ClampToBounds(new Vector3(
                startPosition.x,
                startPosition.y,
                startPosition.z
            ));

            targetTilt = config.DefaultTilt;
            _runtimeMaxZoom = config.MaxZoom;

            if (startZoom.HasValue)
            {
                var localPos = trPivot.localPosition;
                localPos.y = config.MinZoom;
                trPivot.localPosition = localPos;
                targetZoomDistance = trPivot.localPosition.y;
            }
        }

        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        public void UpdateM()
        {
            UpdateZoom();
            UpdateTilt();

            if (CanProcessInput())
            {
                HandleZoomInput();
                HandleMouseDrag();

                if (!isDragging)
                    ApplyInertia();

                ApplyMovement();
            }
        }

        private bool CanProcessInput()
        {
            if (Freeze.Value)
                return false;

            if (TutorialCapabilities != null && !TutorialCapabilities.CanControlCamera)
                return false;

            var f = false;
            if (OnFreeze != null)
                f = OnFreeze.Invoke();
            if (velocity == Vector3.zero && f)
                return false;

            if (!Input.GetMouseButtonDown(0) && (isDragging || velocity.sqrMagnitude > 0.001f))
                return true;

            return !_uiDetector.IsPointerOverUI && !_uiDetector.HasUIUnderCursor();
        }

        private void HandleMouseDrag()
        {
#if UNITY_IOS || UNITY_ANDROID
            if (isPinching || Input.touchCount > 1)
                return;
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

            if (!isDragging) return;

            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            if (config.InvertDrag)
                mouseDelta = -mouseDelta;

            float zoomFactor = cam.orthographicSize / config.MaxZoom;

            Vector3 dragMove = new Vector3(-mouseDelta.x, 0, -mouseDelta.y) * config.DragSpeed * zoomFactor;

            velocity = Vector3.Lerp(velocity, dragMove, config.DragResponsiveness * Time.deltaTime);
        }

        private void HandleZoomInput()
        {
            float zoomInput = 0f;
            isPinching = false;

#if UNITY_EDITOR || UNITY_STANDALONE
            zoomInput = Input.GetAxis("Mouse ScrollWheel") * config.ZoomSpeed;
#endif

#if UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount == 2)
            {
                t0 = Input.GetTouch(0);
                t1 = Input.GetTouch(1);
                t0Prev = t0.position - t0.deltaPosition;
                t1Prev = t1.position - t1.deltaPosition;
                prevDistance = Vector2.Distance(t0Prev, t1Prev);
                currDistance = Vector2.Distance(t0.position, t1.position);
                pinchDelta = currDistance - prevDistance;
                zoomInput = pinchDelta * config.PinchZoomSpeed;
                isPinching = true;
                isDragging = false;
                velocity = Vector3.zero;
            }
#endif

            if (Mathf.Abs(zoomInput) > 0.01f)
                targetZoomDistance = Mathf.Clamp(targetZoomDistance - zoomInput, config.MinZoom, _runtimeMaxZoom);
        }

        private void UpdateZoom()
        {
            var localPos = trPivot.localPosition;
            localPos.y = Mathf.SmoothDamp(localPos.y, targetZoomDistance, ref zoomVelocity, config.ZoomSmoothTime);
            trPivot.localPosition = localPos;
        }

        private void ApplyInertia()
        {
            if (velocity == Vector3.zero) return;

            velocity = Vector3.Lerp(velocity, Vector3.zero, config.InertiaDamping * Time.deltaTime);

            if (velocity.magnitude < config.MinInertiaSpeed)
                velocity = Vector3.zero;
        }

        private Vector3 ClampToBounds(Vector3 position)
        {
            if (config.UseCameraBounds)
            {
                position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
                position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
                position.z = Mathf.Clamp(position.z, minBounds.z, maxBounds.z);
            }

            // ADDED — раздел 14 ТЗ: Level Bounds → Tutorial Bounds → Squad Radius.
            // Применяется поверх level bounds, только XZ (Y не трогаем — раздел 18 ТЗ).
            // Работает автоматически и для drag+inertia (ApplyMovement), и для
            // FocusOnPosition/FocusOnPositionFacility (SmoothMoveToPosition) — оба пути
            // уже проходят через этот единственный ClampToBounds, второй pipeline не нужен.
            if (_hasTutorialBounds)
            {
                position.x = Mathf.Clamp(position.x, _tutorialMinBounds.x, _tutorialMaxBounds.x);
                position.z = Mathf.Clamp(position.z, _tutorialMinBounds.z, _tutorialMaxBounds.z);
            }

            return ClampToSquadRadius(position);
        }

        private void ApplyMovement()
        {
            Vector3 newPos = trRoot.position + velocity * Time.deltaTime;
            trRoot.position = ClampToBounds(newPos);
        }

        private void UpdateTilt()
        {
            float currentTilt = trCam.localEulerAngles.x;
            float tilt = Mathf.SmoothDampAngle(currentTilt, targetTilt, ref tiltVelocity, config.TiltSmooth);
            trCam.localRotation = Quaternion.Euler(tilt, 0f, 0f);
        }

        public bool ScreenCenterToBuildPlane(out Vector3 worldPoint, Vector2 viewportOffset, float planeY = 0f)
        {
            Vector3 viewportPoint = new Vector3(0.5f + viewportOffset.x, 0.5f + viewportOffset.y, 0f);
            Ray ray = Camera.ViewportPointToRay(viewportPoint);
            Plane buildPlane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

            if (buildPlane.Raycast(ray, out float enter))
            {
                worldPoint = ray.GetPoint(enter);
                return true;
            }

            worldPoint = Vector3.zero;
            return false;
        }

        public void SetCameraPosition(Vector3 position)
        {
            // NOTE: намеренно НЕ добавлен ClampToBounds — не видел call sites этого API,
            // менять поведение authoritative placement без подтверждения рискованно
            // (раздел 17 ТЗ). Если это используется для сценового телепорта камеры внутри
            // tutorial bounds — сообщите, добавлю clamp.
            trRoot.position = new Vector3(position.x, position.y, position.z);
            velocity = Vector3.zero;
        }

        public void AddForce(Vector3 force) => velocity += force;

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }

        public void FocusOnPosition(Vector3 target, float duration = .2f)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothMoveToPosition(target, duration));
        }

        public void FocusOnPositionFacility(Vector3 target, bool constructionMode = true)
        {
            if (!ScreenCenterToBuildPlane(
                    out var desiredFocusPoint,
                    new Vector2(0f, constructionMode
                        ? config.ConstructionModeFocusViewportOffsetY
                        : config.FacilityFocusViewportOffsetY)))
                return;

            Vector3 delta = target - desiredFocusPoint;
            Vector3 cameraTarget = trRoot.position + delta;

            StopAllCoroutines();
            StartCoroutine(SmoothMoveToPosition(cameraTarget, config.FocusDuration));
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
                float t = elapsed / duration;
                t = moveCurve.Evaluate(t);

                Vector3 lerped = Vector3.Lerp(startPosition, targetPosition, t);
                trRoot.position = ClampToBounds(lerped);

                yield return null;
            }

            trRoot.position = ClampToBounds(targetPosition);
        }

        private Vector3 ClampToSquadRadius(Vector3 position)
        {
            if (!config.LimitToSquadRadius)
                return position;

            var squadCtl = SquadCtl;
            if (squadCtl?.Squad == null || squadCtl.Squad.Agents.Count == 0)
                return position;

            Vector3 squadCenter = squadCtl.Squad.ComputeMassCenter();
            Vector3 delta = position - squadCenter;
            delta.y = 0f;

            float radius = config.SquadRadiusLimit;
            if (delta.sqrMagnitude > radius * radius)
            {
                delta = delta.normalized * radius;
                position.x = squadCenter.x + delta.x;
                position.z = squadCenter.z + delta.z;
            }

            return position;
        }

        public void FocusOnSquad()
        {
            var group = ServiceLocator.Current.Get<CameraTargetGroup>();
            if (group == null || !group.HasTargets) return;
            FocusOnPosition(group.GetCenter(), 0.35f);
        }

        // ADDED — IMainCamera implementation
        public void SetTutorialBounds(Bounds bounds)
        {
            _hasTutorialBounds = true;
            _tutorialMinBounds = bounds.min;
            _tutorialMaxBounds = bounds.max;

            // Немедленно доклэмпить текущую позицию — иначе игрок увидит "прыжок" только
            // при следующем drag/inertia тике, а не сразу в момент применения constraint.
            trRoot.position = ClampToBounds(trRoot.position);
        }

        public void ClearTutorialBounds()
        {
            _hasTutorialBounds = false;
        }

        // =============================
        // Construction Mode
        // =============================

        private bool _constructionModeZoom = false;
        private float _cachedZoomDistance;

        public void EnterConstructionMode(float zoomDistance)
        {
            _cachedZoomDistance = targetZoomDistance;
            targetZoomDistance = Mathf.Clamp(zoomDistance, config.MinZoom, config.MaxZoom);
            targetTilt = config.ConstructionTilt;
            _runtimeMaxZoom = config.ConstructionMaxZoom;
            _constructionModeZoom = true;
        }

        public void ExitConstructionMode()
        {
            targetZoomDistance = _cachedZoomDistance;
            targetTilt = config.DefaultTilt;
            _runtimeMaxZoom = config.MaxZoom;
            zoomVelocity = 0f;
            _constructionModeZoom = false;
        }
    }
}