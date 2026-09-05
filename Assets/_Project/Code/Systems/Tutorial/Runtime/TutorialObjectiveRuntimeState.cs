namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>Транзиентное (не персистентное) состояние одного активного объектива.</summary>
    public sealed class TutorialObjectiveRuntimeState
    {
        public readonly ITutorialObjective Objective;
        public readonly string ObjectiveTypeId;

        public TutorialObjectiveRuntimeState(ITutorialObjective objective, string objectiveTypeId)
        {
            Objective = objective;
            ObjectiveTypeId = objectiveTypeId;
        }

        public bool IsCompleted => Objective.IsCompleted;
    }
}
