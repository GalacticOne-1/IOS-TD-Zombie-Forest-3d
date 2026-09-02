using UnityEngine;

namespace Galactic1.Code.Utility
{
    /// <summary>
    /// High level gesture recognition.
    /// </summary>
    public static class GestureSystem
    {
        public static bool Tap { get; private set; }
        public static bool Drag { get; private set; }
        public static bool LongPress { get; private set; }

        public static float PinchDelta { get; private set; }

        private static Vector2 _pressStart;
        private static float _pressTime;

        private const float DragThreshold = 10f;
        private const float LongPressTime = 0.45f;

        public static void Update()
        {
            Tap = false;
            LongPress = false;

            if (PointerTracker.PressedDown)
            {
                _pressStart = PointerTracker.Position;
                _pressTime = Time.time;
            }

            if (PointerTracker.Pressed)
            {
                float dist =
                    (PointerTracker.Position - _pressStart).sqrMagnitude;

                Drag = dist > DragThreshold * DragThreshold;

                if (!Drag && Time.time - _pressTime > LongPressTime)
                {
                    LongPress = true;
                }
            }

            if (PointerTracker.PressedUp)
            {
                if (!Drag)
                    Tap = true;

                Drag = false;
            }

            UpdatePinch();
        }

        private static void UpdatePinch()
        {
            PinchDelta = 0;

            if (Input.touchCount < 2)
                return;

            var t0 = Input.GetTouch(0);
            var t1 = Input.GetTouch(1);

            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float prevDist = (prev0 - prev1).magnitude;
            float currDist = (t0.position - t1.position).magnitude;

            PinchDelta = currDist - prevDist;
        }
    }
}