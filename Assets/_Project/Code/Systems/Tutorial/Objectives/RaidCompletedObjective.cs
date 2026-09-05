using Galactic1.Code.Systems.Raid.Mission;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class RaidCompletedObjective : TutorialEventObjectiveBase<MissionCompletedEvent>
    {
        private readonly bool _requireVictory;
        public RaidCompletedObjective(bool requireVictory) => _requireVictory = requireVictory;

        protected override bool EvaluateEvent(MissionCompletedEvent e)
            => !_requireVictory || e.Result.Status == MissionStatus.Victory;
    }
}
