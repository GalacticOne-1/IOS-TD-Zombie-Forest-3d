using System;
using Galactic1.Code.WorldMap;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Configs.WorldMap
{
    /// <summary>
    /// Конфиг стилей маркеров локаций на глобальной карте.
    /// Определяет иконку и цвет для каждого типа локации.
    /// Используется LocationMarker (UI only).
    /// </summary>
    [CreateAssetMenu(
        fileName = "WorldMapStyleConfig",
        menuName = "Game Configs/Style/World Map Style Config"
    )]
    public class WorldMapStyleConfig : StyleConfigBase
    {

        [SerializeField] 
        private Color[] difficultyColor;
        
        [SerializeField]
        private MarkerStyleEntry[] styles;

        /// <summary>
        /// Возвращает стиль для указанного типа локации.
        /// Если стиль не найден — возвращает null.
        /// </summary>
        public (Sprite, Color) GetStyle(LocationType type, int difficulty)
        {
            difficulty--;
            
            // определяем какой стиль выдать
            (LocationType, Color color) style = type switch
            {
                LocationType.Scrap or LocationType.Components or LocationType.Food =>
                    (LocationType.Scrap, difficultyColor[difficulty]),
                
                LocationType.MilitaryBase or LocationType.Hospital or LocationType.Laboratory =>
                    (LocationType.MilitaryBase, difficultyColor[difficulty]),
                
                LocationType.Bunker => (LocationType.Bunker, difficultyColor[difficulty]),
                
                _ => (type, Color.white)
            };


            for (int i = 0; i < styles.Length; i++)
            {
                if (styles[i].locationType == type)
                    return (styles[i].icon, style.color);
            }

            return (styles[1].icon, style.color); // стиль ресурсой локации
        }
    }

    /// <summary>
    /// Описание визуального стиля маркера для конкретного типа локации.
    /// </summary>
    [Serializable]
    public struct MarkerStyleEntry
    {
        public LocationType locationType;
        public Sprite icon;
    }
}