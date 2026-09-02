
using Galactic1;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class ChaseState_v1 : _State_
    {
        /*
         *      Для движения за существами
         */
        
        
        public ChaseState_v1(_Entity entity, FSM fsm, CStateTransition setup) : base(entity, fsm, setup)
        {
        }


        public Vector3 finishCoord;



        #region STATE


        public override void Enter()
        {
            Entity.Log(new CEntityDebugParam() { Message = "State Chase : Enter"});
            base.Enter();
            // ******       ! must have !      ******
            // ******
            // ******

            Entity.Animation.AnimationToggle(_Animation.EAnimationTriggerType.Movement);
        }

        public override void Logic()
        {
            Entity.Log(new CEntityDebugParam() { Message = "State Chase : Logic"});
            base.Logic();
            if (onExit) return;
            // ******       ! must have !      ******
            // ******
            // ******
            
            
            // #1 движемся
            Entity.Animation.VisualDirection(finishCoord.x - Entity.CenterRadius.x);
            // Entity.tr.position = Vector2.MoveTowards(Entity.tr.position, finishCoord,
            //     Entity._feature.GetValue(StatId.SpeedMovement) * Time.deltaTime);
            
            // #2 чек достижения коородинат
            if (Vector3.Distance(((_Object_)Entity).Tr.position, finishCoord) < .3f)         
            {
                Entity.Log(new CEntityDebugParam() { Message = "State Chase : Complete", Color = EDlogColor.YELLOW });
                Entity.AI.MovementComplete();
            }
        }

        protected override void Exit()
        {
            Entity.Log(new CEntityDebugParam() { Message = "State Chase : Exit", Color = EDlogColor.YELLOW });
        }

        public override bool CanExit() => true;
        
        

        #endregion
        
        
        /// <summary>
        /// true - цель осталась по последним координатам
        /// </summary>
        /// <returns></returns>
        public bool TargetOnPoint()
            => Entity.Target.ITarget.tr.position.x < finishCoord.x + 1 && Entity.Target.ITarget.tr.position.x > finishCoord.x - 1 &&
               Entity.Target.ITarget.tr.position.y < finishCoord.y + 1 && Entity.Target.ITarget.tr.position.y > finishCoord.y - 1;
    }
}