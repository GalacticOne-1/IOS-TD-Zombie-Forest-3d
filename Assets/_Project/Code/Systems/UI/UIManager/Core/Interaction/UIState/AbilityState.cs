
namespace Galactic1.Code.UI.Interaction
{
    public sealed class AbilityState : IUIState
    {
        public int Priority => 900;

        public bool BlocksGameplayInput => true;
        public bool BlocksUIInput => true; // 🔥 блокируем всё кроме ability UI

        public void OnEnter() { }
        public void OnExit() { }

        public void Apply(IUILayerService layers, IUIInteractionLockService _)
        {
            layers.HideAllExcept(UILayerType.Targeting);
            
            var block = ServiceLocator.Current.Get<UIBlockService>();

            block.Clear();
            block.Block(UIBlockGroup.Global);
        }
    }
}