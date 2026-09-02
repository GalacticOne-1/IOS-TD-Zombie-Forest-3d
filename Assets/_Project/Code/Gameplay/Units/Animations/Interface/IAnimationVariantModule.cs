namespace Galactic1.Code.Gameplay.Animation.Variants
{
    /// <summary>
    /// Предоставляет случайные варианты анимационных состояний.
    /// Используется для разнообразия idle/walk/attack и т.д.
    /// </summary>
    public interface IAnimationVariantModule
    {
        int GetVariant(AnimationVariantType type);
    }
}