namespace Galactic1.Code.UI.Interaction
{
    public interface IUIInteractionLockService
    {
        bool IsUIBlocked { get; }
        bool IsGameplayBlocked { get; }

        void LockUI();
        void UnlockUI();

        void LockGameplay();
        void UnlockGameplay();
    }
}