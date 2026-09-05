namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// State-семантика: "в стратегическом отряде >= N юнитов". Требует
    /// StrategicSquadChangedEvent — интеграционная точка (одна строка в
    /// GameLoopContext.SelectForStrategicSquad/DeselectFromStrategicSquad),
    /// см. Integration/PENDING_GAMEPLAY_EVENTS.md.
    /// </summary>
    public sealed class SquadSizeObjective : TutorialStateRecheckObjectiveBase<StrategicSquadChangedEvent>
    {
        private readonly ITutorialSquadQuery _squad;
        private readonly int _requiredSize;

        public SquadSizeObjective(ITutorialSquadQuery squad, int requiredSize)
        {
            _squad = squad;
            _requiredSize = requiredSize;
        }

        public override bool EvaluateCurrentState() => _squad.GetStrategicSquadSize() >= _requiredSize;
    }
}
