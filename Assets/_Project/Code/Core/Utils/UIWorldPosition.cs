using UnityEngine;

namespace Galactic1.Utility
{
    public static class UIWorldPosition
    {
        public static Vector3 WorldToCanvas(
            Vector3 worldPos,
            Camera cam,
            RectTransform canvas)
        {
            Vector3 screen = cam.WorldToScreenPoint(worldPos);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas,
                screen,
                cam,
                out Vector2 pos);

            return pos;
        }
    }
}