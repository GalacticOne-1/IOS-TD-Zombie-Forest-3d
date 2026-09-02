using UnityEngine;

namespace Galactic1.Code.Utility
{
    /// <summary>
    /// Production pointer tracker.
    ///
    /// Унифицированный источник pointer input:
    /// - Mouse
    /// - Touch
    ///
    /// Предоставляет:
    /// - позицию
    /// - delta
    /// - tap
    /// - drag
    /// - ray для 3D
    ///
    /// Используется в gameplay и UI системах.
    /// </summary>
    public static class PointerTracker
    {
        // =========================
        // STATE
        // =========================

        public static Vector2 Position { get; private set; }
        public static Vector2 Delta { get; private set; }

        public static bool Pressed { get; private set; }
        public static bool PressedDown { get; private set; }
        public static bool PressedUp { get; private set; }

        public static bool IsDragging { get; private set; }

        private static Vector2 _pressStartPosition;
        private static Vector2 _previousPosition;

        private const float DragThreshold = 8f;
        
        public static bool WasDragging { get; private set; }
        

        // =========================
        // UPDATE
        // =========================

        /// <summary>
        /// Вызывать один раз за кадр.
        /// </summary>
        public static void Update()
        {
            ResetFrameState();

            Vector2 newPosition;
            bool down = false;
            bool up = false;
            bool hold = false;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL

            newPosition = Input.mousePosition;

            down = Input.GetMouseButtonDown(0);
            up = Input.GetMouseButtonUp(0);
            hold = Input.GetMouseButton(0);

#else

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);

                newPosition = touch.position;

                down = touch.phase == TouchPhase.Began;
                up =
                    touch.phase == TouchPhase.Ended ||
                    touch.phase == TouchPhase.Canceled;

                hold =
                    touch.phase == TouchPhase.Moved ||
                    touch.phase == TouchPhase.Stationary ||
                    touch.phase == TouchPhase.Began;
            }
            else
            {
                newPosition = Position;
            }

#endif

            Position = newPosition;

            // delta
            Delta = Position - _previousPosition;

            // pressed state
            if (down)
            {
                Pressed = true;
                PressedDown = true;
                WasDragging = false;

                _pressStartPosition = Position;
            }

            if (up)
            {
                Pressed = false;
                PressedUp = true;
                IsDragging = false;
            }

            // drag detection
            if (Pressed)
            {
                float dist = (Position - _pressStartPosition).sqrMagnitude;

                IsDragging = dist > DragThreshold * DragThreshold;
                
                if (IsDragging)
                    WasDragging = true; 
            }

            _previousPosition = Position;
        }

        // =========================
        // RAYCAST
        // =========================

        public static Ray GetRay(Camera camera)
        {
            return camera.ScreenPointToRay(Position);
        }

        // =========================
        // UTILITIES
        // =========================

        public static bool IsTap =>
            PressedUp && !IsDragging;

        private static void ResetFrameState()
        {
            PressedDown = false;
            PressedUp = false;
        }
    }
}