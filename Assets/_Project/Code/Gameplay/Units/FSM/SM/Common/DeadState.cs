using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    // ─────────────────────────────────────────────
    //  DeadState
    //
    //  Финальное состояние. Выхода нет.
    //  Despawn / pooling — снаружи, через UnitDiedEvent.
    //
    //  Что делает:
    //    - Отписывает юнита от UnitTimerSystem (если используется)
    //    - Больше ничего не тикает
    // ─────────────────────────────────────────────

    public sealed class DeadState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.Dead;
 
        public void OnEnter(UnitInstance unit, IUnitCommand triggerCommand)
        {
            unit.CurrentWeaponHandle?.Dispose();
            unit.CurrentWeaponHandle = null;
 
            EventBus<UnitReadyForDespawnEvent>.Raise(new UnitReadyForDespawnEvent(unit as SurvivorInstance));
        }
 
        public void OnExit(UnitInstance unit)
        {
            Debug.LogError($"[DeadState] OnExit на {unit.name} — нельзя покинуть Dead");
        }
 
        public void Tick(UnitInstance unit, float dt) { }
 
        public bool HandleCommand(UnitInstance unit, IUnitCommand command) => true;
 
        public void ForceTransition(UnitInstance unit, UnitStateId targetState)
        {
            Debug.LogError($"[DeadState] ForceTransition на {unit.name} — нельзя покинуть Dead");
        }
    }
    
    // public sealed class DeadState : IUnitState
    // {
    //     public UnitStateId StateId => UnitStateId.Dead;
    //
    //     public void OnEnter(SurvivorInstance unit, IUnitCommand triggerCommand)
    //     {
    //         // Снять оружие — WeaponHandle.Dispose() чистит entity, view, pool
    //         unit.CurrentWeaponHandle?.Dispose();
    //         unit.CurrentWeaponHandle = null;
    //
    //         // Уведомить что юнит полностью мёртв (после анимации)
    //         EventBus<UnitReadyForDespawnEvent>.Raise(new UnitReadyForDespawnEvent(unit));
    //     }
    //
    //     public void OnExit(SurvivorInstance unit)
    //     {
    //         // Из Dead не выходят — этот метод не должен вызываться
    //         Debug.LogError($"[DeadState] OnExit вызван на {unit.name} — это не должно происходить");
    //     }
    //
    //     public void Tick(SurvivorInstance unit, float dt)
    //     {
    //     }
    //
    //     public bool HandleCommand(SurvivorInstance unit, IUnitCommand command)
    //     {
    //         return true; // Мёртвый не принимает команды
    //     }
    //
    //     public void ForceTransition(SurvivorInstance unit, UnitStateId targetState)
    //     {
    //         Debug.LogError($"[DeadState] ForceTransition на {unit.name} — нельзя покинуть Dead");
    //     }
    // }
}