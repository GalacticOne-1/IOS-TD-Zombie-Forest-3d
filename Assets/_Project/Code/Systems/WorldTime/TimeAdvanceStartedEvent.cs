namespace Galactic1.Code.Systems.GameTime
{
    /// <summary>
    /// Событие начала продвижения времени
    /// Используется для инициализации отчёта
    /// </summary>
    public readonly struct TimeAdvanceStartedEvent
    {
        public readonly int StartDay;
        public readonly int DaysPlanned;
        public readonly TimeAdvanceReason Reason;

        public TimeAdvanceStartedEvent(int startDay, int daysPlanned, TimeAdvanceReason reason)
        {
            StartDay = startDay;
            DaysPlanned = daysPlanned;
            Reason = reason;
        }
    }
}