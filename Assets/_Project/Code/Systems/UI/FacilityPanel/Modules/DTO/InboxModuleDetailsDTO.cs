using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime.Building;

namespace Galactic1.Game.UI.Buildings.DTO
{
    public class InboxModuleDetailsDTO : IFacilityDetailsDTO
    {
        public FacilityType Type => FacilityType.MainContainer;
        
        public IReadOnlyList<InboxItemDTO> Slots { get; }

        public InboxModuleDetailsDTO(IReadOnlyList<InboxItemDTO> slots)
        {
            Slots = slots;
        }
    }
}