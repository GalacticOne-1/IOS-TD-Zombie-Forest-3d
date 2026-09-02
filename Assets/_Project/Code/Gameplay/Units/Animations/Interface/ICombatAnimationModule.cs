namespace Galactic1.Code.Gameplay.Animation.Player
{
    public interface ICombatAnimationModule
    {
        void Initialize(BaseAnimConfig config);
        void PlayShoot();
        void SetFiring(bool isFiring);
        void PlayReload();
        void CancelReload();
        void PlayInteract();
    }
}