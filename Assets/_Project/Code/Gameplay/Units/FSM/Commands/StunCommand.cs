namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Команда оглушения — направляет FSM в SuppressedState.
    ///
    /// Принимается из TemporalAoEZone при попадании в зону шумовой гранаты.
    /// Является форсированным прерыванием: CanExecute возвращает true всегда
    /// кроме Dying/Dead (обрабатывается в UnitStateMachine.Execute через ForceState).
    /// </summary>
    public sealed class StunCommand : IUnitCommand
    {
        /// <summary>Длительность стана в секундах.</summary>
        public readonly float Duration;

        public StunCommand(float duration)
        {
            Duration = duration;
        }

        // FSM направит в SuppressedState
        public UnitStateId TargetState => UnitStateId.Suppressed;

        // Принимается из любого живого состояния
        public bool CanExecute(UnitStateId s) =>
            s != UnitStateId.Dying &&
            s != UnitStateId.Dead;
    }
}
