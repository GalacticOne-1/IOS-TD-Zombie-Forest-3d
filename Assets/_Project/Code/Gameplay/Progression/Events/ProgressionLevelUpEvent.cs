
/// <summary>
/// Raised by ProgressionService whenever the accumulated XP crosses one or
/// more level thresholds. Carries enough info for listeners to know exactly
/// what changed without querying ProgressionService back.
/// </summary>
public sealed class ProgressionLevelUpEvent : IEvent
{
    public readonly int PreviousLevel;
    public readonly int NewLevel;

    public ProgressionLevelUpEvent(int previousLevel, int newLevel)
    {
        PreviousLevel = previousLevel;
        NewLevel = newLevel;
    }
}
