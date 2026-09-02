
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Runtime representation of a constructed building.
    /// </summary>
    public interface IFacilityRuntimeService : IGameService
    {
        BaseCampFacilityRuntime CreateBuildingCompletely(
            FacilityModule facilityItem,
            BuildingFootprintRuntime footprint);

        void DeleteBuildingCompletely(string buildingId);

    }
}