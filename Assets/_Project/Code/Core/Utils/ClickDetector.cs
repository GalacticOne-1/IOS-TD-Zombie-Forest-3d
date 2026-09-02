using UnityEngine;

namespace Galactic1.Code.Gameplay
{
    /// <summary>
    /// Production detector для single / double click.
    /// Использует deferred resolution (как в RTS).
    /// </summary>
    public class ClickDetector
    {
        private float clickTime;
        private Vector2 clickPosition;

        private bool waitingSecondClick;

        private readonly float doubleClickWindow;
        private readonly float maxDistance;

        public ClickDetector(float doubleClickWindow = 0.3f, float maxDistance = 25f)
        {
            this.doubleClickWindow = doubleClickWindow;
            this.maxDistance = maxDistance;
        }

        public ClickType RegisterClick(Vector2 position)
        {
            float now = Time.time;

            if (waitingSecondClick)
            {
                bool withinTime = now - clickTime <= doubleClickWindow;
                bool withinDistance = Vector2.Distance(position, clickPosition) <= maxDistance;

                waitingSecondClick = false;

                if (withinTime && withinDistance)
                {
                    return ClickType.Double;
                }
            }

            clickTime = now;
            clickPosition = position;
            waitingSecondClick = true;

            return ClickType.Pending;
        }

        public ClickType Update()
        {
            if (!waitingSecondClick)
                return ClickType.None;

            if (Time.time - clickTime > doubleClickWindow)
            {
                waitingSecondClick = false;
                return ClickType.Single;
            }

            return ClickType.None;
        }
    }

    public enum ClickType
    {
        None,
        Pending,
        Single,
        Double
    }
}