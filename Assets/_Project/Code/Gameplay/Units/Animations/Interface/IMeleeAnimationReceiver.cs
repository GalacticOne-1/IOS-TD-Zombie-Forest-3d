namespace Galactic1.Code.Gameplay.Animation
{
    public interface IMeleeAnimationReceiver
    {
        void OnAnimationMeleeHitEvent();
        
        void OnAnimationFinished();
    }
}