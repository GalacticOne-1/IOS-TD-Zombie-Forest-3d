using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Tutorial.Presentation;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public interface ITutorialInboxSlotTargetProvider
    {
        bool TryGetInboxSlotTarget(ItemId itemId, out ITutorialTarget target);
    }
}