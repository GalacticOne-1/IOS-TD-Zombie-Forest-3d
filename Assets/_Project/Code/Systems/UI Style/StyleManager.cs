using UnityEngine;
using System.Collections.Generic;
using Galactic1.Configs;

namespace Galactic1
{
    public static class StyleManager
    {
        private static DamagePopupStyleConfig _damagePopupStyleConfig;
        private static readonly Dictionary<string, DamagePopupStyleConfig.CStyle> damagePopupStyleMap = new();
        
        
        
        /// <summary>
        /// Инициализация менеджера при старте игры.
        /// </summary>
        public static void Initialize()
        {
            _damagePopupStyleConfig = ServiceLocator.Current.Get<ConfigProvider>().Get<DamagePopupStyleConfig>();
            
            damagePopupStyleMap.Clear();

            foreach (var style in _damagePopupStyleConfig.styles)
            {
                if (!string.IsNullOrEmpty(style.id))
                    damagePopupStyleMap[style.id.ToLower()] = style;
            }

            Debug.Log($"🎨 StyleManager: Loaded {damagePopupStyleMap.Count} popup styles.");
        }

        
        /// <summary>
        /// Получить стиль по id (например: "critical", "poison", "normal").
        /// </summary>
        public static DamagePopupStyleConfig.CStyle GetPopupStyle(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "normal";

            if (damagePopupStyleMap.TryGetValue(id.ToLower(), out var style))
                return style;

            Debug.LogWarning($"⚠ StyleManager: popup style '{id}' not found!");
            return _damagePopupStyleConfig.styles.Length > 0 ? _damagePopupStyleConfig.styles[0] : null;
        }
    }

}