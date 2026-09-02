
using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Animation.Zombie;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Weapon.Animation;
using Galactic1.Code.Systems.Squad;
using Galactic1.Core.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.Gameplay.Animation
{
    /// <summary>
    /// Debug UI для ручного тестирования анимаций юнита.
    /// Используется только в сцене разработчика.
    /// </summary>
    public sealed class AnimationDebugPanel : MonoBehaviour
    {
        [SerializeField] private bool isActive;

        [Header("UI")] 
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject resetAnimButton;
        [SerializeField] private Transform unitStatusButtonRoot;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject buttonPrefab;

        [Header("Buttons")] 
        [SerializeField] private bool debugEnemy;
        [SerializeField] private List<AnimationDebugButton> buttons = new();
        [SerializeField] private List<AnimationDebugButton> buttonsEnemy = new();
        
        private UnitAnimationController _animationController;
        private WeaponRigController _rigController;
        private ZombieLocomotionAnimationModule _zombieLocomotion;
        private ZombieDeathAnimationModule _zombieDeath;
        private ZombieAttackAnimationModule _zombieAttack;

        private float speedWalk, speedRun;
        private bool _crouch;
        private bool _aim;


        
        
        
        private void Start()
        {
            if (!isActive)
            {
                panel.SetActive(false);
                return;
            }
            panel.SetActive(true);
            
            CheckUnit();
            
            resetAnimButton.RegisterButtonClick(ResetAnimator);

            var _buttons = debugEnemy ? buttonsEnemy : buttons;

            foreach (var data in _buttons)
            {
                var button = Instantiate(buttonPrefab, data.scrollRoot ? scrollRect.content : unitStatusButtonRoot);

                button.GetComponentInChildren<TextMeshProUGUI>().text = data.label;

                button.RegisterButtonClick(() => Execute(data.action));
            }
            
            scrollRect.SetSizeContentLayoutGroup(false);
            scrollRect.ScrollRectResetH();
        }
        
        

        void CheckUnit()
        {
            if (_animationController == null)
            {
                _animationController = FindAnyObjectByType<UnitAnimationController>();
                if(_animationController)
                {
                    speedWalk = _animationController.GetComponent<UnitMover>().WalkSpeed;
                    speedRun = _animationController.GetComponent<UnitMover>().RunSpeed;

                    _rigController = _animationController.GetComponent<WeaponRigController>();
                    _zombieLocomotion = _animationController.GetComponent<ZombieLocomotionAnimationModule>();
                    _zombieDeath = _animationController.GetComponent<ZombieDeathAnimationModule>();
                    _zombieAttack = _animationController.GetComponent<ZombieAttackAnimationModule>();
                }
                
            }
        }
        
        public void ResetAnimator()
        {
            _animationController.GetComponent<SurvivorInstance>().Entity_Reset(false);
        }

        // =========================
        // Action dispatcher
        // =========================

        private void Execute(AnimationDebugAction action)
        {
            CheckUnit();
            if (_animationController == null) return;

            switch (action)
            {
                case AnimationDebugAction.Walk:
                    //_animationController.SetMoveMode(WorldInputDispatcher.MoveMode.Walk);
                    _animationController.SetLocomotionDev(speedWalk);
                    break;

                case AnimationDebugAction.Run:
                    //_animationController.SetMoveMode(WorldInputDispatcher.MoveMode.Run);
                    _animationController.SetLocomotionDev(speedRun);
                    break;

                case AnimationDebugAction.Stop:
                    _animationController.StopLocomotion();
                    break;

                case AnimationDebugAction.ToggleCrouch:
                    _crouch = !_crouch;
                    _animationController.SetCrouching(_crouch);
                    break;

                case AnimationDebugAction.ToggleAim:
                    _aim = !_aim;
                    //_animationController.SetAiming(_aim);
                    break;

                case AnimationDebugAction.Shoot:
                    _animationController.PlayShoot();
                    break;
                case AnimationDebugAction.Greande:
                    _animationController.OnAbilityAnimation(new ItemUseContext()
                    { AnimationType = AbilityAnimationType.TossGrenade });
                    break;

                case AnimationDebugAction.Reload:
                    _animationController.PlayReload();
                    break;

                case AnimationDebugAction.Interact:
                    _animationController.PlayInteract();
                    break;

                case AnimationDebugAction.Hit:
                    _animationController.PlayMeleeAttack();
                    break;

                case AnimationDebugAction.Die:
                    _animationController.GetComponent<WeaponRigController>().DetachWeapon();
                    _animationController.PlayDeath();
                    break;
                case AnimationDebugAction.DieRelease:
                    _animationController.GetComponent<SurvivorInstance>().Entity_Die();
                    break;

                case AnimationDebugAction.WeaponNone:
                    _rigController.DetachWeapon();
                    _animationController.SetWeapon(WeaponType.Unarmed);
                    break;

                case AnimationDebugAction.WeaponPistol:
                    _animationController.SetWeapon(WeaponType.Pistol);
                    break;

                case AnimationDebugAction.WeaponRifle:
                    _animationController.SetWeapon(WeaponType.Rifle);
                    break;
                
                
                
                
                // case AnimationDebugAction.ZombieIdle0:
                //     _zombieLocomotion?.Debug_PlayIdle(0);
                //     break;
                //
                // case AnimationDebugAction.ZombieIdle1:
                //     _zombieLocomotion?.Debug_PlayIdle(1);
                //     break;
                //
                // case AnimationDebugAction.ZombieIdle2:
                //     _zombieLocomotion?.Debug_PlayIdle(2);
                //     break;
                //
                // case AnimationDebugAction.ZombieWalk0:
                //     _zombieLocomotion?.Debug_PlayWalk(0);
                //     break;
                //
                // case AnimationDebugAction.ZombieWalk1:
                //     _zombieLocomotion?.Debug_PlayWalk(1);
                //     break;
                //
                // case AnimationDebugAction.ZombieWalk2:
                //     _zombieLocomotion?.Debug_PlayWalk(2);
                //     break;
                //
                // case AnimationDebugAction.ZombieRun0:
                //     _zombieLocomotion?.Debug_PlayRun(0);
                //     break;
                //
                // case AnimationDebugAction.ZombieRun1:
                //     _zombieLocomotion?.Debug_PlayRun(1);
                //     break;
                //
                // case AnimationDebugAction.ZombieRun2:
                //     _zombieLocomotion?.Debug_PlayRun(2);
                //     break;

                case AnimationDebugAction.ZombieDebugStop:
                    _zombieLocomotion.Debug_Stop();
                    break;
                
                
                case AnimationDebugAction.ZombieDeathReset:
                    _zombieDeath.ResetDeath();
                    break;
                case AnimationDebugAction.ZombieDeath0:
                    _zombieDeath.ForceDeath(0);
                    break;
                case AnimationDebugAction.ZombieDeath1:
                    _zombieDeath.ForceDeath(1);
                    break;
                case AnimationDebugAction.ZombieDeath2:
                    _zombieDeath.ForceDeath(2);
                    break;
                
                case AnimationDebugAction.ZombieAttack0:
                    _zombieAttack.Debug_PlayAttack(0);
                    break;
                case AnimationDebugAction.ZombieAttack1:
                    _zombieAttack.Debug_PlayAttack(1);
                    break;
                case AnimationDebugAction.ZombieAttack2:
                    _zombieAttack.Debug_PlayAttack(2);
                    break;
            }
        }

        private void OnDrawGizmos()
        {
            if (_animationController == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawRay(_animationController.transform.position + Vector3.up, _animationController.transform.forward * 2f);
        }

    }
    
    [Serializable]
    public class AnimationDebugButton
    {
        public string label;
        public bool scrollRoot;
        public AnimationDebugAction action;
    }

    public enum AnimationDebugAction
    {
        Stop = 1,
        Walk = 2,
        Run = 3,
        
        Die = 10,
        DieRelease = 11,
        
        ToggleCrouch = 30,
        ToggleAim = 31,
        Shoot = 32,
        Reload = 33,
        Interact = 34,
        Hit = 35,
        Greande = 36,
        
        
        WeaponNone = 50,
        WeaponPistol = 51,
        WeaponRifle = 52,
        
        
        // ====== ZOMBIE ======
        
        ZombieIdle0 = 100,
        ZombieIdle1 = 101,
        ZombieIdle2 = 102,

        ZombieWalk0 = 110,
        ZombieWalk1 = 111,
        ZombieWalk2 = 112,

        ZombieRun0 = 120,
        ZombieRun1 = 121,
        ZombieRun2 = 122,
        
        
        ZombieDeath0 = 130,
        ZombieDeath1 = 131,
        ZombieDeath2 = 132,
        ZombieDeathReset = 135,
        
        ZombieAttack0 = 150,
        ZombieAttack1 = 151,
        ZombieAttack2 = 152,

        ZombieDebugStop = 200,
    }
}