using Galactic1;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class IdleState : _State_
    {
        public IdleState(_Entity entity, FSM fsm, CStateTransition setup) : base(entity, fsm, setup)
        {
        }

        public override void Enter()
        {
            Entity.Log(new CEntityDebugParam() { Message = $"State Idle : Enter {Entity.gameObject}"});
            base.Enter();
            // ******       ! must have !      ******
            // ******
            // ******
            
            Entity.Animation.AnimationToggle(_Animation.EAnimationTriggerType.Idle);
        }

        public override void Logic()
        {
            Entity.Log(new CEntityDebugParam() { Message = "State Idle : Logic"});
            base.Logic();
            if (onExit) return;
            // ******       ! must have !      ******
            // ******
            // ******
            
        }

        protected override void Exit()
        {
            Entity.Log(new CEntityDebugParam() { Message = $"State Idle : Exit {Entity.gameObject}", Color = EDlogColor.YELLOW });
        }

        public override bool CanExit() => true;
    }
}