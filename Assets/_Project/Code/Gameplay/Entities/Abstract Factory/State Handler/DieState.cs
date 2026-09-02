

namespace Galactic1.AbstractFactory
{
    public class DieState : _State_
    {
        private bool requestedDie;

        public DieState(_Entity entity, FSM fsm, CStateTransition setup) : base(entity, fsm, setup)
        {
            requestedDie = false;
        }

        
        
        /*
         *      ENTER:
         *          - launch animation
         *          - launch sound
         *          - launch fx
         *          - disable gui
         *
         *          - launch Entity_Deactivate
         *          (если не нужно быстрое отключение, установить время в transition.exitTime для состояния DIE)
         *
         *      EXIT:
         *          без выхода
         */
        
        
        
        
        public override void Enter()
        {
            Entity.Log(new CEntityDebugParam() { Message = "State Die : Enter"});
            base.Enter();
            // ******       ! must have !      ******
            // ******
            // ******
            
            Entity.Animation.AnimationToggle(_Animation.EAnimationTriggerType.Die);
            //abilRef.UX_deactivate();
            //aucRef.PlaySound(EUnitClip.die);

            
            // ***  запуск отключения юнита
            if (!transition.needExitTime)
            {
                Entity.Entity_Deactivate(true);
                Entity.OnDeactivate?.Invoke(Entity);
            }
            else
            {
                requestedDie = true;
                // для сброса в 0, если enter был по таймеру
                timer = new _Timer();       
            }
        }

        public override void Logic()
        {
            Entity.Log(new CEntityDebugParam() { Message = "State Die : Logic"});
            base.Logic();
            // ******       ! must have !      ******
            // ******
            // ******
            
            // для таймера используем переменные из transition
            if (requestedDie && timer.Elapsed > transition.exitTime)
            {
                Entity.Entity_Deactivate(true);
                Entity.OnDeactivate?.Invoke(Entity);
            }
        }

        protected override void Exit()
        {
            Entity.Log(new CEntityDebugParam() { Message = "State Die : Exit", Color = EDlogColor.YELLOW});
        }

        public override bool CanExit() => true;
    }
}