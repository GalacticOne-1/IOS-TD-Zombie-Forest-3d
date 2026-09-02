namespace Galactic1.Code.Systems.Daily
{
    /// <summary>
    /// Доменное правило суточного сброса.
    /// Описывает ЧТО нужно сбросить, но не КОГДА.
    /// </summary>
    public interface IDailyResetRule
    {
        void ExecuteReset();
    }
}