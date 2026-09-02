namespace Galactic1.Code.Gameplay.Targeting
{
    /// <summary>
    /// Runtime → Scene bridge
    /// </summary>
    public sealed class CombatTargetingAdapter : ICombatTargetingService
    {
        private readonly CombatTargetingService _service;

        public CombatTargetingAdapter(CombatTargetingService service)
        {
            _service = service;
        }

        public void StartTargeting(TargetingRequest request)
        {
            _service.StartTargeting(
                request.User,
                request.UseModule,
                pos => request.OnConfirm?.Invoke(pos),
                request.OnCancel
            );
        }

        public void Cancel()
        {
            _service.StopTargeting();
        }
    }
}