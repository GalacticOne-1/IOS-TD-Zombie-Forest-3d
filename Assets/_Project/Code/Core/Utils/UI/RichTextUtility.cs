
using UnityEngine;

namespace Galactic1.UI.Text
{
    public static class RichTextUtility
    {
        public static string Color(string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }

        public static string Size(string text, int size)
        {
            return $"<size={size}>{text}</size>";
        }

        public static string Bold(string text)
        {
            return $"<b>{text}</b>";
        }

        public static string Combine(params string[] parts)
        {
            return string.Concat(parts);
        }
    }
}