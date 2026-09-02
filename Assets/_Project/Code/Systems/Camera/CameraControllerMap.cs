
using System.Collections;
using Galactic1.Code.Cameras.Configs;
using Galactic1.Code.Systems;
using UniRx;
using UnityEngine;

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
            tr = transform.GetChild(0);

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
            velocity = Vector3.zero;

            // Сброс фризов
            if (Freeze != null)
                Freeze.Value = false;

            // Границы
            minBounds = newMinBounds;
            maxBounds = newMaxBounds;
            
            // Позиция
            trRoot.position = ClampToBounds(new Vector3(
                startPosition.x,
                startPosition.y,
                startPosition.z
            ));
            

            // Зум (опционально)
            if (startZoom.HasValue)
            {
                var localPos = tr.localPosition;
                localPos.y = config.MinZoom;
                tr.localPosition = localPos;
                targetZoomDistance = tr.localPosition.y;
            }
        }


        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        public void UpdateM()
        {
            if (CanProcessInput())
            {
                HandleZoom();

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
            
            var f = false;
            if (OnFreeze != null)
                f = OnFreeze.Invoke();
            if (velocity == Vector3.zero && f)
                return false;
            
            if (!Input.GetMouseButtonDown(0) && (isDragging || velocity.sqrMagnitude > 0.001f)) 
                return true;
            
            // Проверяем, есть ли UI под курсором
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

            if (!isDragging)
                return;

            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            if (config.InvertDrag)
                mouseDelta = -mouseDelta;

            float zoomFactor = cam.orthographicSize / config.MaxZoom;

            Vector3 dragMove = new Vector3(
                -mouseDelta.x,
                0,
                -mouseDelta.y
            ) * config.DragSpeed * zoomFactor;

            // 🔹 Прямое движение камеры
            //tr.position = ClampToBounds(tr.position + dragMove);

            // 🔹 Сохраняем скорость для инерции
            // 🔹 КЛЮЧЕВОЕ МЕСТО — сглаживание
            velocity = Vector3.Lerp(
                velocity,
                dragMove,
                config.DragResponsiveness * Time.deltaTime
            );
        }

        /// <summary>
        /// Перспективный зум камеры
        /// </summary>
        private void HandleZoom()
        {
            float zoomInput = 0f;
            isPinching = false;

            // ===== PC: Mouse Wheel =====
#if UNITY_EDITOR || UNITY_STANDALONE
            zoomInput = Input.GetAxis("Mouse ScrollWheel") * config.ZoomSpeed;
#endif
            
            // ===== Mobile: Pinch =====
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

            // if (Mathf.Abs(zoomInput) > 0.01f)
            // {
            //     // Луч из центра экрана
            //     Ray ray = cam.ScreenPointToRay(
            //         new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)
            //     );
            //
            //     hit = Physics.RaycastAll(ray, 1000f, 1 << 15)?[0];
            //
            //     if (hit.Value.point != null)
            //     {
            //         currDistance = Vector3.Distance(tr.position, hit.Value.point);
            //
            //         targetZoomDistance = Mathf.Clamp(
            //             currDistance - zoomInput,
            //             config.MinZoom,
            //             config.MaxZoom
            //         );
            //     }
            // }
            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                targetZoomDistance = Mathf.Clamp(
                    targetZoomDistance - zoomInput,
                    config.MinZoom,
                    config.MaxZoom
                );
            }

            // Плавное движение к целевому зуму
            var localPos = tr.localPosition;
            localPos.y = Mathf.SmoothDamp(
                localPos.y,
                targetZoomDistance,
                ref zoomVelocity,
                config.ZoomSmoothTime
            );

            tr.localPosition = localPos;
        }

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

            if (config.UseCameraBounds)
            {
                newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
                newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);
                newPos.z = Mathf.Clamp(newPos.z, minBounds.z, maxBounds.z);
            }

            trRoot.position = newPos;
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
            trRoot.position = new Vector3(position.x, position.y, position.z);
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
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t);

                Vector3 lerped = Vector3.Lerp(startPosition, targetPosition, t);
                trRoot.position = ClampToBounds(lerped);

                yield return null;
            }

            trRoot.position = ClampToBounds(targetPosition);
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
