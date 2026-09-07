using Galactic1.Code.GameDatabase.Registries;

/// <summary>
/// Gameplay fact: a world map location was discovered for the first time.
/// Raised by MapNode.SetDiscovered on the false→true transition.
/// Deliberately does NOT carry an XP amount — see ProgressionXPService.
/// </summary>
public sealed class LocationDiscoveredEvent : IEvent
{
    public readonly LocationId LocationId;

    public LocationDiscoveredEvent(LocationId locationId)
    {
        LocationId = locationId;
    }
}