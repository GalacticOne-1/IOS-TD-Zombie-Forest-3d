using Galactic1;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public abstract class _AI
    {
        protected _Entity Entity;
        
        
        public enum ELogicStateType
        {
            IDLE, ROAMING, CHASE, MOVEMENT, ATTACK,  
        }

        protected ELogicStateType STATE { get; private set; }


        protected _Timer _timer;
        protected float timeDelay = .1f;
        protected bool freezeVisual = false;
        
        
        protected Vector3 startingPosition;
        protected Vector3 roamPosition;
        protected bool inCamp;

        protected ITarget cashTarget;
        

        protected _AI(_Entity entity)
        {
            this.Entity = entity;
            _timer = new _Timer();
        }


        
        
        /// <summary>
        /// For change state
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        public void RequestState(ELogicStateType request, string client)
        {
            if(STATE != request)
            {
                Entity.Log(new CEntityDebugParam()
                {
                    Message = $">>> Oject : Changing AI state [{STATE} => {request}] [client : {client}]",
                    Color = EDlogColor.YELLOW
                });
                STATE = request;
            }
            else
            {
                Entity.Log(new CEntityDebugParam()
                {
                    Message = $">>> Oject : Try changing AI state [{STATE} => {request}] [client : {client}]",
                    Color = EDlogColor.ORANGE
                });
            }
        }
        
        
        


        public void Activate(bool freezeVisual)
        {
            this.freezeVisual = freezeVisual;
            startingPosition = ((_Object_)Entity).Tr.position;
            roamPosition = GetRoamingPosition();
        }




        public Vector3 GetRoamingPosition() => startingPosition.GetPositionFromRandomDirection();



        /// <summary>
        /// For updating
        /// </summary>
        public void Logic()
        {
            if (_timer.Elapsed > timeDelay && !Entity.RequestedTransition)
            {
                _timer.Elapsed = 0;
                // 

                Entity.Log(new CEntityDebugParam()
                {
                    Message = $"AI : state {STATE}"
                });
                LogicUpdate();
            }
        }

        protected abstract void LogicUpdate();




        protected _Entity GetTarget(GameObject entity) => null;// entity.GetComponent<IHealthComponentCollider>().GetControlller();
        
        // поиск цели для атаки
        protected abstract bool FindTarget();
        
        protected virtual bool UpdateTarget()
        {
            // if (!Entity.Target.Available(Entity._feature.GetAttribute(StatId.AttackRange)))
            // {
            //     Entity.Log(new CEntityDebugParam()
            //     {
            //         Message = $"******************************************************************* \n {Entity.gameObject} : Target lost",
            //         Color = EDlogColor.ORANGE
            //     });
            //     
            //     //Debug.LogError("****");
            //     return false;
            // }

            Entity.Log(new CEntityDebugParam()
            {
                Message = $"Target ok",
            });
            if (!freezeVisual)
                Entity.Animation.VisualDirection(Entity.Target.ITarget.CenterRadius.x - Entity.CenterRadius.x);
            return true;
        }

        /// <summary>
        /// Если есть цель
        /// </summary>
        protected void HaveTarget()
        {
            // #1 цель в радиусе, атаkуем
            // if (Entity.Target.InRadius(Entity._feature.GetAttribute(StatId.AttackRange)))
            // {
            //     Entity.RequestState(EUnitStateType.ATTACK, "AI.HaveTarget()", () =>
            //     {
            //         RequestState(ELogicStateType.ATTACK, "AI.HaveTarget()");
            //     });
            // }

            // #2 цель вне радиуса, движемся к ней
            // else
            // {
            //     (Entity._FSM.GetStateHandler(EUnitStateType.CHASE) as ChaseState_v1).finishCoord =
            //         Entity.Target.ITarget.tr.position;
            //     
            //     Entity.RequestState(EUnitStateType.CHASE, "AI.HaveTarget()", () =>
            //     {
            //         RequestState(ELogicStateType.CHASE, "AI.HaveTarget()");
            //     });
            // }
        }

        protected virtual bool TryingGetNewTarget() => true;




        public virtual void MovementComplete(bool mustAttack = false)
        {
            Entity.Log(new CEntityDebugParam() { Message = $"AI : movement complete", Color = EDlogColor.GREEN });
            
            // #1 если цель существо
            if (Entity.Target.IsLive())
            {
                // attack
                // if (Entity.Target.AvailToAttack(Entity._feature.GetAttribute(StatId.AttackRange)))
                // {
                //     Entity.RequestState(EUnitStateType.ATTACK, "AI.MovementComplete()", () =>
                //     {
                //         RequestState(ELogicStateType.ATTACK, "AI.MovementComplete()");
                //     });
                // }
                //
                // else
                // {
                //     // обновляем коорд
                //     (Entity._FSM.GetStateHandler(EUnitStateType.CHASE) as ChaseState_v1).finishCoord =
                //         Entity.Target.ITarget.tr.position;
                // }
            }
            
            // #2 если цель просто координаты
            else
            {
                inCamp = true;          // FIX: нужна проверка дистанции до лагеря
                
                Entity.RequestState(EUnitStateType.IDLE, "AI.MovementComplete()", () =>
                {
                    RequestState(ELogicStateType.IDLE, "AI.MovementComplete()");
                });
            }
        }
    }
}