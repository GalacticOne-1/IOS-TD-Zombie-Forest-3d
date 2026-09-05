using Galactic1.RaidLoot.Events;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class ContainerOpenedObjective : TutorialEventObjectiveBase<ContainerOpenedEvent>
    {
        protected override bool EvaluateEvent(ContainerOpenedEvent e) => true;
    }
}
