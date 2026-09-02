
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Gameplay.Locations.Events;

namespace Galactic1.Code.Systems.GameLoop.Tactical
{
    /// <summary>
    /// Реагирует на вход отряда в Exit Zone.
    /// Применяет результат к текущему RaidRuntime и переключает
    /// TacticalStateMachine — по тому же контракту, что и debug-кнопки в HUD.
    /// Не знает ни о сценах, ни о WorldMap: это уже задача PostRaidReportState.
    /// </summary>
    public sealed class ExitZoneManager
    {
        private readonly GameLoopContext _context;
        private readonly MissionObjectiveService _missionService;
        private readonly EventBinding<ExitZoneTriggerEvent> _binding;

        public ExitZoneManager(GameLoopContext context, MissionObjectiveService missionService)
        {
            _context = context;
            _missionService = missionService;
            _binding = new EventBinding<ExitZoneTriggerEvent>(OnExitTriggered);
            EventBus<ExitZoneTriggerEvent>.Register(_binding);
        }

        // *** может вызываться несколько раз сразу, если юниты близко входили в коллайдер
        // это вроде решило баг => _context.TacticalStateMachine == null
        private void OnExitTriggered(ExitZoneTriggerEvent evt)
        {
            var raid = _context.CurrentRaid;
            if (raid == null || _context.TacticalStateMachine == null)
                return;

            DLog.Alert($"[ExitZoneManager] {evt.ExitId} triggered: {evt.ResultStatus}/{evt.ResultReason}",
                EDlogColor.YELLOW, AppConstants.show_log_core);
            
            EventBus<ExitReachedEvent>.Raise(new ExitReachedEvent());

            // raid.Status = evt.ResultStatus;
            // _missionService.ForceFinished(new()
            // {
            //     Status = MissionStatus.Victory,
            //     EndReason = evt.ResultReason
            // },
            // TacticalTransitionResolver.GetNext(_context.TacticalStateMachine.Current));
        }

        public void Dispose()
        {
            ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(() =>
                EventBus<ExitZoneTriggerEvent>.Deregister(_binding));
        }
    }
}