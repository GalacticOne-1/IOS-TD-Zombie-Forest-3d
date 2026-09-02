using System.Collections.Generic;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Economy;

namespace Galactic1.Code.Inventory.Services
{
    /// <summary>
    /// Доменный сервис проверки ресурсных требований для одного источника
    /// Может использоваться любыми системами.
    /// </summary>
    public sealed class ResourcesRequirementService 
    {
        private readonly IInventoryResourcesPort _inventory;

        public ResourcesRequirementService(IInventoryResourcesPort inventory)
        {
            _inventory = inventory;
        }

        /// <summary>
        /// Проверяет, хватает ли ресурсов для набора требований.
        /// </summary>
        public bool HasResources(IEnumerable<RequirementData> requirements)
        {
            foreach (var require in requirements)
            {
                int total = _inventory.GetTotalAmount(require.Item.Id);
                if (total < require.Amount)
                    return false;
            }

            return true;
        }
    }
}