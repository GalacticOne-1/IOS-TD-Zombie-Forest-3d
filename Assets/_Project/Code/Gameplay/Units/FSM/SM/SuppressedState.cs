using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.PoolObject;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Состояние оглушения юнита (шумовая граната).
    ///
    /// Behaviour:
    /// - Останавливает движение при входе.
    /// - Блокирует все входящие команды пока активно.
    /// - По истечении таймера переходит в Idle.
    ///
    /// Не использует стек прерываний (PopState) —
    /// после оглушения юнит всегда возвращается в Idle,
    /// а не к предыдущему состоянию.
    /// </summary>
    public sealed class SuppressedState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.Suppressed;

        private float _stunTimer;
        private IUnitCommand _pendingCommand;

        public void OnEnter(UnitInstance unit, IUnitCommand triggerCommand)
        {
            _stunTimer = triggerCommand is StunCommand stun ? stun.Duration : 1f;
            _pendingCommand = null;

            unit.Mover.Stop();
            if (unit.Brain != null) unit.Brain.IsEnabled = false;

            SpawnVfx(unit);
        }

        public void OnExit(UnitInstance unit)
        {
            if (unit.Brain != null) unit.Brain.IsEnabled = true;
        }

        public void Tick(UnitInstance unit, float dt)
        {
            _stunTimer -= dt;
            if (_stunTimer > 0f) return;

            if (_pendingCommand != null)
            {
                var cmd = _pendingCommand;
                _pendingCommand = null;
                unit.StateMachine.TransitionTo(cmd.TargetState, cmd);
            }
            else if (unit.StateMachine.HasStack)
            {
                unit.StateMachine.PopState();
            }
            else
            {
                unit.StateMachine.TransitionTo(UnitStateId.Idle, null);
            }
        }

        public bool HandleCommand(UnitInstance unit, IUnitCommand command)
        {
            if (command is MoveCommand or FallBackCommand or TakeCoverCommand)
            {
                _pendingCommand = command;
                return true;
            }

            return true; // Всё остальное — поглощаем
        }

        public void ForceTransition(UnitInstance unit, UnitStateId targetState)
            => unit.StateMachine.TransitionTo(targetState, null);

        private void SpawnVfx(UnitInstance unit)
        {
            ServiceLocator.Current.Get<EffectRequestSystem>().Request(
                new EffectRequest
                {
                    Id = GameIdProvider.StunVfx,
                    AttachTo = unit.transform,
                    Duration = _stunTimer
                },
                EffectPriority.Normal,
                fx => fx.SetActive(true));
        }
    }
}