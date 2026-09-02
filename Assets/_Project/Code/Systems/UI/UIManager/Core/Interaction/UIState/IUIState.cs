namespace Galactic1.Code.UI.Interaction
{
    /// <summary>
    /// UI состояние с приоритетом и правилами блокировки
    /// </summary>
    public interface IUIState
    {
        int Priority { get; }

        bool BlocksGameplayInput { get; }
        bool BlocksUIInput { get; }

        void OnEnter();
        void OnExit();

        void Apply(IUILayerService layers, IUIInteractionLockService lockService);
    }
}