namespace Galactic1.Code.UI.Interaction
{
    public interface IUIBlockable
    {
        UIBlockGroup Group { get; }

        void SetBlocked(bool blocked);
    }
}