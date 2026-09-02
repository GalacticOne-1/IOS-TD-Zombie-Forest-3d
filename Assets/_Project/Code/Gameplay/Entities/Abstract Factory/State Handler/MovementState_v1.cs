
using Galactic1;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class MovementState_v1 : _State_
    {
        /*
         *      Для движения по координатам
         */
        
        public MovementState_v1(_Entity entity, FSM fsm, CStateTransition setup) : base(entity, fsm, setup)
        {
        }
        
        
        public Vector3 movementCoord;
        
        
        
        #region STATE

        public override void Enter()
        {
            Entity.Log(new CEntityDebugParam()
            {
                Message = "State Movement : Enter"
            });
            base.Enter();
            // ******       ! must have !      ******
            // ******
            // ******
            
            Entity.Animation.AnimationToggle(_Animation.EAnimationTriggerType.Movement);
        }

        public override void Logic()
        {
            Entity.Log(new CEntityDebugParam()
            {
                Message = "State Movement : Logic"
            });
            base.Logic();
            if (onExit) return;
            // ******       ! must have !      ******
            // ******
            // ******
            
            
            // #1 движемся
            Entity.Animation.VisualDirection(movementCoord.x - Entity.CenterRadius.x);
            // Entity.tr.position = Vector2.MoveTowards(Entity.tr.position, movementCoord,
            //     Entity._feature.GetAttribute(StatId.SpeedMovement) * Time.deltaTime);
            
            // #2 чек достижения коородинат
            if (Vector3.Distance(((_Object_)Entity).Tr.position, movementCoord) < .1f)         
            {
                DLog.Alert("State Movement : Complete", EDlogColor.YELLOW);
                Entity.AI.MovementComplete();
            }
        }

        protected override void Exit()
        {
            DLog.Alert("State Movement : Exit", EDlogColor.YELLOW);
        }

        public override bool CanExit() => true;
        
        #endregion
    }
}