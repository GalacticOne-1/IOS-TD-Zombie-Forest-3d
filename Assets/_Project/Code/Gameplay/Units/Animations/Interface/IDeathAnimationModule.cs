namespace Galactic1.Code.Gameplay.Animation.Zombie
{
    public interface IDeathAnimationModule
    {
        void Initialize(BaseAnimConfig config);
        void PlayDeath();
    }
}