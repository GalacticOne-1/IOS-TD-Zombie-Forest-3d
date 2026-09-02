using System.Collections.Generic;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Economy;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Construction.Repair
{
    /// <summary>
    /// Строит полное состояние требований ремонта.
    ///
    /// Переиспользует ConstructionRequirementService для проверки/владения
    /// ресурсами — не дублирует логику инвентаря.
    /// </summary>
    public class RepairRequirementService
    {
        private readonly ConstructionRequirementService _constructionRequirementService;
        private readonly IRepairRoundingStrategy _roundingStrategy;

        public RepairRequirementService(
            ConstructionRequirementService constructionRequirementService,
            IRepairRoundingStrategy roundingStrategy)
        {
            _constructionRequirementService = constructionRequirementService;
            _roundingStrategy = roundingStrategy;
        }

        public bool NeedsRepair(IRepairableFacility facility)
            => facility != null && !facility.IsDestroyed && facility.CurrentHP < facility.MaxHP;

        public List<RequirementData> GetRepairCost(IRepairableFacility facility, FacilityModule module)
        {
            if (!NeedsRepair(facility) || module == null)
                return new List<RequirementData>();

            var buildCost = _constructionRequirementService.GetBuildCost(module);
            return RepairCostCalculator.Calculate(
                buildCost,
                facility.CurrentHP,
                facility.MaxHP,
                _roundingStrategy);
        }

        public RepairRequirementResult GetRequirementResult(IRepairableFacility facility, FacilityModule module)
        {
            if (facility == null)
                return RepairRequirementResult.NotRepairable;

            bool needsRepair = NeedsRepair(facility);
            var cost = needsRepair
                ? GetRepairCost(facility, module)
                : new List<RequirementData>();

            var entries = new List<RepairRequirementEntry>(cost.Count);

            foreach (var requirement in cost)
            {
                entries.Add(new RepairRequirementEntry
                {
                    Item = requirement.Item,
                    Required = requirement.Amount,
                    Owned = _constructionRequirementService.GetOwnedAmount(requirement)
                });
            }

            return new RepairRequirementResult
            {
                IsRepairable = true,
                NeedsRepair = needsRepair,
                HasEnoughResources = needsRepair && _constructionRequirementService.HasResources(cost),
                Entries = entries
            };
        }
    }
}