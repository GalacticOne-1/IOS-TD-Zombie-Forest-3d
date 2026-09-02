namespace Galactic1.Code.Systems.GameTime
{
    /// <summary>
    /// Событие завершения продвижения времени
    /// Используется для показа итогового отчёта игроку
    /// </summary>
    public readonly struct TimeAdvanceFinishedEvent
    {
        public readonly int StartDay;
        public readonly int EndDay;
        public readonly int DaysPassed;
        public readonly TimeAdvanceReason Reason;

        public TimeAdvanceFinishedEvent(int startDay, int endDay, int daysPassed, TimeAdvanceReason reason)
        {
            StartDay = startDay;
            EndDay = endDay;
            DaysPassed = daysPassed;
            Reason = reason;
        }
    }
}