using Galactic1.Code.GameDatabase.Registries;

/// <summary>
/// Gameplay fact: a camp facility finished construction.
/// Raised by FacilityRuntimeService.CreateBuildingCompletely.
/// </summary>
public sealed class FacilityBuiltEvent : IEvent
{
    public readonly ItemId FacilityItemId;

    public FacilityBuiltEvent(ItemId facilityItemId)
    {
        FacilityItemId = facilityItemId;
    }
}