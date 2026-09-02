using System.Collections.Generic;
using Galactic1.Game.UI.Production.DTO;
using UnityEngine;

namespace Galactic1.Game.UI.Buildings.DTO
{
    /// <summary>
    /// DTO данных улучшения здания.
    /// Используется UI для отображения требований апгрейда.
    /// </summary>
    public sealed class FacilityUpgradeDetailsDTO
    {
        public readonly Sprite Icon;
        public readonly int CurrentLevel;
        public readonly int NextLevel;
        public readonly List<RecipeRequirementDto> Requirements;
        public readonly bool CanUpgrade;
        public readonly bool UsingRecipes;

        public FacilityUpgradeDetailsDTO(
            Sprite icon,
            int currentLevel,
            int nextLevel,
            List<RecipeRequirementDto> requirements,
            bool canUpgrade, 
            bool usingRecipes)
        {
            Icon = icon;
            CurrentLevel = currentLevel;
            NextLevel = nextLevel;
            Requirements = requirements;
            CanUpgrade = canUpgrade;
            UsingRecipes = usingRecipes;
        }
    }
}