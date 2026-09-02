using UnityEngine;

namespace Galactic1.Code.UI.Formatters
{
    /// <summary>
    /// Утилита форматирования времени в днях для UI World Map.
    /// Правила:
    /// - меньше 1 дня → 1 знак после запятой
    /// - целое число дней → без дробной части
    /// - иначе → 1 знак после запятой
    /// </summary>
    public static class DayTimeFormatter
    {
        public static string Format(float days)
        {
            if (days < 1f)
                return days.ToString("0");

            // если дробная часть практически нулевая
            if (Mathf.Approximately(days, Mathf.Round(days)))
                return Mathf.Round(days).ToString("0");

            return days.ToString("0.0");
        }
    }
}