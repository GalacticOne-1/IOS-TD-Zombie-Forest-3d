using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.UI.Buildings.DTO;

namespace Galactic1.Game.UI.Buildings
{
    sealed class LivingModuleDetailsDTO : IFacilityDetailsDTO
    {
        public FacilityType Type => FacilityType.LivingModule;


        public LivingModuleDetailsDTO()
        {
            
        }
    }
}