using UnityEngine;

namespace Galactic1.Utils
{
    /// <summary>
    /// Утилита округления до ближайшего чётного числа.
    /// Не editor-only - можно использовать и в рантайме, и в редакторских скриптах.
    /// </summary>
    public static class EvenSnapUtility
    {
        /// <summary>
        /// Округляет значение до ближайшего целого, а если оно оказалось нечётным -
        /// сдвигает к ближайшему чётному соседу (в ту сторону, что ближе к исходному значению).
        /// Примеры: 1.2 -> 2, 3.0 -> 2 (при равном расстоянии берётся меньшее), 22.8 -> 22.
        /// </summary>
        public static float RoundToNearestEven(float value)
        {
            return ToNearestEven(Mathf.RoundToInt(value));
        }
        
        /// <summary>
        /// Cдвигает к ближайшему чётному соседу
        /// Примеры: 3.0 -> 2 
        /// </summary>
        public static float ToNearestEven(float value)
        {
            if (value % 2 == 0) return value;

            float lower = value - 1;
            float upper = value + 1;
            return Mathf.Abs(value - lower) <= Mathf.Abs(value - upper) ? lower : upper;
        }

        /// <summary>Округляет обе компоненты Vector2 до ближайшего чётного числа.</summary>
        public static Vector2 RoundToNearestEven(Vector2 value)
        {
            return new Vector2(RoundToNearestEven(value.x), RoundToNearestEven(value.y));
        }

        /// <summary>Округляет X и Y компоненты Vector3 до ближайшего чётного числа, Z не трогает.</summary>
        public static Vector3 RoundToNearestEvenXY(Vector3 value)
        {
            return new Vector3(RoundToNearestEven(value.x), RoundToNearestEven(value.y), value.z);
        }

        /// <summary>Округляет X и Z компоненты Vector3 до ближайшего чётного числа, Y не трогает.</summary>
        public static Vector3 RoundToNearestEvenXZ(Vector3 value)
        {
            return new Vector3(RoundToNearestEven(value.x), value.y, RoundToNearestEven(value.z));
        }

        /// <summary>Округляет все три компоненты Vector3 до ближайшего чётного числа.</summary>
        public static Vector3 RoundToNearestEven(Vector3 value)
        {
            return new Vector3(RoundToNearestEven(value.x), RoundToNearestEven(value.y), RoundToNearestEven(value.z));
        }
    }
}