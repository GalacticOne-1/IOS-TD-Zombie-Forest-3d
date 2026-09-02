
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class _AI_Enemy_v1 : _AI_Enemy
    {
        /*
         *      Поиск цели, движение к ней и атака
         *      если цели нет, движемся в лагерь игрока
         */


        private float delay_roam;
        
        private Vector2 camp;
        
        
        private MovementState_v2 _movementStateV2;
        

        public _AI_Enemy_v1(_Entity entity) : base(entity)
        {
            
        }


        
        public void SetMovementHandler()
        {
            if (_movementStateV2 == null && Entity._FSM.GetStateHandler(EUnitStateType.MOVEMENT) is MovementState_v2 handler)
            {
                _movementStateV2 = handler;
            }
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
                // получаем цель для преследования
                default:
                case ELogicStateType.IDLE:
                {
                    if (Entity.Target.ITarget == null || !Entity.Target.ITarget.IsLive)
                    {
                        Entity.Log(new CEntityDebugParam() { Message = "LogicUpdate : Target is NULL, try find" });
                        
                        // опрелять менее защищенную цель?
                        // пока просто ищем ближайшую 
                        
                        // когда зомби входит в состояние покоя, он должен получить какой то юнит игрока 
                        // что бы ии работал 
                        if (TryingGetNewTarget())
                        {
                            Entity.RequestState(EUnitStateType.MOVEMENT, "ELogicStateType.IDLE", () =>
                            {
                                RequestState(ELogicStateType.MOVEMENT, "ELogicStateType.IDLE");
                            });
                        }
                    }

                    else
                    {
                        Entity.Log(new CEntityDebugParam() { Message = "LogicUpdate : Have a target, begin move" });
                        _movementStateV2.onPathComplete = _ =>
                        {
                            Entity.RequestState(EUnitStateType.MOVEMENT, "ELogicStateType.IDLE", () =>
                            {
                                RequestState(ELogicStateType.MOVEMENT, "ELogicStateType.IDLE");
                            });
                        };
                        _movementStateV2.UpdatePath(Entity.Target.ITarget.tr.position);
                    }
                }
                    break;
                    
                // просто бродим по территории или стоим
                case ELogicStateType.ROAMING:
                {
                    // * find target
                    if (FindTarget())
                    {
                        Entity.Log(new CEntityDebugParam()
                        {
                            Message = $"************************ Enemy : Found target {Entity.Target.ITarget.Obj}"
                        });
                        //Debug.LogError("****");
                        
                        HaveTarget();
                    }
                    
                    // целей нет
                    else
                    {
                        
                        // находимся в лагере, бродим
                        if (inCamp)
                        {
                            delay_roam -= Time.deltaTime;
                            if (delay_roam < 0)
                            {
                                delay_roam = Random.Range(.5f, 3);
                                camp = new Vector3(30 + Random.Range(-3, 3), Random.Range(-3, 3), 0);
                                
                                bool left_side = Random.Range(0, 2) == 0 &&
                                                 (((_Object_)Entity).Tr.position.x > camp.x || Mathf.Abs(camp.x - ((_Object_)Entity).Tr.position.x) < 10);

                                // * если вправо нельзя
                                if (!left_side)
                                    left_side = ((_Object_)Entity).Tr.position.x > camp.x && Mathf.Abs(camp.x - ((_Object_)Entity).Tr.position.x) > 10;

                                if (left_side)
                                {
                                    var v = ((_Object_)Entity).Tr.position;
                                    v.x -= Random.Range(2, 5);
                                    v.y -= Random.Range(2, 5);
                                    _movementStateV2.UpdatePath(v);
                                }
                                else
                                {
                                    var v = ((_Object_)Entity).Tr.position;
                                    v.x += Random.Range(2, 5);
                                    v.y += Random.Range(2, 5);
                                    _movementStateV2.UpdatePath(v);
                                }
                                
                                Entity.RequestState(EUnitStateType.MOVEMENT, "ELogicStateType.ROAMING", () =>
                                {
                                    RequestState(ELogicStateType.MOVEMENT, "ELogicStateType.ROAMING");
                                });
                            }
                        }

                        // движемся в лагерь игрока
                        else
                        {
                            // определяем цель
                            // var playerUnit = _PointerHub.player_unit[0];
                            // unit._target.NewTarget(playerUnit);
                            // _movementStateV2.finishCoord.Value = playerUnit.tr.position;
                            // unit.RequestState(EUnitStateType.MOVEMENT, "ELogicType.ROAMING", () =>
                            // {
                            //     RequestState(ELogicStateType.MOVEMENT, "ELogicType.ROAMING");
                            // });
                        }
                    }
                    //DLog.Alert(">>> state default", EDlogColor.YELLOW, 2);
                }
                    break;

                
                case ELogicStateType.MOVEMENT:
                {
                    // пока движемся ищем цели
                    // if (FindTarget())
                    // {
                    //     HaveTarget();
                    // }
                    
                    // ! во время движения никакой логики !
                    // только чекаем текущую цель => юнит игрока
                    // если он мертв, делаем запрос на другой юнит игрока
                    if (!Entity.Target.IsLive())
                    {
                        if (TryingGetNewTarget())
                        {
                            Entity.RequestState(EUnitStateType.MOVEMENT, "ELogicStateType.MOVEMENT", () =>
                            {
                                RequestState(ELogicStateType.MOVEMENT, "ELogicStateType.MOVEMENT");
                            });
                        }
                    }

                    // так же чекаем путь, может игрок отремонтировал ограждения и пройти уже нельзя
                    // if ()
                    // {
                    //     
                    // }
                }
                    break;

                // преследование цели
                case ELogicStateType.CHASE:
                {
                    // #1 отслеживаем цель, меняем коорд для преследования
                    
                    
                    // #2 переходим в атаку
                    if (UpdateTarget())
                    {
                        Entity.RequestState(EUnitStateType.ATTACK, "ELogicStateType.CHASE", () =>
                        {
                            RequestState(ELogicStateType.ATTACK, "ELogicStateType.CHASE");
                        });
                    }
                }
                    break;


                case ELogicStateType.ATTACK:
                {
                    if (!UpdateTarget())
                    {
                        Entity.RequestState(EUnitStateType.IDLE, "ELogicStateType.ATTACK", () =>
                        {
                            RequestState(ELogicStateType.IDLE, "ELogicStateType.ATTACK");
                        });
                        Entity.AttackContainer.ForceStop();
                        Entity.Target.RemoveTarget();
                    }
                }
                    break;
            }
        }

        protected override bool TryingGetNewTarget()
        {
            // _Entity tryGetTarget = TargetFinderService.FindTarget(
            //     Entity.tr.position,
            //     _PointerHub.GetPlayerUnits(),
            //     TargetSelectionMode.Closest)?.GetComponent<_Entity>();
            // if (tryGetTarget)
            // {
            //     Entity._target.NewTarget(tryGetTarget);
            //     _movementStateV2.UpdatePath(tryGetTarget.tr.position);
            //     return true;
            // }
            return false;
        }


        public override void MovementComplete(bool mustAttack = false)
        {
            
            Entity.Log(new CEntityDebugParam() { Message = $"AI : Movement complete. Target: {Entity.Target.IsLive()}", Color = EDlogColor.GREEN });

            if (Entity.UnitInterface.OnlyThisLogs)
            {
                
            }
            
            if (Entity.Target.IsLive())
            {
                // // кидаем луч в нашу цель для провекри доступности атаки
                // Entity.Target.UnderRay(
                //     Entity._feature.GetAttribute(StatId.AttackRange),
                //     out bool underRay,
                //     out bool inRadius,
                //     out GameObject hitRay);
                //
                // Entity.Log(new CEntityDebugParam()
                // {
                //     Message = $"MovementComplete() : target status >> [under ray > {underRay}] [in radius > {inRadius}] [hit ray > {hitRay}]",
                //     Color = EDlogColor.YELLOW
                // });
                //
                // if (underRay)
                // {
                //     if (inRadius)
                //     {
                //         // for test (уничтожаем зомби по прибытию к цели)
                //         // DamageSystem.I.ApplyDamage(new DamageEvent()
                //         // {
                //         //     Attacker = null,
                //         //     Target = Entity,
                //         //     Amount = 1000,
                //         //     Type = DamageType.Explosion
                //         // });
                //         
                //         Entity.RequestState(EUnitStateType.ATTACK, "AI.MovementComplete()", () =>
                //         {
                //             RequestState(ELogicStateType.ATTACK, "AI.MovementComplete()");
                //         });
                //     }
                //
                //     else
                //     {
                //         Entity.RequestState(EUnitStateType.IDLE, "AI.MovementComplete()", () =>
                //         {
                //             RequestState(ELogicStateType.IDLE, "AI.MovementComplete()");
                //         });
                //     }
                //     
                //     // подходим ближе к цели
                //     // else
                //     // {
                //     //     (unit._FSM.GetStateHandler(EUnitStateType.CHASE) as ChaseState_v1).finishCoord = unit._target.ITarget.tr.position;
                //     //     unit.RequestState(EUnitStateType.CHASE, "AI.MovementComplete()", () =>
                //     //     {
                //     //         RequestState(ELogicStateType.CHASE, "AI.MovementComplete()");
                //     //     });
                //     // }
                // }
                //
                // // луч столкнулся с преградой
                // else if (hitRay != null)
                // {
                //     Entity.Log(new CEntityDebugParam()
                //     {
                //         Message = $"MovementComplete() : [must attack > {mustAttack}]",
                //         Color = EDlogColor.YELLOW
                //     });
                //     // атакуем преграду
                //     if (mustAttack)
                //     {
                //         cashTarget = GetTarget(hitRay);
                //         if (cashTarget != null)
                //         {
                //             Entity.Target.NewTarget(cashTarget);
                //             Entity.RequestState(EUnitStateType.ATTACK, "AI.MovementComplete()", () =>
                //             {
                //                 RequestState(ELogicStateType.ATTACK, "AI.MovementComplete()");
                //             });
                //         }
                //         return;
                //     }
                //     
                //     // пытаемся построить путь до цели в обход преграды
                //     _movementStateV2.onPathComplete = newPath =>
                //     {
                //         
                //         // #1 начинаем движение
                //         if (newPath)
                //         {
                //             Entity.RequestState(EUnitStateType.MOVEMENT, "AI.MovementComplete()", () =>
                //             {
                //                 RequestState(ELogicStateType.MOVEMENT, "AI.MovementComplete()");
                //             });
                //         }
                //
                //         // #2 пройти дальше не можем, атакуем преграду
                //         else
                //         {
                //             cashTarget = GetTarget(hitRay);
                //             if (cashTarget != null)
                //             {
                //                 Entity.Target.NewTarget(cashTarget);
                //                 Entity.RequestState(EUnitStateType.ATTACK, "AI.MovementComplete()", () =>
                //                 {
                //                     RequestState(ELogicStateType.ATTACK, "AI.MovementComplete()");
                //                 });
                //             }
                //         }
                //     };
                //     _movementStateV2.UpdatePath(Entity.Target.ITarget.tr.position);
                // }
                //
                // // луч никуда не попал
                // else
                // {
                //     Entity.Log(new CEntityDebugParam()
                //     {
                //         Message = $"MovementComplete() : ray is NULL",
                //         Color = EDlogColor.RED
                //     });
                // }
            }
            
            // #2 сброс состояния
            else
            {
                Entity.RequestState(EUnitStateType.IDLE, "AI.MovementComplete()", () =>
                {
                    RequestState(ELogicStateType.IDLE, "AI.MovementComplete()");
                });
            }
        }

        
    }
}