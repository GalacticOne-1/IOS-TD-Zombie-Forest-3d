using Galactic1.Code.Cameras;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Стрелка tutorial. Поддерживает два типа таргета:
    ///  - UI target (UIAnchor != null): позиционируется рядом с RectTransform, как раньше;
    ///  - World target (WorldAnchor != null): каждый кадр проецируется через игровую камеру
    ///    в координаты arrowLayer. Если таргет вне safe-области — стрелка прижимается к её границе
    ///    и смотрит в сторону таргета.
    ///
    /// Стрелка ожидает, что её родитель (arrowLayer) — RectTransform внутри Canvas.
    /// Вся world-to-screen логика живёт здесь, TutorialHUDController только создаёт виджет.
    /// </summary>
    // Позже камеры (Cinemachine Brain и т.п. обновляются в LateUpdate), чтобы не было лага в 1 кадр.
    [DefaultExecutionOrder(1000)]
    public sealed class TutorialArrowWidget : MonoBehaviour
    {
        private const float DirectionEpsilon = 1e-4f;

        [SerializeField] private RectTransform selfRect;

        [Tooltip("Смещение стрелки от таргета (для UI — как раньше; для world — в единицах arrowLayer, " +
                 "с учётом Canvas Scaler). Стрелка указывает от этой точки на таргет.")]
        [SerializeField]
        private Vector2 offsetFromTarget = new(0, 60f);
        

        [Tooltip("Отступ safe-области от краёв arrowLayer (единицы arrowLayer). Только для world-таргетов.")]
        [SerializeField]
        private float edgePadding = 80f;
        [Tooltip("Минимальное расстояние стрелки от World Target в единицах arrowLayer.")]
        [SerializeField]
        private float targetOffset = 80f;

        [Tooltip("Куда смотрит спрайт стрелки в prefab без вращения, в градусах против часовой " +
                 "от оси +X: вправо = 0, вверх = 90, влево = 180, вниз = -90.")]
        [SerializeField]
        private float spriteForwardAngle = -90f;

        private ITutorialTarget _target;
        private RectTransform _uiAnchor;
        private Transform _worldAnchor;
        private Behaviour _worldAnchorOwner;

        private RectTransform _layer;
        private Camera _uiCamera; // null для Screen Space - Overlay
        private Camera _worldCamera; // gameplay-камера, кэшируется

        private Vector3 _baseScale = Vector3.one;
        private bool _initialized;

        private void Awake() => EnsureInitialized();

        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            if (selfRect == null)
                selfRect = (RectTransform)transform;

            _baseScale = selfRect.localScale;
            _initialized = true;
        }

        public void PointTo(ITutorialTarget target)
        {
            EnsureInitialized();

            _target = target;
            _uiAnchor = target?.UIAnchor;
            _worldAnchor = _uiAnchor == null ? target?.WorldAnchor : null;
            _worldAnchorOwner = _worldAnchor != null ? target as Behaviour : null;

            _layer = selfRect.parent as RectTransform;
            _uiCamera = null;
            _worldCamera = null;

            if (_layer != null)
            {
                var canvas = _layer.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    var root = canvas.rootCanvas;
                    _uiCamera = root.renderMode == RenderMode.ScreenSpaceOverlay
                        ? null
                        : root.worldCamera;
                }
            }

            Refresh();
        }

        private void LateUpdate()
        {
            if (_target != null)
                Refresh();
        }

        private void Refresh()
        {
            // Unity-объекты сравниваются через перегруженный ==: уничтоженный anchor даёт true для "== null".
            if (_uiAnchor != null)
            {
                SetVisible(true);
                selfRect.position = _uiAnchor.position + (Vector3)offsetFromTarget;
                return;
            }

            if (_worldAnchor != null && _layer != null)
            {
                UpdateWorld();
                return;
            }

            SetVisible(false);
        }

        private void UpdateWorld()
        {
            // Таргет отключён (WorldTutorialTargetBehaviour снят с регистрации в OnDisable) — прячем.
            if (_worldAnchorOwner != null && !_worldAnchorOwner.isActiveAndEnabled)
            {
                SetVisible(false);
                return;
            }

            var cam = GetWorldCamera();
            if (cam == null)
            {
                SetVisible(false);
                return;
            }

            Vector3 screen = cam.WorldToScreenPoint(_worldAnchor.position);
            if (!IsFinite(screen))
            {
                SetVisible(false);
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _layer,
                    new Vector2(screen.x, screen.y),
                    _uiCamera,
                    out Vector2 targetLocal))
            {
                SetVisible(false);
                return;
            }

            // У ортографической камеры x/y экранной точки корректны при любом z.
            // У перспективной при z <= 0 проекция зеркальна — направление нужно инвертировать.
            bool behindCamera = !cam.orthographic && screen.z <= 0f;

            // Safe-область: rect слоя, уменьшенный на edgePadding (в локальных координатах слоя).
            Rect rect = _layer.rect;
            Vector2 center = rect.center;
            float halfW = Mathf.Max(0f, rect.width * 0.5f - edgePadding);
            float halfH = Mathf.Max(0f, rect.height * 0.5f - edgePadding);

            Vector2 delta = targetLocal - center;

            Vector2 position;
            Vector2 direction;

            if (!behindCamera)
            {
                // Направление от target к центру safe-area.
                Vector2 fromTargetToCenter = center - targetLocal;

                if (fromTargetToCenter.sqrMagnitude < DirectionEpsilon * DirectionEpsilon)
                    fromTargetToCenter = Vector2.up;

                Vector2 radialDirection = fromTargetToCenter.normalized;

                // Желаемая позиция стрелки:
                // она ВСЕГДА старается находиться targetOffset от target.
                Vector2 desiredPosition = targetLocal + radialDirection * targetOffset;

                // Проверяем, помещается ли стрелка с таким offset
                // внутри safe-area.
                bool desiredPositionInsideSafeArea =
                    desiredPosition.x >= center.x - halfW &&
                    desiredPosition.x <= center.x + halfW &&
                    desiredPosition.y >= center.y - halfH &&
                    desiredPosition.y <= center.y + halfH;

                if (desiredPositionInsideSafeArea)
                {
                    // Target достаточно близко к экрану:
                    // стрелка движется вокруг target по радиусу.
                    position = desiredPosition;
                }
                else
                {
                    // Target ещё слишком далеко:
                    // стрелка остаётся на границе safe-area.
                    direction = delta;

                    if (direction.sqrMagnitude < DirectionEpsilon * DirectionEpsilon)
                        direction = Vector2.up;

                    direction.Normalize();

                    position = center + direction * DistanceToEdge(direction, halfW, halfH);
                }

                // В обоих случаях стрелка смотрит непосредственно на target.
                direction = targetLocal - position;
            }
            else
            {
                // Target находится за камерой.
                // Используем существующую edge-логику.
                direction = -delta;

                if (direction.sqrMagnitude < DirectionEpsilon * DirectionEpsilon)
                    direction = Vector2.up;

                direction.Normalize();

                position = center + direction * DistanceToEdge(direction, halfW, halfH);
            }

            SetVisible(true);

            selfRect.localPosition = new Vector3(position.x, position.y, selfRect.localPosition.z);

            if (direction.sqrMagnitude >= DirectionEpsilon * DirectionEpsilon)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - spriteForwardAngle;
                selfRect.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
            // иначе стрелка лежит поверх таргета — оставляем прошлое вращение.
        }

        /// <summary>Расстояние от центра до границы прямоугольника (±halfW, ±halfH) вдоль
        /// нормализованного direction. Нормализация гарантирует, что хотя бы одна компонента ≥ ~0.7.</summary>
        private static float DistanceToEdge(Vector2 direction, float halfW, float halfH)
        {
            float ax = Mathf.Abs(direction.x);
            float ay = Mathf.Abs(direction.y);

            float tx = ax > DirectionEpsilon ? halfW / ax : float.MaxValue;
            float ty = ay > DirectionEpsilon ? halfH / ay : float.MaxValue;

            return Mathf.Min(tx, ty);
        }

        private Camera GetWorldCamera()
        {
            if (_worldCamera != null)
                return _worldCamera;

            var cameraController = ServiceLocator.Current.Get<CameraController>();
            _worldCamera = cameraController != null ? cameraController.Camera : null;
            return _worldCamera;
        }

        private void SetVisible(bool visible)
        {
            Vector3 scale = visible ? _baseScale : Vector3.zero;
            if (selfRect.localScale != scale)
                selfRect.localScale = scale;
        }

        private static bool IsFinite(Vector3 v) =>
            !(float.IsNaN(v.x) || float.IsInfinity(v.x) ||
              float.IsNaN(v.y) || float.IsInfinity(v.y) ||
              float.IsNaN(v.z) || float.IsInfinity(v.z));
    }
}