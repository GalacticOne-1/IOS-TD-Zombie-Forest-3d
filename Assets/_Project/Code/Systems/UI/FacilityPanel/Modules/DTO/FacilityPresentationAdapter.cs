
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.UI.Buildings.DTO;

namespace Galactic1.Game.UI.Buildings
{
    public sealed class FacilityPresentationAdapter
    {
        //private readonly BuildingDependencyService _dependencyService;
        private readonly FacilityDetailsFactory _detailsFactory;


        public FacilityPresentationAdapter(
            object dependencyService, 
            FacilityDetailsFactory detailsFactory)
        {
            //_dependencyService = dependencyService;
            _detailsFactory = detailsFactory;
        }


        public FacilityDTO Create(BaseCampFacilityRuntime runtime)
        {
            return new FacilityDTO(
                runtime.Id,
                runtime.ConfigId,
                runtime.Config.Item.Header.titleLid,
                runtime.Config.Item.Header.descriptionLid,
                runtime.Config.Item.Header.icon,
                runtime.Level,
                3,
                runtime.CanUpgrade,
                null,
                null,
                _detailsFactory.Create(runtime)
            );
        }
        
    }
}