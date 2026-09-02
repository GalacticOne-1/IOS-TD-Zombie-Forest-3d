namespace Galactic1.Code.Gameplay.Units
{
    // ─────────────────────────────────────────────
    //  Интерфейс состояния
    // ─────────────────────────────────────────────
 
    public interface IUnitState
    {
        UnitStateId StateId { get; }
 
        void OnEnter(UnitInstance unit, IUnitCommand triggerCommand);
        void OnExit(UnitInstance unit);
        void Tick(UnitInstance unit, float dt);
 
        // Вернуть false = "не могу принять команду сейчас"
        // Вернуть true  = "принял, обработаю"
        bool HandleCommand(UnitInstance unit, IUnitCommand command);
 
        // Форсированный переход — не отклоняется
        void ForceTransition(UnitInstance unit, UnitStateId targetState);
    }
}