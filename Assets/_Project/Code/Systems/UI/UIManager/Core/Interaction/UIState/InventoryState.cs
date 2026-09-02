
namespace Galactic1.Code.UI.Interaction
{
    public sealed class InventoryState : IUIState
    {
        public int Priority => 200;

        public bool BlocksGameplayInput => true;   // ❗ блокируем мир
        public bool BlocksUIInput => false;

        public void OnEnter() { }
        public void OnExit() { }

        public void Apply(IUILayerService layers, IUIInteractionLockService _)
        {
            layers.Show(UILayerType.Inventory);
            layers.Show(UILayerType.HUD);
        }
    }
}