using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.UI.Tooltips;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Stats
{
    /// <summary>
    /// Применяет стиль и форматирование к UI элементу стата.
    /// </summary>
    public static class StatUIBuilder
    {
        public static void Apply(
            StatStyleEntry style,
            TMP_Text label,
            Image icon,
            TMP_Text valueText,
            float value,
            string valueStr = "")
        {
            if (style == null)
                return;

            // Label
            if (label != null)
            {
                label.text = style.localizationKey; // позже заменить на LocalizationService

                if (style.appendColon)
                    label.text += ":";
            }
            
            if(icon != null)
                icon.sprite = style.icon;

            // Formatting
            if(value > 0)
            {
                string formattedValue = FormatValue(style, value);

                valueText.text = $"{style.front}{formattedValue}{style.suffix}";
                valueText.color = style.valueColor;
            }
            else if (!string.IsNullOrEmpty(valueStr))
            {
                valueText.text = valueStr;
                valueText.color = style.valueColor;
            }

            if (style.font != null)
                valueText.font = style.font;
        }

        
        public static (string label, string value) Apply(
            StatStyleEntry style,
            float value1,
            float value2)
        {
            if (style == null)
                return ("---", "---");

            // Label
            var label = style.localizationKey; // позже заменить на LocalizationService

            if (style.appendColon)
                label += ":";

            string formattedValue = style.currentMaxField
                ? $"{value2}/{value1}"
                : FormatValue(style, value1);
                

            var v = $"{style.front}{formattedValue}{style.suffix}";


            return (label, v);
        }
        
        

        public static void Apply(
            StatStyleEntry style,
            TMP_Text label,
            Transform listRoot,
            List<RuntimeId> idList = null)
        {
            if (style == null)
                return;

            // Label
            if (label != null)
            {
                label.text = style.localizationKey; // позже заменить на LocalizationService
                
                if (style.appendColon)
                    label.text += ":";
            }

            if (idList != null && idList.Count > 0)
            {
                var maxQu = listRoot.childCount;
                var targetQu = idList.Count;

                var delta = listRoot.parent.CMP_RectTr().sizeDelta;
                var y = 170; // первый ряд
                if (targetQu > 5)
                    y += (targetQu / 5) * 120;
                
                listRoot.parent.CMP_RectTr().sizeDelta = new Vector2(delta.x, y);
                
                for (int i = 0; i < maxQu; i++)
                {
                    var view = listRoot.GetChild(i).gameObject;
                    if (i >= targetQu)
                    {
                        view.SetActive(false);
                        continue;
                    }
                    
                    
                    if (GameContent.Items.TryGet(idList[i], out var config))
                    {
                        view.SetActive(true);
                        view.GetComponent<ItemFieldView>().Bind(config);
                    }
                }
            }
        }
        
        
        
        

        private static string FormatValue(StatStyleEntry style, float value)
        {
            float finalValue = style.roundToInt
                ? Mathf.Round(value)
                : (float)Math.Round(value, style.decimalPlaces, MidpointRounding.AwayFromZero);

            string text = finalValue.ToString();

            if (style.showPlusForPositive && finalValue > 0)
                text = $"+{text}";

            return text;
        }
    }
}