namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class ExitReachedObjective : TutorialEventObjectiveBase<ExitReachedEvent>
    {
        protected override bool EvaluateEvent(ExitReachedEvent e) => true;
    }
}
