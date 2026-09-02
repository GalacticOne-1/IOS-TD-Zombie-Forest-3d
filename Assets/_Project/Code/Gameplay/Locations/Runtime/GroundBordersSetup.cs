using UnityEngine;

namespace Galactic1.Gameplay.Locations.Utils
{
    /// <summary>
    /// Установка границ локации (обёртка для LOCATION_SETUP из проекта).
    /// </summary>
    public class GroundBordersSetup
    {
        public void Apply(Vector2 borderX, Vector2 borderY)
        {
            new LOCATION_SETUP().SetGroundBorderX(borderX);
            new LOCATION_SETUP().SetGroundBorderY(borderY);
        }
    }
}