using UnityEngine;

namespace Galactic1.EditorTools.PrefabScreenshot
{
    /// <summary>
    /// Единая математика положения камеры.
    /// Используется и в превью окна, и в финальном рендере,
    /// чтобы они гарантированно совпадали 1:1.
    /// Все смещения нормализованы относительно размера bounds объекта,
    /// поэтому камера ведёт себя одинаково независимо от масштаба префаба.
    /// </summary>
    public static class CameraRigMath
    {
        public struct CameraTransform
        {
            public Vector3 Position;
            public Vector3 LookAtPoint;
            public float Roll;
            public float Fov;
        }

        public static CameraTransform Calculate(Bounds bounds, ScreenshotSettings settings)
        {
            bounds.Expand(bounds.size * (settings.padding - 1f));

            float radius = bounds.extents.magnitude;

            // Нормализованный target offset переводим в мировые единицы
            // через extents объекта (компонентно), а не через фиксированное число.
            Vector3 targetOffsetWorld = Vector3.Scale(bounds.extents, settings.targetOffsetNormalized);

            Vector3 center = bounds.center + targetOffsetWorld;

            float rad = settings.cameraYaw * Mathf.Deg2Rad;
            float pit = settings.cameraPitch * Mathf.Deg2Rad;

            Vector3 dir = new Vector3(
                Mathf.Cos(pit) * Mathf.Sin(rad),
                Mathf.Sin(pit),
                Mathf.Cos(pit) * Mathf.Cos(rad)
            );

            float distance = radius * settings.cameraDistanceMultiplier;

            // Нормализованный position offset тоже переводим через radius,
            // чтобы он масштабировался вместе с объектом.
            Vector3 positionOffsetWorld = settings.positionOffsetNormalized * radius;

            Vector3 position = center + dir.normalized * distance + positionOffsetWorld;

            return new CameraTransform
            {
                Position = position,
                LookAtPoint = center,
                Roll = settings.cameraRoll,
                Fov = settings.fieldOfView
            };
        }

        public static void Apply(Camera camera, Bounds bounds, ScreenshotSettings settings)
        {
            var t = Calculate(bounds, settings);

            camera.transform.position = t.Position;
            camera.transform.LookAt(t.LookAtPoint, Vector3.up);
            camera.transform.Rotate(Vector3.forward, t.Roll, Space.Self);

            camera.fieldOfView = t.Fov;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 500f;
        }
    }
}