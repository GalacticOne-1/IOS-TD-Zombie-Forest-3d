
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class _Animation
    {
        public Animator animator {get; private set;}

        private GameObject visual;




        public _Animation(_Entity entity)
        {
            // #1
            animator = entity.gameObject.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                DLog.Alert($"Unit : Animator not found [{entity.name}]", EDlogColor.ORANGE);
            }
            
            // #2
            visual = entity.transform.Find("VisualRoot").gameObject;
            if (visual == null)
            {
                Debug.LogError($"Unit : Visual not found [{entity.name}]");
            }
        }
        
        
        
        
        /// <summary>
        /// Направление спрайта 
        /// </summary>
        /// <param name="coordX"></param>
        public virtual void VisualDirection(float coordX)
        {
            //enemRef.controller.sr.flipX = path.vectorPath[currWaypoint].x - enemRef.controller.tr.position.x >= 0;
            visual.transform.rotation = Quaternion.Euler(new Vector3(0, coordX  >= 0 ? 0 : 180, 0));
        }
        
        
        
        #region ANIMATION TRIGGER
        
        public enum EAnimationTriggerType
        {
            Reset, Idle, Movement, Attack, Die
        }

        public void AnimationToggle(EAnimationTriggerType type)
        {
            if(animator)
            {
                switch (type)
                {
                    case EAnimationTriggerType.Reset: // для сброса анимации
                        animator.SetTrigger("Clear");
                        animator.Play("Action", -1, 0);
                        break;

                    case EAnimationTriggerType.Idle:
                        animator.SetInteger("Action", 0);
                        break;

                    case EAnimationTriggerType.Movement:
                        animator.SetInteger("Action", 1);
                        break;

                    case EAnimationTriggerType.Attack:
                        animator.SetTrigger("Attack");
                        break;

                    case EAnimationTriggerType.Die:
                        animator.SetTrigger("Die");
                        break;
                }
            }
        }

        #endregion
    }
}