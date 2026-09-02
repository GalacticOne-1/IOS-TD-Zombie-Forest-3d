using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Utils
{
    /// <summary>
    /// Утилиты для работы с shader properties.
    /// </summary>
    public static class ShaderPropertyUtil
    {
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");

        public static void SetFlashAmount(Graphic graphic, float value)
        {
            if (graphic == null || graphic.material == null)
                return;

            graphic.material.SetFloat(FlashAmountId, value);
        }

        public static void SetFlashColor(Graphic graphic, Color color)
        {
            if (graphic == null || graphic.material == null)
                return;

            graphic.material.SetColor(FlashColorId, color);
        }

        public static void SetFlash(Graphic graphic, Color color, float amount)
        {
            if (graphic == null || graphic.material == null)
                return;

            graphic.material.SetColor(FlashColorId, color);
            graphic.material.SetFloat(FlashAmountId, amount);
        }
    }
}