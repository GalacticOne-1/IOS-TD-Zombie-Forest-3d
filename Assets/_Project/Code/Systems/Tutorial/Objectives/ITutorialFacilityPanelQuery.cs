using Galactic1.Code.Systems.Runtime.Building;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public interface ITutorialFacilityPanelQuery
    {
        bool IsFacilityPanelOpen(FacilityType type);
    }
}