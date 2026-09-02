using System;
using System.Reflection.PortableExecutable;
using Galactic1.AbstractFactory;
using Galactic1.Repository;
using Galactic1.Gameplay.Player;
using Galactic1.Gameplay.Player.StateMachine;
using Galactic1.Systems;
using UnityEngine;

namespace Galactic1
{
    public abstract class CHARACTER_CONTROLLER
    {
        
        protected PlayerStateMachine _machine;
        
        public float xMove;
        public Vector2 vMove;
        
        
        protected Rigidbody2D rb;
        protected Vector2 position;

        
        protected float timer_step;
        protected bool step_au;
        
        
        
        /// <summary>
        /// Для активации контроллера
        /// </summary>
        public abstract void Enter();
        
        /// <summary>
        /// Для выхода из контроллера
        /// </summary>
        public abstract void Exit();


        

        /// <summary>
        /// Механика движения юнита
        /// </summary>
        /// <param name="x"></param>
        /// <param name="borderX"></param>
        public abstract void Movement(Vector2 borderX, Vector2 borderY);
        public abstract void EndMovement();
        
        public abstract void Jumping();

        public abstract void CheckBorder(Vector2 borderX, Vector2 borderY);

        public abstract void Update_();
        
        
        
        /// <summary>
        /// Если юнит на стоит земле
        /// </summary>
        public virtual void IsGrounded(){}
        
        /// <summary>
        /// Если юнит падает
        /// </summary>
        public virtual void IsFalling(){}

    }

    public class CHARACTER_CONTROLLER_Empty : CHARACTER_CONTROLLER
    {
        public override void Enter(){}

        public override void Exit(){}

        public override void Movement(Vector2 borderX, Vector2 borderY){}
        public override void EndMovement(){}

        public override void Jumping(){}

        public override void CheckBorder(Vector2 borderX, Vector2 borderY){}

        public override void Update_(){}
    }
    
    
    public class CHARACTER_CONTROLLER_Player_Unit : CHARACTER_CONTROLLER
    {
        private bool isGrounded;
        private bool isTouchingWall;
        private bool isWallSliding;
        private bool isWallJumping;
        private bool isFacingRight;
        
        private float wallJumpTimer;
        private float wallJumpDuration = 0.2f;
        private float wallJumpDirection;
        private Vector2 wallJumpPower;
        
        private Transform checkWallRight;
        private Vector2 wallCheckSize;

        private PlayerController _playerUnitController;
        
        
        
        
        public override void Enter()
        {
            _playerUnitController = ServiceLocator.Current.Get<PlayerRepository>().GetController;
            _machine = _playerUnitController.GetComponent<PlayerStateMachine>();
            rb = _playerUnitController.GetComponent<Rigidbody2D>();

            checkWallRight = _playerUnitController.checkWall;
            wallCheckSize = _playerUnitController.wallCheckSize;

            wallJumpPower = new Vector2(12, PlayerControlStatsRepository.wallJumpForce);
            
            // *** для правильного направления спрайта 
            isFacingRight = false;
            ((_Object_)_playerUnitController).Tr.localScale = Vector3.one;
        }

        public override void Exit()
        {
            // сбрасывавем состояние анимации
            _playerUnitController.Animation.animator.Rebind();
            _playerUnitController.Animation.animator.Update(0f); // Force immediate re-evaluation
        }
        
        


        public override void Movement(Vector2 borderX, Vector2 borderY)
        {
            if(_playerUnitController != null)
            {
                if (!isWallJumping)
                    rb.linearVelocity = new Vector2(xMove * PlayerControlStatsRepository.speedMovement, rb.linearVelocity.y);
                
                // *** FLIP
                if(isFacingRight && xMove < 0 || !isFacingRight && xMove > 0)
                {
                    Flip();
                }
                
                // * step sound
                timer_step -= Time.deltaTime;
                if (isGrounded && timer_step < 0)
                {
                    timer_step = .31f;
                    step_au = !step_au;
                    AudioService.Play(step_au
                        ? AudioService.Player.Command.RunStep_1
                        : AudioService.Player.Command.RunStep_2);
                }
            }
        }

        public override void EndMovement()
        {
            timer_step = 0;
        }


        public override void Jumping()
        {
            // прыжок от стены
            if (isWallSliding)
            {
                /*isWallJumping = true;
                wallJumpTimer = wallJumpDuration;
                Vector2 force = new Vector2(-Player2dController.I.horizontal * wallJumpPower.x, wallJumpPower.y);
                if (force.x == 0) force.x = -_PointerHub.player.tr.localScale.x * wallJumpPower.x; // fallback if input = 0
                rb.velocity = Vector2.zero;
                rb.AddForce(force, ForceMode2D.Impulse);*/
                
                isWallJumping = true;
                wallJumpTimer = 0;
                rb.linearVelocity = new Vector2(-wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
                AudioService.Play(AudioService.Player.Command.Jump);

                // wall flip
                if (((_Object_)_playerUnitController).Tr.localScale.x != wallJumpDirection)
                {
                    Flip();
                }
            }
            
            // обычный прыжок
            else if(Physics2D.OverlapCircle(((_Object_)_playerUnitController).Tr.position, 0.2f, 
                        1 << AppConstants.layer_walkable_ground | 1 << AppConstants.layer_obstacle_hard ))
            {
                //HUBController.I.ActionState(HUBController.EControllerFSM.JUMP, () =>
                //{
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, PlayerControlStatsRepository.jumpForce);
                    _playerUnitController.Animation.animator.SetBool("isJumping", true);
                    AudioService.Play(AudioService.Player.Command.Jump);
                //});
            }
        }

        public override void CheckBorder(Vector2 borderX, Vector2 borderY)
        {
            position = ((_Object_)_playerUnitController).Tr.position;
            position.x = Mathf.Clamp(position.x, borderX.x, borderX.y);
            //position.y = Mathf.Clamp(position.y, borderY.x, borderY.y);
            ((_Object_)_playerUnitController).Tr.position = position;
        }

        public override void Update_()
        {
            if(_playerUnitController != null)
            {
                _playerUnitController.Animation.animator.SetFloat("xVelocity", Math.Abs(rb.linearVelocity.x));
                _playerUnitController.Animation.animator.SetFloat("yVelocity", rb.linearVelocity.y);
                
                
                // WALL SLIDING
                isTouchingWall = IsWall();
                isWallSliding = !isGrounded && isTouchingWall && rb.linearVelocity.y < 0;
                
                // сползаем по стене
                if (!isGrounded && isTouchingWall)   
                {
                    _machine.ChangeState(_machine.GetWallSlideState());
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 
                        Mathf.Max(rb.linearVelocity.y, -PlayerControlStatsRepository.wallSlideSpeed));
                    _playerUnitController.Animation.animator.SetBool("touchesWall", true);
                }
                else if (!isTouchingWall)
                {
                    _playerUnitController.Animation.animator.SetBool("touchesWall", false);
                }
                //DLog.Alert($">>> touches wall {isTouchingWall}");
                
                
                

                if (isWallSliding)
                {
                    wallJumpDirection = -((_Object_)_playerUnitController).Tr.localScale.x;
                }
                
                else if (isWallJumping)
                {
                    wallJumpTimer += Time.deltaTime;
                    if (wallJumpTimer > wallJumpDuration)
                    {
                        isWallJumping = false;
                    }
                }

            }
        }

        void Flip()
        {
            isFacingRight = !isFacingRight;
            var ls = ((_Object_)_playerUnitController).Tr.localScale;
            ls.x *= -1;
            ((_Object_)_playerUnitController).Tr.localScale = ls;
        }
        

        public override void IsGrounded()
        {
            isGrounded = true;
            _playerUnitController.Animation.animator.SetBool("isJumping", false);
            _playerUnitController.Animation.animator.SetBool("isFalling", false);
            _machine.ChangeState(_machine.GetIdleState());
            //HUBController.I.ActionState(HUBController.EControllerFSM.JUMP_FINISH);
            _playerUnitController.CapsuleCollider2D.sharedMaterial = ServiceLocator.Current.Get<HubMaterials>().Material2D.normal;
        }

        public override void IsFalling()
        {
            isGrounded = false;
            _playerUnitController.Animation.animator.SetBool("isFalling", true);
            _playerUnitController.CapsuleCollider2D.sharedMaterial = ServiceLocator.Current.Get<HubMaterials>().Material2D.frictionZero;
        }


        bool IsWall()
            => Physics2D.OverlapBox(checkWallRight.position, wallCheckSize,0, 1 << AppConstants.layer_obstacle_hard);
    }
    
    
    
    
    
    public class CHARACTER_CONTROLLER_Player_Dragon : CHARACTER_CONTROLLER
    {
        private float au_step_movement = 0.58f;
        private float au_step_up = 1f;

        private DragonController _dragon;
        
        
        public override void Enter()
        {
            // _dragon = ServiceLocator.Current.Get<DragonRepository>().GetController;
            // _machine = ServiceLocator.Current.Get<PlayerRepository>().GetController.GetComponent<PlayerStateMachine>();
            // rb = _dragon.GetComponent<Rigidbody2D>();
        }
        
        public override void Exit()
        {
            // сбрасывавем состояние анимации
            _dragon.Animation.animator.Rebind();
            _dragon.Animation.animator.Update(0f); // Force immediate re-evaluation

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            //CheckBorder(JoystickController.I.borderX, JoystickController.I.borderY);
        }
        
        public override void Movement(Vector2 borderX, Vector2 borderY)
        {
            if (_dragon != null)
            {
                _dragon.Animation.VisualDirection(vMove.x);
                


                rb.linearVelocity = new Vector2(
                    vMove.x * PlayerControlStatsRepository.speedMovement, 
                    vMove.y * PlayerControlStatsRepository.speedMovement);
                //position = _PointerHub.dragon.tr.position;
                //position += vMove * PlayerSTAT.I.SPEED() * Time.fixedDeltaTime;

                // Clamp the position
                //CheckBorder(borderX, borderY);

                // Move the transform
                //_PointerHub.dragon.tr.position = position;
                
                
                // * sound movement variant
                timer_step -= Time.deltaTime;
                if (Math.Abs(rb.linearVelocity.x) > 2)                      // #1 for horizontal
                {
                    if (timer_step < 0)
                    {
                        timer_step = au_step_movement;
                        AudioService.Play(AudioService.Player.Command.DragonFly);
                    }
                }
                else if (rb.linearVelocity.y > 2)                 // #2 for vertical
                {
                    if (timer_step < 0)
                    {
                        timer_step = au_step_up;
                        AudioService.Play(AudioService.Player.Command.DragonFly);
                    }
                }
            }
        }
        
        public override void EndMovement()
        {
            timer_step = 0;
        }
        
        public override void Jumping(){}

        public override void CheckBorder(Vector2 borderX, Vector2 borderY)
        {
            position = ((_Object_)_dragon).Tr.position;
            position.x = Mathf.Clamp(position.x, borderX.x, borderX.y);
            position.y = Mathf.Clamp(position.y, borderY.x, borderY.y);
            ((_Object_)_dragon).Tr.position = position;
        }

        public override void Update_()
        {
            _dragon.Animation.animator.SetFloat("xVelocity", Math.Abs(rb.linearVelocity.x));
            _dragon.Animation.animator.SetFloat("yVelocity", rb.linearVelocity.y);
        }
    }
}