
namespace Galactic1.Code.Gameplay.Damage
{
    /// <summary>Любая цель, принимающая урон.</summary>
    public interface IDamageable
    {
        void ApplyDamage(float damage);
    }
}