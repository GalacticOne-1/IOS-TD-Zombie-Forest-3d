using System;
using System.Collections.Generic;
using Galactic1.Core.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.UI.Core
{
    /// <summary>
    /// UI-стиль для отображения редкости предметов.
    /// Используется UI-слоем (tooltip, inventory, raid report и т.д.).
    /// </summary>
    [CreateAssetMenu(
        fileName = "ItemRarityStyleConfig",
        menuName = "Game Configs/Style/Item Rarity Style"
    )]
    public class ItemRarityStyleConfig : StyleConfigBase
    {
        [SerializeField]
        private List<RarityStyleEntry> styles = new();

        [Header("Fallback")]
        [SerializeField]
        private Color fallbackOutlineColor = Color.white;

        /// <summary>
        /// Возвращает стиль для указанной редкости.
        /// Безопасен к отсутствующим записям.
        /// </summary>
        public RarityStyleEntry GetColor(ItemRarity rarity)
        {
            for (int i = 0; i < styles.Count; i++)
            {
                if (styles[i].Rarity == rarity)
                    return styles[i];
            }

            // default
            return new RarityStyleEntry
            {
                Rarity = rarity,
                Material = null,
                OutlineColor = fallbackOutlineColor,
                TitleColor = Color.white,
                BackgroundSprite = null
            };
        }
    }

    /// <summary>
    /// Описание визуального стиля для одной редкости.
    /// </summary>
    [Serializable]
    public struct RarityStyleEntry
    {
        public ItemRarity Rarity;
        
        [FormerlySerializedAs("ItemMaterial")] [Header("Materials")]
        public Material Material;

        [Header("Colors")]
        public Color OutlineColor;
        public Color TitleColor;

        [Header("Optional UI")]
        public Sprite BackgroundSprite;
    }
}