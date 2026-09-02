using System.Collections.Generic;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime.Building;

namespace Galactic1.Code.Systems.Camp
{
    /// <summary>
    /// После загрузки лагеря удаляет разрушенные здания.
    /// </summary>
    public sealed class CampFacilityPostResolveService
    {
        private readonly GameLoopContext _context;
        private readonly IFacilityRuntimeService _facilityService;

        public CampFacilityPostResolveService(
            GameLoopContext context,
            IFacilityRuntimeService facilityService)
        {
            _context = context;
            _facilityService = facilityService;
        }

        public void Execute()
        {
            var toDelete = new List<string>();

            foreach (var runtime in _context.Facilities)
            {
                if (runtime is not CombatFacilityRuntime combat)
                    continue;

                if (!combat.IsDestroyed 
                    || combat is CampHQFacilityRuntime) // *** главное здание не удаляется
                    continue;

                toDelete.Add(combat.Id);
            }

            foreach (var id in toDelete)
            {
                _facilityService.DeleteBuildingCompletely(id);
            }
        }
    }
}