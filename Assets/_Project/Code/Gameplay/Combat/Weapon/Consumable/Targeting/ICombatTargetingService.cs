namespace Galactic1.Code.Gameplay.Targeting
{
    /// <summary>
    /// Сервис, который умеет запускать режим таргетинга (гранаты и т.п.)
    /// </summary>
    public interface ICombatTargetingService
    {
        void StartTargeting(TargetingRequest request);
        void Cancel();
    }
}