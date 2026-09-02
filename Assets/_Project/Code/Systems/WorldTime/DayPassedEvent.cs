namespace Galactic1.Code.Systems.GameTime
{
    /// <summary>
    /// Событие, вызываемое при переходе на новый день
    /// </summary>
    public readonly struct DayPassedEvent
    {
        public readonly int Day;
        public readonly TimeAdvanceReason Reason;

        public DayPassedEvent(int day, TimeAdvanceReason reason)
        {
            Day = day;
            Reason = reason;
        }

        
    }
}