namespace Galactic1.Code.Gameplay.Units
{
    // ─────────────────────────────────────────────
    //  DyingState
    //
    //  Что делает:
    //    - Вызывает Entity_Die() — там уже WeaponAnimSwitcher.Die(),
    //      UnitMover.Die(), анимация смерти
    //    - Ждёт окончания анимации через таймер или AnimationEvent
    //    - Уведомляет SquadController через UnitEventBus
    //    - Переходит в Dead
    //
    //  Переходы ИЗ Dying:
    //    → Dead : анимация завершена (таймер или AE_DeathComplete)
    //
    //  Примечание: DyingState — форсированный, войти можно из любого состояния.
    //  Выйти из него нельзя ничем кроме перехода в Dead.
    // ─────────────────────────────────────────────
    public sealed class DyingState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.Dying;
 
        private const float FallbackDeathTimer = 3.5f;
        private float _timer;
        private bool  _deathTriggered;

        public void OnEnter(UnitInstance unit, IUnitCommand triggerCommand)
        {
            _timer = FallbackDeathTimer;
            _deathTriggered = false;

            // ReloadHandler опционален (у зомби его нет)
            unit.ReloadHandler?.Interrupt();

            unit.Entity_Die();

            EventBus<UnitKilledEvent>.Raise(new UnitKilledEvent(unit as SurvivorInstance));
        }

        public void OnExit(UnitInstance unit) { }
 
        public void Tick(UnitInstance unit, float dt)
        {
            if (_deathTriggered) return;
            _timer -= dt;
            if (_timer <= 0f) CompleteDeath(unit);
        }
 
        public void CompleteDeath(UnitInstance unit)
        {
            if (_deathTriggered) return;
            _deathTriggered = true;
            unit.StateMachine.ForceState(UnitStateId.Dead);
        }
 
        public bool HandleCommand(UnitInstance unit, IUnitCommand command) => true;
 
        public void ForceTransition(UnitInstance unit, UnitStateId targetState)
        {
            if (targetState == UnitStateId.Dead)
                unit.StateMachine.ForceState(UnitStateId.Dead);
        }
    }
}