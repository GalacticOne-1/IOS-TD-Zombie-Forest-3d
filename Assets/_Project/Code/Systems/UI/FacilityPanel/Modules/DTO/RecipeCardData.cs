using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Game.UI.Production
{
    /// <summary>
    /// DTO для построения карточки.
    /// UI не знает о runtime.
    /// </summary>
    public readonly struct RecipeCardData
    {
        public readonly RuntimeId RecipeId;
        public readonly Sprite Icon;
        public readonly ItemRarity Rarity;
        public readonly string Name;
        public readonly bool IsAvailable;

        public RecipeCardData(
            RuntimeId recipeId, 
            Sprite icon, 
            ItemRarity rarity,
            string name, 
            bool isAvailable)
        {
            RecipeId = recipeId;
            Icon = icon;
            Rarity = rarity;
            Name = name;
            IsAvailable = isAvailable;
        }

    }
}