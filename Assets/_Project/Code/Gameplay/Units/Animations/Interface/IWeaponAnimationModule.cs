namespace Galactic1.Code.Gameplay.Animation
{
    /// <summary>
    /// Опциональные weapon animation features.
    /// </summary>
    public interface IWeaponAnimationModule
    {
        void SetWeaponVisible(bool visible);
        void SetRigEnabled(bool enabled);
    }
}