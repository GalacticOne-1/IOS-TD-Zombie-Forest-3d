
namespace Galactic1.Code.Gameplay.Construction.States
{
    /// <summary>
    /// Ничего не выбрано
    /// </summary>
    public class IdleState : ConstructionStateBase
    {
        public IdleState(
            ConstructionModeController controller,
            ConstructionPlacementController placement)
            : base(controller, placement)
        {
        }

        public override void OnObjectClicked(BuildableObject obj)
        {
            controller.Context.SelectedObject = obj;
            controller.SetState(ConstructionStateType.SelectedObject);
        }
    }
}