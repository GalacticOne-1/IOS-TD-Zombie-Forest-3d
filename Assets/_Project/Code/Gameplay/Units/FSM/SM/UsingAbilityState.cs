using Galactic1.Code.Gameplay.Abilities;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{

    // ─────────────────────────────────────────────
    //  UsingAbilityState
    //
    //  Нюанс: ReactiveAI.IsEnabled — специфика игрока.
    //  Вместо прямого вызова используем Brain.IsEnabled,
    //  который доступен через публичное свойство UnitInstance.
    //
    //  WeaponRigController.HitOrigin — специфика SurvivorInstance.
    //  Получаем через as-cast; у AI-юнитов способности без
    //  SpawnOrigin (или задают его по-другому через контекст).
    // ─────────────────────────────────────────────

    public sealed class UsingAbilityState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.UsingAbility;

        private IUnitCommand _pendingCommand;
        private readonly AbilityUseCoordinator _coordinator;
        private AbilityComponent _abilityComponent;
        private UnitInstance _unit;
        private bool _wasMoving;
        private bool _waitingForAnimation;
        private bool _needsRotation;
        private Vector3 _rotationTarget;

        public UsingAbilityState(AbilityUseCoordinator coordinator)
        {
            _coordinator = coordinator;
        }

        // ─────────────────────────────────────────
        //  Enter / Exit
        // ─────────────────────────────────────────

        public void OnEnter(UnitInstance unit, IUnitCommand triggerCommand)
        {
            _unit = unit;
            _abilityComponent = unit.Ability;
            _wasMoving = unit.Mover.IsMoving;
            _waitingForAnimation = false;
            _needsRotation = false;
            _pendingCommand = null;

            unit.Status.SetAbilityBusy(true);
            
            unit.Mover.Stop();
            unit.Mover.SetRotationControl(false);

            // Отключаем реактивный мозг пока анимация способности
            if (unit.Brain != null) unit.Brain.IsEnabled = false;

            if (triggerCommand is not AbilityCommand cmd) return;

            cmd.Context.OnCancelled = () =>
            {
                if (_unit != null) TransitionTo(_unit, UnitStateId.Idle);
            };

            bool needsAnimation = cmd.Context.UseModule.Behaviour is GrenadeBehaviour;

            if (needsAnimation)
            {
                _waitingForAnimation = true;
                _abilityComponent.OnFinished += OnAbilityComplete;

                cmd.Context.OnConfirmed += () =>
                {
                    _rotationTarget = cmd.Context.TargetPosition;
                    _needsRotation = true;

                    // SpawnOrigin — рука игрока. Только у SurvivorInstance есть WeaponRigController.
                    if (unit is SurvivorInstance survivor)
                        cmd.Context.SpawnOrigin = survivor.WeaponRigController.HitOrigin;
                };
            }
            else
            {
                cmd.Context.OnConfirmed += () =>
                {
                    if (_unit.StateMachine.HasStack) _unit.StateMachine.PopState();
                    else TransitionTo(_unit, UnitStateId.Idle);
                };
            }

            _coordinator.Use(cmd.Context);
        }

        public void OnExit(UnitInstance unit)
        {
            if (unit.Brain != null) 
                unit.Brain.IsEnabled = true;
            unit.Mover.SetRotationControl(true);

            _waitingForAnimation = false;
            _needsRotation = false;
            _pendingCommand = null;
            _unit = null;
            unit.Status.SetAbilityBusy(false);

            unit.AnimationController?.CombatExit();

            if (_abilityComponent != null)
                _abilityComponent.OnFinished -= OnAbilityComplete;

            _coordinator.Cancel();
        }

        // ─────────────────────────────────────────
        //  Tick
        // ─────────────────────────────────────────

        public void Tick(UnitInstance unit, float dt)
        {
            if (!_needsRotation) return;

            Vector3 dir = _rotationTarget - unit.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;

            unit.transform.rotation = Quaternion.RotateTowards(
                unit.transform.rotation,
                Quaternion.LookRotation(dir),
                600f * dt);
        }

        // ─────────────────────────────────────────
        //  HandleCommand
        // ─────────────────────────────────────────

        public bool HandleCommand(UnitInstance unit, IUnitCommand command)
        {
            if (_waitingForAnimation &&
                command is MoveCommand or FallBackCommand or AttackCommand or TakeCoverCommand)
            {
                _pendingCommand = command;
                return true;
            }

            return false;
        }

        public void ForceTransition(UnitInstance unit, UnitStateId targetState)
            => unit.StateMachine.ForceState(targetState);

        // ─────────────────────────────────────────
        //  Callback — AE_TossGrenadeFinish
        // ─────────────────────────────────────────

        public void OnAbilityComplete()
        {
            _waitingForAnimation = false;
            _needsRotation = false;

            if (_unit == null) return;

            if (_pendingCommand != null)
            {
                var cmd = _pendingCommand;
                _pendingCommand = null;
                _unit.StateMachine.Execute(cmd);
            }
            else if (_unit.StateMachine.HasStack)
            {
                _unit.StateMachine.PopState();
            }
            else
            {
                TransitionTo(_unit, UnitStateId.Idle);
            }
        }

        private void TransitionTo(UnitInstance unit, UnitStateId state)
            => unit.StateMachine.TransitionTo(state, null);
    }

}