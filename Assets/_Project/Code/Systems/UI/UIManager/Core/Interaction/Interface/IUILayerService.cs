namespace Galactic1.Code.UI.Interaction
{
    public interface IUILayerService
    {
        void Show(UILayerType layer);
        void Hide(UILayerType layer);
        void HideAllExcept(params UILayerType[] layers);
    }
}