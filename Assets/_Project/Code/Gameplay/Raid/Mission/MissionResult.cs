namespace Galactic1.Code.Systems.Raid.Mission
{
    public struct MissionResult
    {
        public MissionStatus Status;

        /// <summary>
        /// Причина завершения рейда (успех, эвакуация, wipe, panic).
        /// </summary>
        public RaidEndReason EndReason;
        
        

        public bool IsFinished =>
            Status != MissionStatus.Running;

        public static MissionResult Running =>
            new MissionResult { Status = MissionStatus.Running };

        public static MissionResult Victory =>
            new MissionResult { Status = MissionStatus.Victory };

        public static MissionResult Defeat =>
            new MissionResult { Status = MissionStatus.Defeat };
    }
}