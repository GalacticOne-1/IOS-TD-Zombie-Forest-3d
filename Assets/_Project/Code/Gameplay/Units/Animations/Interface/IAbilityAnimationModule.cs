using Galactic1.Code.Gameplay.Effect;

namespace Galactic1.Code.Gameplay.Animation.Player
{
    public interface IAbilityAnimationModule
    {
        void Initialize(BaseAnimConfig config);
        void OnAbilityAnimation(ItemUseContext ctx);
        void EndGrenadeToss();
    }
}