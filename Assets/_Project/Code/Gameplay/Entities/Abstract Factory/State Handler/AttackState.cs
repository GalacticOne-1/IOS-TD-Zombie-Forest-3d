
using Galactic1;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class AttackState : _State_
    {
        public AttackState(_Entity entity, FSM fsm, CStateTransition setup) : base(entity, fsm, setup)
        {
        }

        public override void Enter()
        {
            Entity.Log(new CEntityDebugParam() { Message = "State Attack : Enter" });
            base.Enter();
            // ******       ! must have !      ******
            // ******
            // ******
            
            Entity.AttackContainer.Begin();
        }

        public override void Logic()
        {
            Entity.Log(new CEntityDebugParam() { Message = "State Attack : Logic" });
            base.Logic();
            if (onExit) return;
            // ******       ! must have !      ******
            // ******
            // ******
            
            Entity.AttackContainer.Process();
        }

        protected override void Exit()
        {
            Entity.Log(new CEntityDebugParam() { Message = "State Attack : Exit", Color = EDlogColor.YELLOW });
            
            Entity.AttackContainer.Stop();
        }

        public override bool CanExit() => Entity.AttackContainer.CanStop();

        public override void ExitForDeath()
        {
            Entity.Animation.AnimationToggle(_Animation.EAnimationTriggerType.Reset);
            base.ExitForDeath();
        }
    }
}