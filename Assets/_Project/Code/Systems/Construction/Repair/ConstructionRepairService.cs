using Galactic1.Code.Inventory.Services;

namespace Galactic1.Code.Gameplay.Construction.Repair
{
    /// <summary>
    /// Единственная точка входа в repair-пайплайн для UI/State-слоя.
    ///
    /// Работает только с runtime-объектами (IRepairableFacility).
    /// Scene-объект (BuildableObject) HP не хранит — только пробрасывает
    /// доступ к своему runtime.
    /// </summary>
    public class ConstructionRepairService
    {
        private readonly ConstructionRequirementService _constructionRequirementService;
        private readonly RepairRequirementService _repairRequirementService;

        public ConstructionRepairService(
            ConstructionRequirementService constructionRequirementService,
            RepairRequirementService repairRequirementService)
        {
            _constructionRequirementService = constructionRequirementService;
            _repairRequirementService = repairRequirementService;
        }

        public RepairRequirementResult GetRepairState(BuildableObject obj)
        {
            var facility = ResolveFacility(obj);

            if (facility == null)
                return RepairRequirementResult.NotRepairable;

            return _repairRequirementService.GetRequirementResult(facility, obj.FacilityConfig);
        }

        public RepairResult TryRepair(BuildableObject obj)
        {
            var facility = ResolveFacility(obj);

            if (facility == null)
                return RepairResult.Fail(RepairFailReason.NotRepairable);

            if (!_repairRequirementService.NeedsRepair(facility))
                return RepairResult.Fail(RepairFailReason.AlreadyFull);

            var cost = _repairRequirementService.GetRepairCost(facility, obj.FacilityConfig);

            if (!_constructionRequirementService.HasResources(cost))
                return RepairResult.Fail(RepairFailReason.NotEnoughResources);

            if (!_constructionRequirementService.TrySpend(cost))
                return RepairResult.Fail(RepairFailReason.NotEnoughResources);

            facility.RestoreFullHP();

            return RepairResult.Ok();
        }

        private static IRepairableFacility ResolveFacility(BuildableObject obj)
        {
            if (obj == null || obj.Adapter == null)
                return null;

            return obj.Adapter.Runtime as IRepairableFacility;
        }
    }
}