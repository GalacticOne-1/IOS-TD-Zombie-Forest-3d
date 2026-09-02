namespace Galactic1.Code.Gameplay.Damage
{
    public interface IDamageStep
    {
        /// <summary>
        /// Обработка урона.
        /// Вернуть false → пайплайн останавливается
        /// </summary>
        bool Process(DamageContext context);
    }
}