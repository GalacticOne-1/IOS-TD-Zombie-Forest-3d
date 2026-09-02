
using Galactic1.Code.Systems.Runtime.Building;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Meta-конфигурация footprint здания.
    ///
    /// Описывает:
    /// - размер здания на grid
    /// - разрешён ли поворот
    /// - правила размещения
    ///
    /// Используется:
    /// ConstructionValidator
    /// Ghost система
    /// GridService
    /// </summary>
    [System.Serializable]
    public class FacilityFootprintConfig
    {
        [Header("Grid Size")]
        [Tooltip("Ширина здания в клетках")]
        public int width = 1;

        [Tooltip("Высота здания в клетках")]
        public int height = 1;

        public bool autoBuild;

        
        [Header("Placement Rules")]
        [Tooltip("Разрешён ли поворот здания")]
        public bool allowRotation = true;

        [Tooltip("Можно ли ставить поверх других объектов")]
        public bool allowOverlap = false;

        [Tooltip("Нужно ли проверять занятость клеток")]
        public bool checkOccupancy = true;

        /// <summary>
        /// Возвращает footprint для grid системы.
        /// </summary>
        public BuildingFootprint ToFootprint()
        {
            return new BuildingFootprint(width, height);
        }
    }
}