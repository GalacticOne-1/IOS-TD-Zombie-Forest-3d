using Galactic1.Code.Systems.Raid;
using Galactic1.Gameplay.Locations.Definitions;

namespace Galactic1.Gameplay.Locations.Events
{
    /// <summary>
    /// Событие входа отряда в Exit Zone.
    /// Несёт полный набор данных для завершения рейда —
    /// подписчику (ExitZoneManager) не нужно ничего резолвить.
    /// </summary>
    public readonly struct ExitZoneTriggerEvent : IEvent
    {
        public readonly ExitId ExitId;
        public readonly RaidStatus ResultStatus;
        public readonly RaidEndReason ResultReason;

        public ExitZoneTriggerEvent(ExitId exitId, RaidStatus resultStatus, RaidEndReason resultReason)
        {
            ExitId = exitId;
            ResultStatus = resultStatus;
            ResultReason = resultReason;
        }
    }
}