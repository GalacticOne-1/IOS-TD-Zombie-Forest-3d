using UnityEngine;

namespace Galactic1.Code.Utility
{
    public static class TimeUtils
    {
        /// <summary>
        /// Преобразует часы в формат "Xd Yh" или "Yh"
        /// </summary>
        public static string FormatTime(float hours)
        {
            int totalHours = Mathf.RoundToInt(hours);

            int days = totalHours / 24;
            int remainingHours = totalHours % 24;

            if (days > 0 && remainingHours > 0)
                return $"{days}d {remainingHours}h";

            if (days > 0)
                return $"{days}d";

            return $"{remainingHours}h";
        }
    }
}