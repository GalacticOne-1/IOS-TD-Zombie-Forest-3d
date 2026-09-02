
namespace Galactic1.Code.UI.Interaction
{
    public sealed class HUDState : IUIState
    {
        public int Priority => 0;

        public bool BlocksGameplayInput => false;
        public bool BlocksUIInput => false;

        public void OnEnter() { }
        public void OnExit() { }

        public void Apply(IUILayerService layers, IUIInteractionLockService _)
        {
            layers.Show(UILayerType.HUD);
            layers.Hide(UILayerType.Inventory);
            layers.Hide(UILayerType.Targeting);
            
            var block = ServiceLocator.Current.Get<UIBlockService>();
            block.Clear(); 
        }
    }
}