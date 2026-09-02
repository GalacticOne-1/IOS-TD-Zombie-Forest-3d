namespace Galactic1.Code.Systems.GameTime
{
    /// <summary>
    /// Причина продвижения времени
    /// </summary>
    public enum TimeAdvanceReason
    {
        CampDefense,
        Raid,
        ManualSkip,
        MapMovement,
        ScriptedEvent
    }
}