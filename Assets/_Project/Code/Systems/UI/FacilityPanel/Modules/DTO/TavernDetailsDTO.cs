using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime.Building;

namespace Galactic1.Game.UI.Buildings.DTO
{
    /// <summary>
    /// Детали таверны (найм).
    /// </summary>
    public sealed class TavernDetailsDTO  : IFacilityDetailsDTO
    {
        public FacilityType Type => FacilityType.Tavern;
        
        public IReadOnlyList<TavernOfferDTO> Offers { get; }
        public int CampCapacity { get; }
        public int UsedSlots { get; }
        
        public int DaysUntilRefresh { get; }

        public TavernDetailsDTO(
            int campCapacity,
            int usedSlots,
            int daysUntilRefresh,
            IReadOnlyList<TavernOfferDTO> offers)
        {
            CampCapacity = campCapacity;
            UsedSlots = usedSlots;
            DaysUntilRefresh = daysUntilRefresh;
            Offers = offers;
        }
    }
}