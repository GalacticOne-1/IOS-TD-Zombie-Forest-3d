using System;
using System.Collections.Generic;
using Galactic1.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1
{
    public class Dragon_GroundDetector : MonoBehaviour
    {

        [SerializeField] private int numberOfRays = 12;
        public Transform tr { get; private set; }


        private CGround[] groundDetected;
        [Serializable]
        public struct CGround
        {
            public GameObject ground;
            public Vector2 triggerHitPoint;             // столкновение с землей
        }
        

        private bool waitCoroutine;


        
        
        

        private void OnTriggerEnter2D(Collider2D other)
        {
            AddGround(new CGround()
            {
                ground = other.gameObject,
                triggerHitPoint = other.COL_TriggerHitPoint(transform.position)
            });
            
            /*
             *      Небольшаая задержка для включения кнопки спешиться
             */
            if (!waitCoroutine)
            {
                ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(.1f, () =>
                {
                    waitCoroutine = false;

                    if (GroundExist())
                    {
                        ServiceLocator.Current.Get<DragonInteractionSystem>().DetectGround(true);
                        
                        //Debug.LogError(other.COL_TriggerHitPoint(transform.position));
                    }
                });
            }
            waitCoroutine = true;
        }


        private void OnTriggerExit2D(Collider2D other)
        {
            RemoveGround(other.gameObject);
            if (!GroundExist())
                ServiceLocator.Current.Get<DragonInteractionSystem>()?.DetectGround(false);
        }
        
        
        
        
        
        private void Awake()
        {
            tr = transform;
            groundDetected = new CGround[10];
        }


        void AddGround(CGround d)
        {
            var l = groundDetected.Length;
            for (int i = 0; i < l; i++)
            {
                if (groundDetected[i].ground == null)
                {
                    groundDetected[i] = d;
                    break;
                }
            }
        }

        void RemoveGround(GameObject g)
        {
            var l = groundDetected.Length;
            for (int i = 0; i < l; i++)
            {
                if (groundDetected[i].ground == g)
                {
                    groundDetected[i].ground = null;
                    break;
                }
            }
        }


        public bool GroundExist()
        {
            var l = groundDetected.Length;
            for (int i = 0; i < l; i++)
                if (groundDetected[i].ground) return true;

            return false;
        }


        
        
        /// <summary>
        /// Расчитывает коорд для высадки юнита на землю
        /// <br/>(return Vector2.zero если не удалось расчитать коорд)
        /// </summary>
        /// <param name="hitPoint">точка столкновения с коллайдером земли</param>
        /// <param name="dismountCoord">для юнита</param>
        public void GetDismountPosition(out Vector2 hitPoint, out Vector2 dismountCoord)
        {
            // *** если не смогли получить координты,
            // отправляем это что бы юнит не выкинуло
            dismountCoord = Vector2.zero;
            hitPoint = Vector2.zero;
            
            
            // find dismount postition
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(1 << AppConstants.layer_walkable_ground);
            filter.useTriggers = true;
            List<Collider2D> colliders = new List<Collider2D>();
            
            if (Physics2D.OverlapCircle(transform.position, GetComponent<CircleCollider2D>().radius, filter, colliders) > 0)
            {
                tr.position.FIND_NearestCoord_ID(colliders, out int id, out hitPoint);
                // что бы юнит по возможности не застревал между коллайдерами, скидываем его выше земли
                dismountCoord = hitPoint;
                dismountCoord.y += 1f;      
                //Debug.LogError($"*** colliders {colliders.Count}");
            }
        }
        
        
        /*public Vector2 GetDismountPosition()
        {
            List<Vector2> coord = new List<Vector2>();
            
            var radius = GetComponent<CircleCollider2D>().radius;
            RaycastHit2D hit;
            float angleStep = 360f / numberOfRays;
            float angle;
            Vector2 direction, c;

            // #1 пускаем лучи для обнаружения земли
            for (int i = 0; i < numberOfRays; i++)
            {
                angle = i * angleStep * Mathf.Deg2Rad;
                direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                hit = tr.position.Ray2d(direction, radius, 1 << Globals.layer_ground);

                if (hit.collider)
                {
                    //DLog.Alert($"Hit {hit.collider.name} at {hit.point}");
                    c = hit.point;
                    c.y = hit.collider.GetComponent<IGroundY>().Y + 1;
                    coord.Add(c);
                }
            }

            // #1.1 finding nearest hit point
            if (coord.Count > 0)
            {
                DLog.Alert(">>> dismount on raycast", EDlogColor.YELLOW);
                tr.position.FIND_NearestCoord_ID(coord, out int id);
                return coord[id];
            }
            
            
            // #2 если луч не попал в землю, делаем расчет через коллайдер
            // for (int i = 0; i < 10; i++)
            // {
            //     if (groundDetected[i].ground)
            //     {
            //         groundDetected[i].triggerHitPoint.y = groundDetected[i].ground.GetComponent<IGroundY>().Y;
            //         DLog.Alert($">>> dismount on ground collider {groundDetected[i].triggerHitPoint}", EDlogColor.YELLOW);
            //         Debug.LogError("***");
            //         return groundDetected[i].triggerHitPoint;
            //     }
            // }

            var hitCollider = Physics2D.OverlapCircle(transform.position, radius, 1 << Globals.layer_ground);
            if (hitCollider)
            {
                
            }
            
            // *** если не смогли получить координты,
            // отправляем это что бы юнит не выкинуло
            return Vector2.zero;
        }*/
    }
}