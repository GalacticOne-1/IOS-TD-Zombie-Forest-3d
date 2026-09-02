using System.Collections.Generic;
using Galactic1.Game.UI.Production;
using Galactic1.Game.UI.Production.DTO;
using UnityEngine;

namespace Galactic1.Game.UI.Production.DTO
{
    /// <summary>
    /// DTO представление производственного здания.
    /// Передаётся в UI для отображения.
    /// </summary>
    public sealed class ProductionStationDTO
    {
        public Sprite StationIcon { get; }
        public int StationLevel { get; }
        public int TotalRemainingTime { get; }
        public IReadOnlyList<ProductionJobDTO> Queue { get; }
        public List<RecipeCardData> Recipes { get; }
        public RecipeDetailsDto RecipeDetails { get; }

        public bool HasCompleted { get; }
        
        public int SkipCost { get; }
        public bool CanSkip { get; }


        public ProductionStationDTO(
            IReadOnlyList<ProductionJobDTO> queue,
            List<RecipeCardData> recipes,
            RecipeDetailsDto recipeDetails,
            bool hasCompleted, 
            Sprite stationIcon,
            int stationLevel, 
            int totalRemainingTime, 
            int skipCost, 
            bool canSkip)
        {
            Queue = queue;
            Recipes = recipes;
            RecipeDetails = recipeDetails;
            HasCompleted = hasCompleted;
            StationIcon = stationIcon;
            StationLevel = stationLevel;
            TotalRemainingTime = totalRemainingTime;
            SkipCost = skipCost;
            CanSkip = canSkip;
        }
    }
}