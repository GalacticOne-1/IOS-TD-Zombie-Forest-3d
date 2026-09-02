using Galactic1.Code.Gameplay.Animation.Player;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Systems.Squad;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation
{
    /// <summary>
    /// Центральная точка управления анимацией юнита.
    ///
    /// Все системы (Movement, Combat, Interaction, Ability) вызывают
    /// только методы этого класса.
    /// Никто не трогает Animator напрямую.
    ///
    /// Слои Animator Controller:
    ///   Layer 0 — Locomotion  (Idle, Walk, Run, Crouch)
    ///   Layer 1 — Combat      (Aim, Shoot, Reload, Cover)  — additive
    ///   Layer 2 — Action      (Interact, Panic, Die)       — override
    /// </summary>
    [RequireComponent(typeof(AnimatorBridge))]
    [RequireComponent(typeof(UnitMover))]
    public sealed class UnitAnimationController : MonoBehaviour
    {
        // =========================
        // Deps
        // =========================
        private BaseAnimConfig _config;
        private AnimatorBridge _bridge;
        private IAttackAnimationModule _attackModule;
        private IWeaponAnimationModule _weaponAnimation;
        private IWeaponAnimatorSwitcher _weaponSwitcher;
        private IAbilityAnimationModule _abilityModule;
        private ICombatAnimationModule _combatModule;

        // =========================
        // State
        // =========================
        private bool _isDead;

        /// For dev !!!
        public void Restore()
        {
            _isDead = false;
            _bridge.ResetAnimator();
        }


        // =========================
        // Init
        // =========================
        public void Initialize(BaseAnimConfig config)
        {
            _config = config;
            _bridge = GetComponent<AnimatorBridge>();
            _bridge.Bind();
            _attackModule = GetComponent<IAttackAnimationModule>();
            
            _weaponAnimation = GetComponent<IWeaponAnimationModule>();
            _weaponSwitcher = GetComponent<IWeaponAnimatorSwitcher>();
            _abilityModule = GetComponent<IAbilityAnimationModule>();
            _combatModule = GetComponent<ICombatAnimationModule>();
        }

        // =========================
        // Tick
        // =========================
        private bool closeDev;

        // public void Tick()
        // {
        //     if (closeDev || _isDead)
        //         return;
        //
        //     UpdateLocomotion();
        // }


        /// <summary>
        /// Вызывается из EquipmentSystem или InventorySystem при подборе/смене оружия.
        /// </summary>
        public void SetWeapon(WeaponType type)
        {
            if (_isDead) 
                return;
            _weaponSwitcher?.SetWeapon(type);
        }

        // =========================
        // Locomotion API
        // =========================


        public void SetCrouching(bool isCrouching)
            => _bridge.SetBool(_config.IsCrouchingHash, isCrouching);

        
        
        // =========================
        // Combat API
        // =========================
        
        /// <summary>
        /// Анимации для способностей (граната, аптечка и пр)
        /// </summary>
        /// <param name="ctx"></param>
        public void OnAbilityAnimation(ItemUseContext ctx)
        {
            // switch (ctx.AnimationType)
            // {
            //     case AbilityAnimationType.TossGrenade:
            //         PlayTossGrenade();
            //         break;
            // }
            _abilityModule?.OnAbilityAnimation(ctx);
        }
        
        
        
        // public void SetAiming(bool isAiming)
        //     => _bridge.SetBool(_config.IsAimingHash, isAiming);

        /// <summary>
        /// Вызывается из CombatSystem при команде атаки.
        /// </summary>
        public void PlayShoot()
        {
            if (_isDead) return;
            //_bridge.SetTrigger(_config.ShootHash);
            _combatModule?.PlayShoot();
        }
        
        /// <summary>
        /// Вызывается из WeaponAnimBridge при выстреле.
        /// </summary>
        public void PlayRangedAttack()
        {
            if (_isDead) return;
            _attackModule.PlayAttack();
        }
        
        public void PlayMeleeAttack()
        {
            if (_isDead) return;
            _attackModule.PlayMeleeAttack();
        }
        
        // void PlayTossGrenade()
        // {
        //     if (_isDead) return;
        //     
        //     _weaponAnimation?.SetRigEnabled(false);
        //     _weaponAnimation?.SetWeaponVisible(false);
        //     _bridge.SetTrigger(_config.GrenadeHash);
        // }

        /// <summary>
        /// Для FullAuto — держит bool пока стреляем.
        /// </summary>
        public void SetFiring(bool isFiring)
        {
            if (_isDead) return;
            //_bridge.SetBool(_config.IsFiringHash, isFiring);
            _combatModule?.SetFiring(isFiring);
        }

        public void PlayReload()
        {
            if (_isDead) return;
            //_bridge.SetTrigger(_config.ReloadHash);
            _combatModule?.PlayReload();
        }
        public void CancelReload()
        {
            if (_isDead) return;
            _bridge.CrossFade(_config.IdleStateName, 0.1f, 0);
            _bridge.PlayState("Empty", 1);
        }

        // public void SetInCover(bool inCover)
        //     => _bridge.SetBool(_config.IsInCoverHash, inCover);

        /// <summary>
        /// Для выхода из боевых клипов -> idle state
        /// </summary>
        public void CombatExit()
        {
            if (_isDead) return;
            _bridge.CrossFade(_config.IdleStateName, 0.1f, 0);
            _bridge.PlayState("Empty", 2);
        }
        
        
        
        // =========================
        // Action API
        // =========================
        public void PlayInteract()
        {
            if (_isDead) return;
            //_bridge.SetTrigger(_config.InteractHash);
            _combatModule?.PlayInteract();
        }

        public void PlayHit()
        {
            if (_isDead) return;
            _bridge.SetTrigger(_config.HitHash);
        }

        public void PlayDeath()
        {
            if (_isDead) return;
            _isDead = true;
            _bridge.SetFloat(_config.SpeedHash, 0f, 0f);
            _bridge.SetBool(_config.IsMovingHash, false);
            //_bridge.SetBool(_config.IsAimingHash, false);
            _bridge.SetTrigger(_config.DieHash);
        }

        public void SetPanicking(bool isPanicking)
            => _bridge.SetBool(_config.PanicHash, isPanicking);
        
        
        public void EndGrenadeToss()
        {
            if (_isDead)
                return;

            _abilityModule?.EndGrenadeToss();
            // // без корутины баг !
            // ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait1(() => _weaponAnimation?.SetRigEnabled(true));
            // _weaponAnimation?.SetWeaponVisible(true);
        }

        // =========================
        // Private
        // =========================
        // private void UpdateLocomotion()
        // {
        //     bool isMoving = _mover.IsMoving;
        //     _bridge.SetBool(_config.IsMovingHash, isMoving);
        //
        //     float actualSpeed = _mover.Velocity.magnitude;
        //     _bridge.SetFloat(_config.SpeedHash, actualSpeed);
        // }
        
        
        
        // =========================
        // DEV
        // =========================
        public void SetLocomotionDev(float speed)
        {
            closeDev = true;
            _bridge.SetBool(_config.IsMovingHash, true);
            _bridge.SetFloat(_config.SpeedHash, speed, 0);
        }

        public void StopLocomotion()
        {
            closeDev = false;
            _bridge.SetBool(_config.IsMovingHash, false);
            _bridge.SetFloat(_config.SpeedHash, 0);
        }
    }
}