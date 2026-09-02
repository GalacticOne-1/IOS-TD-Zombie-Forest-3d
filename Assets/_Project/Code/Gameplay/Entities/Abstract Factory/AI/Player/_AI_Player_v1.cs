using Galactic1;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class _AI_Player_v1 : _AI_Player
    {
        /*
         *
         */






        public _AI_Player_v1(_Entity entity) : base(entity)
        {

        }




        protected override void LogicUpdate()
        {

            /*
             *      - логика должна быть в рамках своего состояния
             *      - не должно быть внешних условий (за пределами switch)
             *      - каждая логика должна быть в своем методе и добавлена в любое состояние где эта логика требуется
             */

            switch (STATE)
            {

                // просто бродим по территории или стоим
                default:
                case ELogicStateType.ROAMING:
                {
                    // * find target
                    if (FindTarget())
                    {
                        Entity.Log(new CEntityDebugParam()
                        {
                            Message = "************************ Player : Found target"
                        });
                        //Debug.LogError("****");
                        Entity.RequestState(EUnitStateType.ATTACK, "ELogicType.Roaming", () =>
                        {
                            RequestState(ELogicStateType.ATTACK, "ELogicType.Roaming");
                        });
                    }
                    else
                    {
                        
                    }
                    //DLog.Alert(">>> state default", EDlogColor.YELLOW, 1);
                }
                    break;

                // преследование цели
                case ELogicStateType.CHASE:
                {

                }
                    break;


                case ELogicStateType.ATTACK:
                {
                    if (!UpdateTarget())
                    {
                        Entity.RequestState(EUnitStateType.IDLE, "ELogicType.ATTACK", () =>
                        {
                            RequestState(ELogicStateType.IDLE, "ELogicType.ATTACK");
                        });
                        Entity.AttackContainer.ForceStop();
                        Entity.Target.RemoveTarget();
                    }
                }
                    break;
            }
        }


        
        
        
        
        



        //     #region FOR ENEMIES
        //
        //     // поиск цели для атаки
        //     bool FindTarget()
        //     {
        //         cashTarget = null;
        //         cashTarget = unit.trgRef.GetTarget(HUBLink.player.tr.position, maxDistanceToOwner);
        //     
        //         if (cashTarget == null)
        //         {
        //             //DLog.Alert(">>> ROBOT : Target null", EDlogColor.YELLOW);
        //             return false;
        //         }
        //         
        //         unit.target = cashTarget;
        //         return true;
        //     }
        //
        //     // отслеживаем полученную цель
        //     bool UpdateTarget()
        //     {
        //         if (unit.target == null ||
        //             !unit.target.IsLive() ||
        //             !unit.trgRef.TargetOnFloor(unit.target.tr) ||
        //             !unit.trgRef.TargetInRay(HUBLink.player.tr.position, unit.target, maxDistanceToOwner))
        //         {
        //             return false;
        //         }
        //
        //         return true;
        //     }
        //
        //     // для перевода в состояние атаки
        //     bool CheckTargetToAttack()
        //     {
        //         if (TargetUnderAttack())
        //         {
        //             RequestState(EFSM.Attack_Enemy, "CheckTargetToAttack()");
        //             unit.Fsm = FSM.attack;
        //             return true;
        //         }
        //
        //         return false;
        //     }
        //
        //     bool TargetUnderAttack() => Vector3.Distance(unit.tr.position, unit.target.tr.position) < unit.attackRef.weapon.rangeAttack - .1f;
        //
        //     bool IsAttack() => unit.Fsm == FSM.attack && unit.attackRef._WPS != WeaponABS.EWeaponStep.reload;
        //     
        //     // для запуска движения к цели
        //     void BeginMovementToEnemy()
        //     {
        //         RequestState(EFSM.Movement_To_Enemy, "BeginMovementToEnemy()");
        //         (unit as PlayerUnit_Companion).BeginMoveToEnemy();
        //         IN_RANGE = false;
        //     }
        //     
        //
        //     #endregion
        //
        //
        //
        //
        //
        //     public void EndConnecting()
        //     {
        //         IN_RANGE = false;
        //         RequestState(EFSM.Idle, "EndMoving() EFSM.Movement_To_Player");
        //         unit.BeginIdle();
        //     }
        //
        //     public override void EndMoving()
        //     {
        //         //GConsole.ClearLog();
        //         //DLog.Alert("ROBOT : End moving!", EDlogColor.YELLOW);
        //         //Debug.LogError("stop");
        //         
        //         IN_RANGE = false;
        //
        //         switch (STATE)
        //         {
        //             case EFSM.Movement_To_Player:
        //             {
        //                 if(OwnerInRange())
        //                 {
        //                     IN_RANGE = true;
        //                     if(unit.target.Fsm != FSM.move)
        //                     {
        //                         RequestState(EFSM.Idle, "EndMoving() EFSM.Movement_To_Player");
        //                         unit.BeginIdle();
        //                     }
        //                 }
        //                 else
        //                 {
        //                     unit.UpdateMovePoint();
        //                 }
        //             } break;
        //
        //             
        //             case EFSM.Movement_To_Enemy:
        //             {
        //                 BeginMovementToEnemy();
        //             } break;
        //         }
        //         
        //         // if (Vector3.Distance(unit.tr.position, unit.target.tr.position) < unit.attackRef.weapon.rangeAttack + .1f)
        //         // {
        //         //     DLog.Alert(">>> To Attack!", EDlogColor.ORANGE);
        //         //     unit.animRef.SetAnim(FSM.idle);
        //         //     unit.Fsm = FSM.attack;
        //         // }
        //         //
        //         // else
        //         // {
        //         //     DLog.Alert(">>> Enemy far, continue moving!", EDlogColor.ORANGE);
        //         //     if (unit.Fsm == FSM.move || unit.Fsm == FSM.move_to_attack)
        //         //     {
        //         //         unit.moveRef.targetCoord = unit.target.tr.position;
        //         //     }
        //         //     else
        //         //     {
        //         //         unit.Fsm = FSM.idle;
        //         //         unit.animRef.SetAnim(FSM.idle);
        //         //     }
        //         // }
        //     }
        //
        //
        //     private void OnDrawGizmos()
        //     {
        //         if(HUBLink.player_unit != null)
        //         {
        //             Gizmos.color = Color.green;
        //             Gizmos.DrawWireSphere(HUBLink.player.tr.position, distancePatrol);
        //             
        //             Gizmos.color = Color.blue;
        //             Gizmos.DrawWireSphere(HUBLink.player.tr.position, maxDistanceToOwner);
        //         }
        //     }
        // }

    }
}