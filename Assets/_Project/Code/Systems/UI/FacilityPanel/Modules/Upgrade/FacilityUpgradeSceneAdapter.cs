using System;
using System.Linq;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Game.Runtime.Production;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.Game.UI.Production.DTO;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// SceneAdapter апгрейда здания.
    /// Проверяет ресурсы и выполняет upgrade runtime.
    /// </summary>
    public sealed class FacilityUpgradeSceneAdapter : IFacilitySceneAdapter
    {
        public IFacilityRuntime _runtime { get; }
        private readonly IInventoryResourcesPort _inventory;
        private readonly ResourcesRequirementService _requirements;
        
        
        public FacilityType Type { get; }
        public event Action OnStateChanged
        {
            add => _runtime.OnStateChanged += value;
            remove => _runtime.OnStateChanged -= value;
        }
        
        

        public FacilityUpgradeSceneAdapter(
            IFacilityRuntime runtime,
            IInventoryResourcesPort inventory)
        {
            _runtime = runtime;
            _inventory = inventory;
            _requirements = new ResourcesRequirementService(_inventory);
        }

        public FacilityUpgradeDetailsDTO GetUpgradeDetails()
        {
            var upgrade = _runtime.GetUpgrade(_runtime.Level + 1);

            if (upgrade == null)
                return null;

            var requirements = upgrade.Requirements
                .Select(r =>
                {
                    int owned = _inventory.GetTotalAmount(r.Item.Id);

                    return new RecipeRequirementDto(
                        r.Item.Id,
                        r.Item,
                        r.Item.Header.icon,
                        r.Amount,
                        owned,
                        owned >= r.Amount);
                })
                .ToList();

            bool canUpgrade = _requirements.HasResources(upgrade.Requirements);

            return new FacilityUpgradeDetailsDTO(
                _runtime.Config.Item.Header.icon,
                _runtime.Level,
                _runtime.Level + 1,
                requirements,
                canUpgrade,
                _runtime.Config.FacilityType == FacilityType.Production);
        }

        public bool TryUpgrade()
        {
            var upgrade = _runtime.GetUpgrade(_runtime.Level + 1);

            if (upgrade == null)
                return false;

            if (!_requirements.HasResources(upgrade.Requirements))
                return false;

            foreach (var r in upgrade.Requirements)
                _inventory.TrySpend(r.Item.Id, r.Amount);

            _runtime.Upgrade();

            return true;
        }

        
    }
}