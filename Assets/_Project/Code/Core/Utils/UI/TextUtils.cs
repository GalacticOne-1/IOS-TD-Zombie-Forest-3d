using UnityEngine;

namespace Galactic1.Code.Utils
{
    /// <summary>
    /// Утилиты для работы с TMP текстом и RichText.
    /// </summary>
    public static class TextUtils
    {
        /// <summary>
        /// Возвращает строку с окрашенным текстом.
        /// </summary>
        /// <param name="text">Текст, который нужно окрасить</param>
        /// <param name="color">Цвет (можно Color или HEX #RRGGBB)</param>
        /// <returns>Строка с тегами RichText для TMP</returns>
        public static string ColorText(string text, Color color)
        {
            string hex = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hex}>{text}</color>";
        }

        /// <summary>
        /// Перегрузка для передачи цвета через HEX строку (#RRGGBB)
        /// </summary>
        public static string ColorText(string text, string hexColor)
        {
            return $"<color={hexColor}>{text}</color>";
        }

        /// <summary>
        /// Форматирует owned/required и окрашивает owned в зависимости от достаточности.
        /// </summary>
        /// <param name="owned">Сколько есть</param>
        /// <param name="required">Сколько нужно</param>
        /// <param name="enoughColor">Цвет при достаточности</param>
        /// <param name="notEnoughColor">Цвет при нехватке</param>
        /// <returns>Строка для TMP</returns>
        public static string FormatOwnedRequired(int owned, int required)
        {
            Color color = owned >= required ? Color.white : Color.red;
            return $"{ColorText(owned.ToString(), color)} / {required}";
        }
    }
}