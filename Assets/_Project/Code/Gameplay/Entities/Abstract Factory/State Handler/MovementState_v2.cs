using System;
using System.Collections.Generic;
using Galactic1;
using Pathfinding;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Galactic1.AbstractFactory
{
    public class MovementState_v2 : _State_
    {
        /*
         *      Для движения по сетке
         */
        
        
        public DFuncBool2 onPathComplete;
       
        
        private Seeker seeker;
        private Path path;
        private Vector3 finishCoord;
        private Vector3 lastPathPoint;
        private Vector3 direction, targetPosition;
        private int currentWaypoint;
        private bool mustAttack;

        private float checkPathTimer;
        
        
        public MovementState_v2(_Entity entity, FSM fsm, CStateTransition setup) : base(entity, fsm, setup)
        {
            seeker = entity.gameObject.GetComponent<Seeker>();

        }


        public void UpdatePath(Vector3 newFinishCoord)
        {
            if (seeker.IsDone())
            {
                finishCoord = newFinishCoord;
                lastPathPoint = path != null ? path.vectorPath[path.vectorPath.Count - 1] : Vector3.one * -1000;

                seeker.StartPath(((_Object_)Entity).Tr.position, finishCoord, OnPathComplete);
            }
        }

        // поиск нового пути и проверка с текущим
        void FindNewPath()
        {
            seeker.StartPath(((_Object_)Entity).Tr.position, finishCoord, newPath =>
            {
                if (!newPath.error && newPath.vectorPath.Count < path.vectorPath.Count)
                {
                    OnPathComplete(newPath);
                }
            });
        }


        private void OnPathComplete(Path p)
        {
            if (!p.error)
            {
                Entity.Log(new CEntityDebugParam() { Message = "New path" });
                
                // *** меняем конечную точку если текущая приводит к не разрушаемой преграде
                // var notDestroyable = p.vectorPath[p.vectorPath.Count - 1].Ray2d_get_object(
                //     Entity._target.ITarget.tr.position,
                //     1 << AppConstants.layer_not_destroyable,
                //     out float distance,
                //     (byte)Entity.Team);
                // if (notDestroyable != null)
                // {
                //     mustAttack = true;
                //     var nearestObstacle = FindNearestObstacles(
                //         p.vectorPath[p.vectorPath.Count - 1],
                //         5,
                //         Entity._target.ITarget.tr.position);
                //     UpdatePath(nearestObstacle);
                //     return;
                // }
                //
                
                path = p;
                currentWaypoint = 0;
                checkPathTimer = 0;
                ReachedPoint();
                        
                var newPath = lastPathPoint != path.vectorPath[path.vectorPath.Count - 1];
                onPathComplete?.Invoke(newPath);
                onPathComplete = null;
            }
        }
        
        
        
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
            Entity.Log(new CEntityDebugParam() { Message = "State Movement : Logic" });
            base.Logic();
            if (onExit) return;
            // ******       ! must have !      ******
            // ******
            // ******
            
            if (path == null) return;

            

            // #1 чек достижения коородинат
            if (currentWaypoint >= path.vectorPath.Count)
            {
                Entity.Log(new CEntityDebugParam()
                {
                    Message = $"State Movement : Complete {currentWaypoint} == {path.vectorPath.Count}",
                    Color = EDlogColor.YELLOW
                });
                Entity.AI.MovementComplete(mustAttack);
                mustAttack = false;
                return;
            }
            
            // Проверяем блокировку ближайших узлов
            checkPathTimer += Time.deltaTime;
            if (checkPathTimer > 3)             // each 3 sec
            {
                checkPathTimer = 0;

                // #1
                // var min = Mathf.Min(path.vectorPath.Count, currentWaypoint + 5);
                // for (int i = currentWaypoint; i < min; i++)
                // {
                //     if (Physics2D.OverlapCircle(
                //             path.vectorPath[i],
                //             0.25f,
                //             1 << Globals.layer_detect_player_obj_destroyable) &&
                //         i != path.vectorPath.Count - 1)
                //     {
                //         Entity.Log(new CEntityDebugParam() { Message = "Update path A", Color = EDlogColor.YELLOW });
                //         UpdatePath(finishCoord);
                //         return;
                //     }
                // }
                
                
                // #2 нашли путь короче
                FindNewPath();
            }

            // #2 Направление движения к следующей точке пути
            // Entity.tr.position = Vector2.MoveTowards(Entity.tr.position, targetPosition,
            //     Entity._feature.GetValue(StatId.SpeedMovement) * Time.deltaTime);

            // Проверка расстояния до текущей точки
            if (Vector2.Distance(((_Object_)Entity).Tr.position, targetPosition) < .1f)
            {
                currentWaypoint++;
                
                //Проверка на новое препятствие (блокировка пути)
                if(currentWaypoint < path.vectorPath.Count-1)
                {
                    // if (Physics2D.OverlapCircle(
                    //         path.vectorPath[currentWaypoint],
                    //         0.25f,
                    //         1 << AppConstants.layer_detect_player_obj_destroyable) && 
                    //     currentWaypoint != path.vectorPath.Count - 1)
                    // {
                    //     
                    //     Entity.Log(new CEntityDebugParam() { Message = "Update path B", Color =  EDlogColor.YELLOW });
                    //     UpdatePath(finishCoord); // Обновить путь немедленно
                    // }
                    // else
                    // {
                    //     ReachedPoint();
                    // }
                }
                
                else if (currentWaypoint < path.vectorPath.Count)
                {
                    ReachedPoint();
                }
            }
        }

        private void ReachedPoint()
        {
            targetPosition = path.vectorPath[currentWaypoint];
            Entity.Animation.VisualDirection(targetPosition.x - Entity.CenterRadius.x);
        }


        protected override void Exit()
        {
            Entity.Log(new CEntityDebugParam()
            {
                Message = "State Movement : Exit",
                Color =  EDlogColor.YELLOW
            });
        }

        public override bool CanExit() => true;
        
        #endregion
        
        
        // public Vector2 FindNearestObstacles(
        //     Vector2 center, 
        //     float radius, 
        //     Vector2 target)
        // {
        //     Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius, 1 << AppConstants.layer_detect_player_obj_destroyable);
        //
        //     // foreach (var col in colliders)
        //     // {
        //     //     Vector2 origin = col.bounds.center;
        //     //     Vector2 direction = (target - origin).normalized;
        //     //     float distance = Vector2.Distance(origin, target);
        //     //
        //     //     RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, obstacleMask);
        //     //
        //     // }
        //     DLog.Alert($"Founded colliders "+colliders.Length);
        //     return colliders[Random.Range(0, colliders.Length)].transform.position;
        // }
    }
}