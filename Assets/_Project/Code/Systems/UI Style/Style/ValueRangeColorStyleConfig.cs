using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.UI.Core
{
    /// <summary>
    /// Универсальный конфиг для цветового отображения значений,
    /// зависящих от диапазонов (прочность, здоровье, угроза и т.д.).
    ///
    /// Используется UI для визуализации "состояния", а не конкретных чисел.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ValueRangeColorStyleConfig",
        menuName = "Game Configs/Style/Value Range Color Style"
    )]
    public class ValueRangeColorStyleConfig : StyleConfigBase
    {
        
        
        [Serializable]
        public struct ColorRange
        {
            [Tooltip("Минимальное значение (включительно)")]
            [Range(0f, 1f)]
            public float min;

            [Tooltip("Максимальное значение (включительно)")]
            [Range(0f, 1f)]
            public float max;

            public Color color;
        }

        [Serializable]
        public class ValueStyle
        {
            public ValueRangeType type;
            public List<ColorRange> ranges;
        }

        [Header("Styles")]
        [SerializeField]
        private List<ValueStyle> styles = new();

        private Dictionary<ValueRangeType, List<ColorRange>> cache;

        private void OnEnable()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            cache = new Dictionary<ValueRangeType, List<ColorRange>>();

            for (int i = 0; i < styles.Count; i++)
            {
                if (styles[i] == null)
                    continue;

                cache[styles[i].type] = styles[i].ranges;
            }
        }

        /// <summary>
        /// Возвращает цвет для заданного типа шкалы и значения.
        /// </summary>
        public Color GetColor(ValueRangeType type, float value01)
        {
            if (cache == null || !cache.ContainsKey(type))
                return Color.white;
            
            value01 = Mathf.Clamp01(value01);

            var ranges = cache[type];
            var l = ranges.Count;

            for (int i = 0; i < l; i++)
            {
                if (value01 >= ranges[i].min && value01 <= ranges[i].max)
                    return ranges[i].color;
            }

            return Color.white;
        }
    }

    /// <summary>
    /// Типы шкал, использующих диапазонную цветовую визуализацию.
    /// </summary>
    public enum ValueRangeType
    {
        Durability = 0,
        Health = 1,
        ThreatLevel = 2,
        Radiation = 3,
        Noise = 4,
        
        
        MissionStatus = 20,
    }
}
