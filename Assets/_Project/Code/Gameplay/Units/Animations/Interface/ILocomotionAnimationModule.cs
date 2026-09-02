using Galactic1.Code.Gameplay.Units.Definitions;

namespace Galactic1.Code.Gameplay.Animation.Zombie
{
    public interface ILocomotionAnimationModule
    {
        void Initialize(BaseAnimConfig config, UnitGameplayDefinition definition);
        void Tick();
    }
}