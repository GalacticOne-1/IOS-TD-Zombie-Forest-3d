using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    public static class DebugScene
    {
        public static void CreateSphere(
            Vector3 position,
            Color color,
            float radius = 1,
            bool isDestroy = true,
            float destroyingDelay = 1,
            bool drawWireSphere = false)
        {
            var g = new GameObject();
            var dc = g.AddComponent<DC>();
            dc.Center = position;
            dc.Radius = radius;
            dc.Color = color;
            
            dc.IsDestroy = isDestroy;
            dc.DestroyingDelay = destroyingDelay;
            dc.DrawWireSphere = drawWireSphere;
        }
    }
}