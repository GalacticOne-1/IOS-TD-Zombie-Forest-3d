namespace Galactic1.Code.Gameplay.Animation
{
    public interface IAbilityAnimationReceiver
    {
        void ExecutePending();
        void OnAbilityFinished();
    }
}