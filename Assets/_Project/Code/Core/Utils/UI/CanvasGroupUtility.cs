using UnityEngine;

namespace Galactic1.Code.Utils
{
    /// <summary>
    /// Утилиты для управления CanvasGroup.
    /// </summary>
    public static class CanvasGroupUtility
    {
        public const float DisabledAlpha = 0.5f;
        public const float EnabledAlpha = 1f;

        /// <summary>
        /// Делает UI неактивным:
        /// alpha = 0.5
        /// interactable = false
        /// blocksRaycasts = false
        /// </summary>
        public static void Disable(CanvasGroup group, bool block = true)
        {
            if (group == null)
                return;

            group.alpha = DisabledAlpha;
            group.interactable = !block;
            group.blocksRaycasts = !block;
        }

        /// <summary>
        /// Восстанавливает обычное состояние UI:
        /// alpha = 1
        /// interactable = true
        /// blocksRaycasts = true
        /// </summary>
        public static void Enable(CanvasGroup group)
        {
            if (group == null)
                return;

            group.alpha = EnabledAlpha;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }
}