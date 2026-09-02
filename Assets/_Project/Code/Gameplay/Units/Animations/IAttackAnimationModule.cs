namespace Galactic1.Code.Gameplay.Animation
{
    /// <summary>
    /// Handles attack animation presentation.
    /// </summary>
    public interface IAttackAnimationModule
    {
        void Initialize(BaseAnimConfig config);

        void PlayAttack();

        void PlayMeleeAttack();

        void PlayRangedAttack();

        void ResetState();
    }
}