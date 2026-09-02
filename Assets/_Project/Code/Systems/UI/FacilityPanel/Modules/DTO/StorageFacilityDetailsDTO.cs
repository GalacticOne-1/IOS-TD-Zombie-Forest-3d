using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Game.UI.Buildings.DTO
{
    public sealed class StorageFacilityDetailsDTO : IFacilityDetailsDTO
    {
        public FacilityType Type { get; }
        public StorageType StorageType { get; }
        public int Capacity { get; }
        public string SpecialDescription { get; }

        public StorageFacilityDetailsDTO(
            FacilityType type, 
            StorageType storageType,
            int capacity,
            string specialDescription)
        {
            Type = type;
            StorageType = storageType;
            Capacity = capacity;
            SpecialDescription = specialDescription;
        }
    }
}