using Galactic1.RaidLoot.Events;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class LootCollectedObjective : TutorialEventObjectiveBase<ContainerLootCollectedEvent>
    {
        protected override bool EvaluateEvent(ContainerLootCollectedEvent e) => true;
    }
}
