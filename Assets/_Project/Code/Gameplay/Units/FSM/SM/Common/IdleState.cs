
namespace Galactic1.Code.Gameplay.Units
{
    // ─────────────────────────────────────────────
    //  IdleState
    //
    //  Что делает:
    //    - Останавливает движение
    //    - Разблокирует ReactiveAI (команда выполнена)
    //    - В Tick() — ничего, ReactiveAI сам решит что делать
    //
    //  Переходы ИЗ Idle:
    //    → Moving    : MoveCommand от игрока или ReactiveAI
    //    → Engaging  : AttackCommand
    //    → Reloading : ReloadCommand
    //    → Dying     : ForceState (hp = 0)
    //    → Panicking : ForceState (stress ≥ 80)
    // ─────────────────────────────────────────────

    public sealed class IdleState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.Idle;
 
        public void OnEnter(UnitInstance unit, IUnitCommand triggerCommand)
        {
            unit.Mover.Stop();
        }
 
        public void OnExit(UnitInstance unit) { }
 
        public void Tick(UnitInstance unit, float dt) { }
 
        public bool HandleCommand(UnitInstance unit, IUnitCommand command) => false;
 
        public void ForceTransition(UnitInstance unit, UnitStateId targetState)
            => unit.StateMachine.ForceState(targetState);
    }
    
    // public sealed class IdleState : IUnitState
    // {
    //     public UnitStateId StateId => UnitStateId.Idle;
    //
    //     public void OnEnter(SurvivorInstance unit, IUnitCommand triggerCommand)
    //     {
    //         // Остановить движение
    //         unit.Mover.Stop();
    //
    //         // Анимация простоя
    //         //unit.AnimationController.SetMoving(false);
    //
    //         // Разблокировать реактивный AI — команда выполнена
    //         unit.ReactiveAI.OnCommandCompleted();
    //     }
    //
    //     public void OnExit(SurvivorInstance unit) { }
    //
    //     public void Tick(SurvivorInstance unit, float dt)
    //     {
    //         // Idle ничего не делает сам.
    //         // ReactiveAI.Tick() (вызывается в SurvivorInstance.Tick)
    //         // сам решит атаковать, укрыться или перезарядиться.
    //     }
    //
    //     public bool HandleCommand(SurvivorInstance unit, IUnitCommand command)
    //     {
    //         // Idle принимает любую команду — пусть StateMachine переключает
    //         return false;
    //     }
    //
    //     public void ForceTransition(SurvivorInstance unit, UnitStateId targetState)
    //     {
    //         unit.StateMachine.ForceState(targetState);
    //     }
    // }


}