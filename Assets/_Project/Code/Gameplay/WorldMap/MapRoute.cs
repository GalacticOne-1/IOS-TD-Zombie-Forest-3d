using UnityEngine;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Связь между двумя узлами карты
    /// </summary>
    [System.Serializable]
    public class MapRoute
    {
        public MapNode From;
        public MapNode To;

        /// <summary>
        /// Стоимость пути в днях (не дискретное время!)
        /// </summary>
        public float PathCostDays = 1f;
    }
}