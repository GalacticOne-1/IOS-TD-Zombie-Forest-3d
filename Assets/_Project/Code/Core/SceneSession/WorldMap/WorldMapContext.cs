using Galactic1.Code.WorldMap;
using UnityEngine;

namespace Galactic1.Core.Systems.GameSession.WorldMap
{
    /// <summary>
    /// КОНТЕКСТ МИРОВОЙ КАРТЫ
    /// Хранит состояние стратегического режима
    /// </summary>
    public class WorldMapContext
    {
        // визуальный корень карты
        public GameObject MapRoot;

        // данные карты
        public WorldMapConfig MapConfig;

        public WorldMapService MapService;
        public WorldMapController MapController;

        public int CurrentDay;
        public int DaysUntilHorde;
    }
}