using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.UI.Production;
using Galactic1.Game.UI.Production.DTO;

namespace Galactic1.Game.UI.Buildings.DTO
{
    /// <summary>
    /// Детали производственной станции.
    /// </summary>
    public sealed class ProductionFacilityDetailsDTO : IFacilityDetailsDTO
    {
        public FacilityType Type { get; }
        public int TotalRemainingTime { get; }
        public IReadOnlyList<ProductionJobDTO> Queue { get; }
        public List<RecipeCardData> Recipes { get; }
        public RecipeDetailsDto RecipeDetails { get; }

        public bool HasCompleted { get; }
        
        public bool HasActiveProduction { get; }
        public int SkipCost { get; }
        public bool CanSkip { get; }


        public ProductionFacilityDetailsDTO(
            FacilityType type,
            IReadOnlyList<ProductionJobDTO> queue,
            List<RecipeCardData> recipes,
            RecipeDetailsDto recipeDetails,
            bool hasCompleted, 
            int totalRemainingTime, 
            bool hasActiveProduction,
            int skipCost, 
            bool canSkip)
        {
            Type = type;
            Queue = queue;
            Recipes = recipes;
            RecipeDetails = recipeDetails;
            HasCompleted = hasCompleted;
            TotalRemainingTime = totalRemainingTime;
            HasActiveProduction = hasActiveProduction;
            SkipCost = skipCost;
            CanSkip = canSkip;
            
        }

       
    }
}