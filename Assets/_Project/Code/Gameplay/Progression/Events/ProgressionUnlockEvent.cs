using Galactic1.Code.GameDatabase.Registries;

/// <summary>
/// Raised by UnlockService the moment a piece of content becomes unlocked.
/// </summary>
public sealed class ProgressionUnlockEvent : IEvent
{
    public readonly UnlockId UnlockId;

    public ProgressionUnlockEvent(UnlockId unlockId)
    {
        UnlockId = unlockId;
    }
}